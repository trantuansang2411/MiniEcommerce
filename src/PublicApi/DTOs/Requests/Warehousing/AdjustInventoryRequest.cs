using System.ComponentModel.DataAnnotations;
using ApplicationCore.Entities.Warehousing;

namespace PublicApi.DTOs.Requests.Warehousing;

public sealed class AdjustInventoryRequest
{
    public Guid WarehouseId { get; init; }
    public Guid ProductId { get; init; }
    public int QuantityChange { get; init; }

    [EnumDataType(typeof(InventoryAdjustmentReason), ErrorMessage = "Adjustment reason is invalid.")]
    public InventoryAdjustmentReason Reason { get; init; }
}
