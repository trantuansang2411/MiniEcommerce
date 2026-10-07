using System.ComponentModel.DataAnnotations;
namespace PublicApi.DTOs.Requests.Catalog
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        public string Name { get; init; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả danh mục không được vượt quá 500 ký tự.")]
        public string? Description { get; init; }

        [StringLength(50, ErrorMessage = "Icon key cannot exceed 50 characters.")]
        public string? IconKey { get; init; }

    }
}
