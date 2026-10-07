namespace PublicApi.DTOs.Responses.Warehousing;

public sealed class InventoryResponse
{
    public Guid Id { get; init; }
    public Guid WarehouseId { get; init; }
    public string WarehouseCode { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int OnHand { get; init; }
    public int Reserved { get; init; }
    public int Available { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
