using ApplicationCore.Entities.Orders;

namespace ApplicationCore.Entities.Payments;

public class Payment
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid OrderId { get; private set; }

    public Order Order { get; private set; } = null!;

    public decimal Amount { get; private set; }

    public PaymentMethod Method { get; private set; }

    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    // Mã giao dịch do cổng thanh toán trả về. COD hoặc payment chưa khởi tạo có thể chưa có mã.
    public string? TransactionId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? PaidAt { get; private set; }

    private Payment() { }

    public Payment(Guid orderId, decimal amount, PaymentMethod method)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId is required.", nameof(orderId));

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        OrderId = orderId;
        Amount = amount;
        Method = method;
    }

    public void MarkAsSucceeded(string transactionId)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only a pending payment can be marked as succeeded.");

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("TransactionId is required.", nameof(transactionId));

        TransactionId = transactionId;
        Status = PaymentStatus.Succeeded;
        PaidAt = DateTimeOffset.UtcNow;
    }
}

public enum PaymentMethod
{
    CashOnDelivery = 1,
    BankTransfer = 2,
    VnPay = 3,
    Momo = 4,
    Manual = 5
}

public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}
