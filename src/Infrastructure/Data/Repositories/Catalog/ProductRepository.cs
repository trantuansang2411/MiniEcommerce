using ApplicationCore.Entities.Catalog;
using ApplicationCore.Interfaces.Catalog;
using ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories.Catalog;

public sealed class ProductRepository : EfRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetAllActiveAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.Status == ProductStatus.Active && x.Category.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedResult<Product>> SearchActiveAsync(string? search, Guid? categoryId, int page, int pageSize)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 50);
        var normalizedSearch = search?.Trim();

        IQueryable<Product> query = _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.Status == ProductStatus.Active && x.Category.IsActive);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
            query = query.Where(x => x.Name.Contains(normalizedSearch) || x.Specifications.Contains(normalizedSearch));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        return new PagedResult<Product>(items, totalCount, normalizedPage, normalizedPageSize);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(Guid categoryId)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.CategoryId == categoryId && x.Status == ProductStatus.Active && x.Category.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<Product?> GetActiveByIdAsync(Guid id)
    {
        return _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status == ProductStatus.Active && x.Category.IsActive);
    }

    public Task<Product?> GetByIdWithImagesAsync(Guid id)
    {
        return _context.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<bool> HasCartItemsAsync(Guid productId)
    {
        return _context.CartItems.AnyAsync(x => x.ProductId == productId);
    }

    public Task<bool> HasOrderItemsAsync(Guid productId)
    {
        return _context.OrderItems.AnyAsync(x => x.ProductId == productId);
    }
}
