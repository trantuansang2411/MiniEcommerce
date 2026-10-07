namespace PublicApi.DTOs.Responses.Shopping;

public sealed class CartResponse
{
    public Guid? Id { get; init; }
    public IReadOnlyList<CartItemResponse> Items { get; init; } = Array.Empty<CartItemResponse>();
    public int TotalQuantity { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
