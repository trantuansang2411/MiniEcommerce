using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Catalog;

public sealed class UpdateCategoryRequest
{
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string? Name { get; init; }

    [StringLength(500, ErrorMessage = "Category description cannot exceed 500 characters.")]
    public string? Description { get; init; }

    [StringLength(50, ErrorMessage = "Icon key cannot exceed 50 characters.")]
    public string? IconKey { get; init; }

    public bool? IsActive { get; init; }
}
