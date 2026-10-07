namespace ApplicationCore.Entities.Catalog;

public class ProductImage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public string ImageUrl { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private ProductImage() { }

    public ProductImage(Guid productId, string imageUrl, int sortOrder)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl is required.", nameof(imageUrl));

        ProductId = productId;
        ImageUrl = imageUrl.Trim();
        SortOrder = sortOrder;
    }

    public void UpdateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder));

        SortOrder = sortOrder;
    }
}
