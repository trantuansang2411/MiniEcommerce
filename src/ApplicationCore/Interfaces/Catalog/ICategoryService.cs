using ApplicationCore.Entities.Catalog;

namespace ApplicationCore.Interfaces.Catalog;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<Category> GetByIdAsync(Guid id);
    Task<Category> CreateCategoryAsync(string categoryName, string? description, string? iconKey);
    Task<Category> UpdateCategoryAsync(Guid id, string? categoryName, string? description, string? iconKey, bool? isActive);
    Task DeleteCategoryAsync(Guid id);
}
