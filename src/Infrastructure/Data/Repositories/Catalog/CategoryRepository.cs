using ApplicationCore.Entities.Catalog;
using ApplicationCore.Interfaces.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Catalog;

public sealed class CategoryRepository : EfRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Category?> GetByCategoryAsync(string categoryName)
    {
        return _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == categoryName);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<bool> HasProductsAsync(Guid categoryId)
    {
        return _context.Products.AnyAsync(x => x.CategoryId == categoryId);
    }
}
