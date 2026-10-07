using ApplicationCore.Entities.Payments;
using ApplicationCore.Entities.Fulfillment;

namespace ApplicationCore.Entities.Orders;

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public decimal TotalAmount { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.AwaitingPayment;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; private set; }

    // Snapshot thông tin nhận hàng tại thời điểm đặt: profile sau này đổi/xóa không làm đổi lịch sử đơn.
    public string? RecipientName { get; private set; }

    public string? RecipientPhone { get; private set; }

    public string? ShippingAddress { get; private set; }

    public string? DeliveryNote { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    // Lưu lịch sử tất cả lần thanh toán: thất bại, đang chờ và thành công.
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    public ICollection<Shipment> Shipments { get; private set; } = new List<Shipment>();

    private Order() { }

    public Order(Guid userId, decimal totalAmount, DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (totalAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAmount));

        UserId = userId;
        TotalAmount = totalAmount;
        ExpiresAt = expiresAt;
    }

    public void SetDeliveryDetails(string recipientName, string recipientPhone, string shippingAddress, string? deliveryNote)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("Recipient name is required.", nameof(recipientName));
        if (string.IsNullOrWhiteSpace(recipientPhone))
            throw new ArgumentException("Recipient phone is required.", nameof(recipientPhone));
        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required.", nameof(shippingAddress));

        RecipientName = recipientName.Trim();
        RecipientPhone = recipientPhone.Trim();
        ShippingAddress = shippingAddress.Trim();
        DeliveryNote = string.IsNullOrWhiteSpace(deliveryNote) ? null : deliveryNote.Trim();
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        Items.Add(new OrderItem(Id, productId, quantity, unitPrice));
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.AwaitingPayment)
            throw new InvalidOperationException("Only an order awaiting payment can be marked as paid.");

        Status = OrderStatus.Paid;
    }

    public void MarkAsExpired()
    {
        if (Status != OrderStatus.AwaitingPayment)
            throw new InvalidOperationException("Only an order awaiting payment can expire.");

        Status = OrderStatus.Expired;
    }

    public void MarkAsCompleted()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Only a paid order can be completed.");

        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.AwaitingPayment)
            throw new InvalidOperationException("Only an order awaiting payment can be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}

public enum OrderStatus
{
    AwaitingPayment = 1,
    Paid = 2,
    Completed = 3,
    Cancelled = 4,
    Expired = 5
}
