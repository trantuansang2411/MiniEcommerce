using ApplicationCore.Entities.Catalog;
using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Shopping;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Orders;
using ApplicationCore.Interfaces.Shopping;
using ApplicationCore.Interfaces.Warehousing;
using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Entities.Shipping;
using ApplicationCore.Interfaces.Shipping;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services.Orders;

public sealed class OrderService : IOrderService
{
    private static readonly TimeSpan PaymentDeadline = TimeSpan.FromMinutes(15);

    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryReservationRepository _reservationRepository;
    private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
    private readonly IShippingProfileRepository _shippingProfileRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IRepository<CartItem> cartItemRepository,
        IInventoryRepository inventoryRepository,
        IInventoryReservationRepository reservationRepository,
        IRepository<InventoryTransaction> inventoryTransactionRepository,
        IShippingProfileRepository shippingProfileRepository,
        ILogger<OrderService> logger)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _cartItemRepository = cartItemRepository;
        _inventoryRepository = inventoryRepository;
        _reservationRepository = reservationRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _shippingProfileRepository = shippingProfileRepository;
        _logger = logger;
    }

    public async Task<Order> CheckoutAsync(Guid userId, CheckoutShippingInfo shippingInfo)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");

        ArgumentNullException.ThrowIfNull(shippingInfo);
        var deliveryDetails = await ResolveDeliveryDetailsAsync(userId, shippingInfo);

        var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId)
            ?? throw new BadRequestException("Cart is empty.");

        if (cart.Items.Count == 0)
            throw new BadRequestException("Cart is empty.");

        EnsureAllItemsAreAvailable(cart);

        var totalAmount = cart.Items.Sum(item => item.Product.SellingPrice * item.Quantity);
        var order = new Order(userId, totalAmount, DateTimeOffset.UtcNow.Add(PaymentDeadline));
        order.SetDeliveryDetails(
            deliveryDetails.RecipientName,
            deliveryDetails.RecipientPhone,
            deliveryDetails.ShippingAddress,
            shippingInfo.DeliveryNote);

        foreach (var cartItem in cart.Items)
        {
            // Chụp giá hiện tại vào OrderItem để lịch sử đơn không đổi khi Product đổi giá.
            order.AddItem(cartItem.ProductId, cartItem.Quantity, cartItem.Product.SellingPrice);
        }

        _orderRepository.Add(order);

        foreach (var cartItem in cart.Items)
        {
            var inventory = await _inventoryRepository.GetAvailableForProductAsync(cartItem.ProductId, cartItem.Quantity)
                ?? throw new ConflictException($"Insufficient inventory for product {cartItem.Product.Name}.");

            try { inventory.Reserve(cartItem.Quantity); }
            catch (InvalidOperationException exception) { throw new ConflictException(exception.Message); }

            var orderItem = order.Items.Single(x => x.ProductId == cartItem.ProductId);
            _reservationRepository.Add(new InventoryReservation(inventory.Id, order.Id, orderItem.Id, cartItem.Quantity, order.ExpiresAt));
            _inventoryTransactionRepository.Add(new InventoryTransaction(inventory.Id, InventoryTransactionType.Reservation, cartItem.Quantity, userId));
        }

        foreach (var cartItem in cart.Items.ToList())
        {
            _cartItemRepository.Delete(cartItem);
        }

        // Các repository cùng dùng một AppDbContext (Scoped), nên SaveChanges này:
        // INSERT Order + OrderItems và DELETE CartItems trong cùng transaction của EF Core.
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} was created from cart {CartId} for user {UserId}",
            order.Id,
            cart.Id,
            userId);

        return order;
    }

    private async Task<(string RecipientName, string RecipientPhone, string ShippingAddress)> ResolveDeliveryDetailsAsync(
        Guid userId,
        CheckoutShippingInfo shippingInfo)
    {
        if (shippingInfo.ShippingProfileId is { } profileId)
        {
            var profile = await _shippingProfileRepository.GetByIdForUserAsync(profileId, userId)
                ?? throw new NotFoundException("Shipping profile not found.");

            if (shippingInfo.SetAsDefault && !profile.IsDefault)
            {
                await _shippingProfileRepository.ClearDefaultForUserAsync(userId, profile.Id);
                profile.SetDefault(true);
            }

            return (profile.RecipientName, profile.RecipientPhone, profile.ShippingAddress);
        }

        if (string.IsNullOrWhiteSpace(shippingInfo.RecipientName) ||
            string.IsNullOrWhiteSpace(shippingInfo.RecipientPhone) ||
            string.IsNullOrWhiteSpace(shippingInfo.ShippingAddress))
        {
            throw new BadRequestException("Recipient name, phone, and delivery address are required.");
        }

        if (shippingInfo.SaveAsProfile)
        {
            if (shippingInfo.SetAsDefault)
                await _shippingProfileRepository.ClearDefaultForUserAsync(userId);

            _shippingProfileRepository.Add(new ShippingProfile(
                userId,
                shippingInfo.RecipientName,
                shippingInfo.RecipientPhone,
                shippingInfo.ShippingAddress,
                shippingInfo.SetAsDefault));
        }

        return (shippingInfo.RecipientName.Trim(), shippingInfo.RecipientPhone.Trim(), shippingInfo.ShippingAddress.Trim());
    }

    public async Task<IReadOnlyList<Order>> GetMyOrdersAsync(Guid userId)
    {
        EnsureUserId(userId);
        return await _orderRepository.GetByUserIdAsync(userId);
    }

    public async Task<Order> GetMyOrderByIdAsync(Guid userId, Guid orderId)
    {
        EnsureUserId(userId);

        if (orderId == Guid.Empty)
            throw new BadRequestException("OrderId is required.");

        // Điều kiện UserId nằm trong query để không lộ đơn hàng của người khác.
        return await _orderRepository.GetByIdForUserAsync(orderId, userId)
            ?? throw new NotFoundException("Order not found.");
    }

    public Task<IReadOnlyList<Order>> GetAllForManagementAsync(OrderStatus? status) =>
        _orderRepository.GetAllForManagementAsync(status);

    public async Task<Order> GetManagementDetailAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new BadRequestException("OrderId is required.");

        return await _orderRepository.GetManagementDetailAsync(orderId)
            ?? throw new NotFoundException("Order not found.");
    }

    public async Task CancelAsync(Guid userId, Guid orderId)
    {
        EnsureUserId(userId);
        var order = await _orderRepository.GetForUpdateByIdForUserAsync(orderId, userId)
            ?? throw new NotFoundException("Order not found.");

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        var reservations = await _reservationRepository.GetActiveByOrderIdAsync(order.Id);
        foreach (var reservation in reservations)
        {
            reservation.Inventory.ReleaseReservation(reservation.Quantity);
            reservation.Release();
            _inventoryTransactionRepository.Add(new InventoryTransaction(
                reservation.InventoryId,
                InventoryTransactionType.ReservationRelease,
                -reservation.Quantity,
                userId));
        }

        await _orderRepository.SaveChangesAsync();
        _logger.LogInformation("Order {OrderId} was cancelled by user {UserId}", order.Id, userId);
    }

    private static void EnsureAllItemsAreAvailable(Cart cart)
    {
        var unavailableItem = cart.Items.FirstOrDefault(item =>
            item.Product is null ||
            item.Product.Status != ProductStatus.Active ||
            item.Product.Category is null ||
            !item.Product.Category.IsActive);

        if (unavailableItem is not null)
            throw new ConflictException("One or more products in the cart are unavailable. Please update your cart and try again.");
    }

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");
    }
}
