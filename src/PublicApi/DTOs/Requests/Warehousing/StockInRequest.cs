using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Warehousing;

public sealed class StockInRequest
{
    public Guid WarehouseId { get; init; }
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; init; }
}
