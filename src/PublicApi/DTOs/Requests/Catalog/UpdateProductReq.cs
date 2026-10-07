using ApplicationCore.Entities.Catalog;
using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Catalog;

public sealed class UpdateProductReq
{
    public Guid? CategoryId { get; init; }

    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
    public string? Name { get; init; }

    [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Original price must be zero or greater.")]
    public decimal? OriginalPrice { get; init; }

    [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Selling price must be zero or greater.")]
    public decimal? SellingPrice { get; init; }

    public IFormFile? Thumbnail { get; init; }

    public List<IFormFile> Images { get; init; } = [];

    public List<string> RemoveImageUrls { get; init; } = [];

    [StringLength(4000, ErrorMessage = "Specifications cannot exceed 4000 characters.")]
    public string? Specifications { get; init; }

    [EnumDataType(typeof(ProductStatus), ErrorMessage = "Product status is invalid.")]
    public ProductStatus? Status { get; init; }
}
