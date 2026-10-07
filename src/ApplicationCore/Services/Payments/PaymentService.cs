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
    private readonly IPaymentRepository _payments;
    private readonly IVnPayGateway _vnPay;
    private readonly ILogger<PaymentService> _logger;
    private readonly IInventoryReservationRepository _reservationRepository;
    private readonly IRepository<Shipment> _shipmentRepository;

    public PaymentService(
        IOrderRepository orderRepository,
        IRepository<Payment> paymentRepository,
        IPaymentRepository payments,
        IVnPayGateway vnPay,
        IInventoryReservationRepository reservationRepository,
        IRepository<Shipment> shipmentRepository,
        ILogger<PaymentService> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _payments = payments;
        _vnPay = vnPay;
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
        await CompletePaymentAsync(payment, order, $"manual_{Guid.NewGuid():N}");

        _paymentRepository.Add(payment);
        await _paymentRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Manual payment {PaymentId} succeeded for order {OrderId} and user {UserId}",
            payment.Id,
            order.Id,
            userId);

        return payment;
    }

    public async Task<VnPayPaymentSession> CreateVnPayPaymentAsync(Guid userId, Guid orderId, string clientIp)
    {
        EnsureIds(userId, orderId);
        var order = await _orderRepository.GetForPaymentByIdForUserAsync(orderId, userId)
            ?? throw new NotFoundException("Order not found.");
        EnsureOrderCanBePaid(order);

        var payment = new Payment(order.Id, order.TotalAmount, PaymentMethod.VnPay);
        var paymentUrl = _vnPay.CreatePaymentUrl(new(payment.Id, order.Id, payment.Amount, order.ExpiresAt, clientIp));
        _paymentRepository.Add(payment);
        await _paymentRepository.SaveChangesAsync();

        return new VnPayPaymentSession(payment.Id, paymentUrl);
    }

    public async Task<VnPayIpnResponse> ProcessVnPayIpnAsync(IReadOnlyDictionary<string, string> parameters)
    {
        if (!_vnPay.IsValidSignature(parameters)) return new("97", "Invalid signature");
        if (!parameters.TryGetValue("vnp_TxnRef", out var txnRef) || !Guid.TryParseExact(txnRef, "N", out var paymentId)) return new("01", "Payment not found");
        var payment = await _payments.GetWithOrderAsync(paymentId);
        if (payment is null || payment.Method != PaymentMethod.VnPay) return new("01", "Payment not found");
        if (payment.Status == PaymentStatus.Succeeded) return new("00", "Confirm Success");
        if (payment.Status != PaymentStatus.Pending) return new("02", "Payment already confirmed");
        if (!parameters.TryGetValue("vnp_Amount", out var rawAmount) || !long.TryParse(rawAmount, out var amount) || amount != decimal.ToInt64(payment.Amount * 100m)) return new("04", "Invalid amount");

        var succeeded = parameters.TryGetValue("vnp_ResponseCode", out var responseCode) && responseCode == "00" && parameters.TryGetValue("vnp_TransactionStatus", out var transactionStatus) && transactionStatus == "00";
        if (!succeeded)
        {
            payment.MarkAsFailed();
            await _payments.SaveChangesAsync();
            return new("00", "Confirm Success");
        }

        if (!parameters.TryGetValue("vnp_TransactionNo", out var transactionId) || string.IsNullOrWhiteSpace(transactionId)) return new("99", "Missing transaction id");
        try { await CompletePaymentAsync(payment, payment.Order, transactionId); }
        catch (InvalidOperationException) { return new("02", "Payment already confirmed"); }
        await _payments.SaveChangesAsync();
        _logger.LogInformation("VNPay payment {PaymentId} succeeded for order {OrderId}", payment.Id, payment.OrderId);
        return new("00", "Confirm Success");
    }

    public async Task<Payment> GetPaymentForUserAsync(Guid userId, Guid paymentId)
    {
        if (userId == Guid.Empty) throw new UnauthorizedException("User is not authenticated.");
        return await _payments.GetWithOrderForUserAsync(paymentId, userId) ?? throw new NotFoundException("Payment not found.");
    }

    private async Task CompletePaymentAsync(Payment payment, Order order, string transactionId)
    {
        payment.MarkAsSucceeded(transactionId);
        order.MarkAsPaid();
        var reservations = await _reservationRepository.GetActiveByOrderIdAsync(order.Id);
        if (reservations.Count == 0) throw new ConflictException("Order has no active inventory reservations.");
        foreach (var group in reservations.GroupBy(x => x.Inventory.WarehouseId))
        {
            var shipment = new Shipment(order.Id, group.Key);
            foreach (var reservation in group) shipment.AddItem(reservation.OrderItemId, reservation.Inventory.ProductId, reservation.Quantity);
            _shipmentRepository.Add(shipment);
        }
    }

    private static void EnsureOrderCanBePaid(Order order)
    {
        if (order.Status == OrderStatus.Expired || order.ExpiresAt <= DateTimeOffset.UtcNow) throw new ConflictException("Order payment deadline has expired.");
        if (order.Status != OrderStatus.AwaitingPayment) throw new ConflictException("Order cannot be paid in its current status.");
        if (order.Payments.Any(x => x.Status == PaymentStatus.Succeeded)) throw new ConflictException("Order has already been paid.");
    }

    private static void EnsureIds(Guid userId, Guid orderId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");

        if (orderId == Guid.Empty)
            throw new BadRequestException("OrderId is required.");
    }
}
