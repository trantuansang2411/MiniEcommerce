namespace PublicApi.DTOs.Responses.Warehousing;

public sealed class InventoryTransactionResponse
{
    public Guid Id { get; init; }
    public Guid InventoryId { get; init; }
    public string Type { get; init; } = string.Empty;
    public int QuantityChange { get; init; }
    public string? Reason { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
