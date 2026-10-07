using ApplicationCore.Entities.Catalog;

namespace PublicApi.DTOs.Responses.Catalog;

public class ProductResponse
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal OriginalPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public string ThumbnailUrl { get; init; } = string.Empty;
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
    public string Specifications { get; init; } = string.Empty;
    public ProductStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public int AvailableStock { get; init; }
}
