using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Warehousing;

public sealed class CreateWarehouseRequest
{
    [Required(ErrorMessage = "Warehouse code is required.")]
    [StringLength(30, ErrorMessage = "Warehouse code cannot exceed 30 characters.")]
    public string Code { get; init; } = string.Empty;

    [Required(ErrorMessage = "Warehouse name is required.")]
    [StringLength(200, ErrorMessage = "Warehouse name cannot exceed 200 characters.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Warehouse address is required.")]
    [StringLength(500, ErrorMessage = "Warehouse address cannot exceed 500 characters.")]
    public string Address { get; init; } = string.Empty;
}
