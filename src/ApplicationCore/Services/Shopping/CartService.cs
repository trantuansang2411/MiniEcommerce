using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Catalog;
using ApplicationCore.Interfaces.Shopping;
using Microsoft.Extensions.Logging;
using ApplicationCore.Entities.Shopping;

namespace ApplicationCore.Services.Shopping;

public sealed class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CartService> _logger;

    public CartService(
        ICartRepository cartRepository,
        IRepository<CartItem> cartItemRepository,
        IProductRepository productRepository,
        ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public Task<Cart?> GetMyCartAsync(Guid userId)
    {
        EnsureUserId(userId);
        return _cartRepository.GetByUserIdWithItemsAsync(userId);
    }

    public async Task<Cart> AddItemAsync(Guid userId, Guid productId, int quantity)
    {
        EnsureUserId(userId);
        EnsureQuantity(quantity);
        await EnsureProductIsAvailableAsync(productId);

        var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId);
        if (cart is null)
        {
            cart = new Cart(userId);
            cart.AddItem(productId, quantity);
            _cartRepository.Add(cart);
        }
        else
        {
            var isNewItem = cart.Items.All(x => x.ProductId != productId);
            cart.AddItem(productId, quantity);

            if (isNewItem)
            {
                var newItem = cart.Items.Single(x => x.ProductId == productId);
                _cartItemRepository.Add(newItem);
            }
        }

        await _cartRepository.SaveChangesAsync();
        _logger.LogInformation("Product {ProductId} was added to cart {CartId} for user {UserId}", productId, cart.Id, userId);

        // CartItem mới chỉ có ProductId; query lại để nạp Product/Category cho response.
        return await _cartRepository.GetByUserIdWithItemsAsync(userId)
            ?? throw new InvalidOperationException("Cart could not be reloaded after adding an item.");
    }

    public async Task<Cart> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity)
    {
        EnsureUserId(userId);
        EnsureQuantity(quantity);

        var cart = await GetRequiredCartAsync(userId);
        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId)
            ?? throw new NotFoundException("Product is not in the cart.");

        await EnsureProductIsAvailableAsync(productId);
        item.ChangeQuantity(quantity);
        cart.Touch();

        await _cartRepository.SaveChangesAsync();
        _logger.LogInformation("Cart item for product {ProductId} was updated in cart {CartId}", productId, cart.Id);

        return cart;
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        EnsureUserId(userId);

        var cart = await GetRequiredCartAsync(userId);
        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId)
            ?? throw new NotFoundException("Product is not in the cart.");

        cart.RemoveItem(item);
        _cartItemRepository.Delete(item);
        await _cartRepository.SaveChangesAsync();
        _logger.LogInformation("Product {ProductId} was removed from cart {CartId}", productId, cart.Id);
    }

    private async Task<Cart> GetRequiredCartAsync(Guid userId)
    {
        return await _cartRepository.GetByUserIdWithItemsAsync(userId)
            ?? throw new NotFoundException("Cart not found.");
    }

    private async Task EnsureProductIsAvailableAsync(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new BadRequestException("ProductId is required.");

        var product = await _productRepository.GetActiveByIdAsync(productId);
        if (product is null)
            throw new NotFoundException("Product is unavailable.");
    }

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");
    }

    private static void EnsureQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BadRequestException("Quantity must be greater than zero.");
    }
}
