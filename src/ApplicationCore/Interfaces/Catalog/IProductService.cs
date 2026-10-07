using ApplicationCore.Entities.Catalog;
using ApplicationCore.Models;

namespace ApplicationCore.Interfaces.Catalog;

public sealed record ProductImageUpload(Stream Content, string FileName, string ContentType, long Length);

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllActiveAsync();
    Task<PagedResult<Product>> SearchActiveAsync(string? search, Guid? categoryId, int page, int pageSize);
    Task<IReadOnlyList<Product>> GetAllForManagementAsync();
    Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(Guid categoryId);
    Task<Product> GetActiveByIdAsync(Guid id);
    Task<Product> CreateAsync(
        Guid categoryId,
        string name,
        decimal originalPrice,
        decimal sellingPrice,
        string? specifications,
        ProductStatus status,
        ProductImageUpload? thumbnail,
        IReadOnlyCollection<ProductImageUpload> images,
        CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(
        Guid id,
        Guid? categoryId,
        string? name,
        decimal? originalPrice,
        decimal? sellingPrice,
        string? specifications,
        ProductStatus? status,
        ProductImageUpload? thumbnail,
        IReadOnlyCollection<ProductImageUpload> images,
        IReadOnlyCollection<string> removeImageUrls,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id);
}
