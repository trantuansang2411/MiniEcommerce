using ApplicationCore.Entities.Catalog;

namespace ApplicationCore.Interfaces.Catalog;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByCategoryAsync(string categoryName);
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<bool> HasProductsAsync(Guid categoryId);
}
