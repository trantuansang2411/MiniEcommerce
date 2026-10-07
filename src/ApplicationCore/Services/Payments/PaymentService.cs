using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Payments;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Orders;
using ApplicationCore.Interfaces.Payments;
using ApplicationCore.Interfaces.Warehousing;
using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Entities.Fulfillment;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services.Payments;

public sealed class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ILogger<PaymentService> _logger;
    private readonly IInventoryReservationRepository _reservationRepository;
    private readonly IRepository<Shipment> _shipmentRepository;

    public PaymentService(
        IOrderRepository orderRepository,
        IRepository<Payment> paymentRepository,
        IInventoryReservationRepository reservationRepository,
        IRepository<Shipment> shipmentRepository,
        ILogger<PaymentService> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
        _shipmentRepository = shipmentRepository;
        _logger = logger;
    }

    public async Task<Payment> PayOrderManuallyAsync(Guid userId, Guid orderId)
    {
        EnsureIds(userId, orderId);

        var order = await _orderRepository.GetForPaymentByIdForUserAsync(orderId, userId)
            ?? throw new NotFoundException("Order not found.");

        if (order.Status == OrderStatus.Expired || order.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            if (order.Status == OrderStatus.AwaitingPayment)
            {
                order.MarkAsExpired();
                await _orderRepository.SaveChangesAsync();
            }

            throw new ConflictException("Order payment deadline has expired.");
        }

        if (order.Status != OrderStatus.AwaitingPayment)
            throw new ConflictException("Order cannot be paid in its current status.");

        if (order.Payments.Any(x => x.Status == PaymentStatus.Succeeded))
            throw new ConflictException("Order has already been paid.");

        var payment = new Payment(order.Id, order.TotalAmount, PaymentMethod.Manual);
        payment.MarkAsSucceeded($"manual_{Guid.NewGuid():N}");
        order.MarkAsPaid();

        var reservations = await _reservationRepository.GetActiveByOrderIdAsync(order.Id);
        if (reservations.Count == 0)
            throw new ConflictException("Order has no active inventory reservations.");

        foreach (var group in reservations.GroupBy(x => x.Inventory.WarehouseId))
        {
            var shipment = new Shipment(order.Id, group.Key);
            foreach (var reservation in group)
                shipment.AddItem(reservation.OrderItemId, reservation.Inventory.ProductId, reservation.Quantity);
            _shipmentRepository.Add(shipment);
        }

        _paymentRepository.Add(payment);
        await _paymentRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Manual payment {PaymentId} succeeded for order {OrderId} and user {UserId}",
            payment.Id,
            order.Id,
            userId);

        return payment;
    }

    private static void EnsureIds(Guid userId, Guid orderId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");

        if (orderId == Guid.Empty)
            throw new BadRequestException("OrderId is required.");
    }
}
