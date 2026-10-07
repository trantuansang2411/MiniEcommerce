using ApplicationCore.Models;
using ApplicationCore.Entities.Catalog;

namespace ApplicationCore.Interfaces.Catalog;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAllActiveAsync();
    Task<PagedResult<Product>> SearchActiveAsync(string? search, Guid? categoryId, int page, int pageSize);
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(Guid categoryId);
    Task<Product?> GetActiveByIdAsync(Guid id);
    Task<Product?> GetByIdWithImagesAsync(Guid id);
    Task<bool> HasCartItemsAsync(Guid productId);
    Task<bool> HasOrderItemsAsync(Guid productId);
}
