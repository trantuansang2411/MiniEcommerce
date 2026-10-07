namespace ApplicationCore.Entities.Shopping;

public class Cart
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public ICollection<CartItem> Items { get; private set; } = new List<CartItem>();

    private Cart() { }

    public Cart(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        UserId = userId;
    }

    public CartItem AddItem(Guid productId, int quantity)
    {
        var existingItem = Items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            Touch();
            return existingItem;
        }

        var item = new CartItem(Id, productId, quantity);
        Items.Add(item);
        Touch();
        return item;
    }

    public void RemoveItem(CartItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!Items.Remove(item))
            throw new InvalidOperationException("Cart item does not belong to this cart.");

        Touch();
    }

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
