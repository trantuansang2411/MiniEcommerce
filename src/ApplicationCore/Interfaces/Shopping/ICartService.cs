using ApplicationCore.Entities.Shopping;

namespace ApplicationCore.Interfaces.Shopping;

public interface ICartService
{
    Task<Cart?> GetMyCartAsync(Guid userId);
    Task<Cart> AddItemAsync(Guid userId, Guid productId, int quantity);
    Task<Cart> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity);
    Task RemoveItemAsync(Guid userId, Guid productId);
}
