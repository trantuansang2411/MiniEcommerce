using System.ComponentModel.DataAnnotations;
using ApplicationCore.Entities.Warehousing;

namespace PublicApi.DTOs.Requests.Warehousing;

public sealed class UpdateWarehouseRequest
{
    [StringLength(200, ErrorMessage = "Warehouse name cannot exceed 200 characters.")]
    public string? Name { get; init; }

    [StringLength(500, ErrorMessage = "Warehouse address cannot exceed 500 characters.")]
    public string? Address { get; init; }

    [EnumDataType(typeof(WarehouseStatus), ErrorMessage = "Warehouse status is invalid.")]
    public WarehouseStatus? Status { get; init; }
}
