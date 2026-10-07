using System.ComponentModel.DataAnnotations;

namespace PublicApi.DTOs.Requests.Shopping;

public sealed class AddCartItemRequest
{
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; init; }
}
