namespace ApplicationCore.Entities.Catalog;
public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    // Foreign Key
    public Guid CategoryId { get; private set; }

    // Navigation Property
    public Category Category { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public decimal OriginalPrice { get; private set; }

    public decimal SellingPrice { get; private set; }

    public string ThumbnailUrl { get; private set; } = string.Empty;

    public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();

    public string Specifications { get; private set; } = string.Empty;

    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private Product() { }

    public Product(Guid categoryId, string name, decimal originalPrice, decimal sellingPrice, string thumbnailUrl, string? specifications, ProductStatus status = ProductStatus.Draft)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required.", nameof(categoryId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (originalPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(originalPrice));

        if (sellingPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(sellingPrice));

        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("ThumbnailUrl is required.", nameof(thumbnailUrl));

        CategoryId = categoryId;
        Name = name.Trim();
        OriginalPrice = originalPrice;
        SellingPrice = sellingPrice;
        ThumbnailUrl = thumbnailUrl;
        Specifications = specifications ?? string.Empty;
        Status = status;
    }

    public void Update(
        Guid categoryId,
        string name,
        decimal originalPrice,
        decimal sellingPrice,
        string thumbnailUrl,
        string specifications,
        ProductStatus status)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required.", nameof(categoryId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (originalPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(originalPrice));

        if (sellingPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(sellingPrice));

        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("ThumbnailUrl is required.", nameof(thumbnailUrl));

        CategoryId = categoryId;
        Name = name.Trim();
        OriginalPrice = originalPrice;
        SellingPrice = sellingPrice;
        ThumbnailUrl = thumbnailUrl;
        Specifications = specifications;
        Status = status;
    }

    public void AddImages(IEnumerable<string> imageUrls)
    {
        var nextSortOrder = Images.Count == 0 ? 0 : Images.Max(image => image.SortOrder) + 1;
        foreach (var imageUrl in imageUrls)
            Images.Add(new ProductImage(Id, imageUrl, nextSortOrder++));
    }

    public IReadOnlyList<string> RemoveImages(IEnumerable<string> imageUrls)
    {
        var urlsToRemove = imageUrls.Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var imagesToRemove = Images.Where(x => urlsToRemove.Contains(x.ImageUrl)).ToList();

        foreach (var image in imagesToRemove)
            Images.Remove(image);

        return imagesToRemove.Select(x => x.ImageUrl).ToList();
    }
}
public enum ProductStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3
}
