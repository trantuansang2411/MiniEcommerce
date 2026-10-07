using ApplicationCore.Entities.Catalog;

namespace ApplicationCore.Entities.Shopping;

public class CartItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid CartId { get; private set; }

    public Cart Cart { get; private set; } = null!;

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public int Quantity { get; private set; }

    private CartItem() { }

    public CartItem(Guid cartId, Guid productId, int quantity)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentException("CartId is required.", nameof(cartId));

        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity += quantity;
    }

    public void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity = quantity;
    }
}
