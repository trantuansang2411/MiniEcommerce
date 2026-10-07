using ApplicationCore.Entities.Catalog;
using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Catalog;

public sealed class CreateProductReq
{
    public Guid CategoryId { get; init; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Original price must be zero or greater.")]
    public decimal OriginalPrice { get; init; }

    [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Selling price must be zero or greater.")]
    public decimal SellingPrice { get; init; }

    [Required(ErrorMessage = "Product thumbnail is required.")]
    public IFormFile? Thumbnail { get; init; }

    public List<IFormFile> Images { get; init; } = [];

    [StringLength(4000, ErrorMessage = "Specifications cannot exceed 4000 characters.")]
    public string? Specifications { get; init; }

    [EnumDataType(typeof(ProductStatus), ErrorMessage = "Product status is invalid.")]
    public ProductStatus? Status { get; init; }
}
