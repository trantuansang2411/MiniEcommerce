using ApplicationCore.Entities.Catalog;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Catalog;
using ApplicationCore.Models;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services.Catalog;

public sealed class ProductService : IProductService
{
    private const long MaxImageFileSize = 10 * 1024 * 1024;
    private const int MaxGalleryImages = 10;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductImageStorage _productImageStorage;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IProductImageStorage productImageStorage, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productImageStorage = productImageStorage;
        _logger = logger;
    }

    public Task<IReadOnlyList<Product>> GetAllActiveAsync() => _productRepository.GetAllActiveAsync();
    public Task<PagedResult<Product>> SearchActiveAsync(string? search, Guid? categoryId, int page, int pageSize) =>
        _productRepository.SearchActiveAsync(search, categoryId, page, pageSize);
    public Task<IReadOnlyList<Product>> GetAllForManagementAsync() => _productRepository.GetAllAsync();
    public Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(Guid categoryId) => _productRepository.GetActiveByCategoryAsync(categoryId);

    public async Task<Product> GetActiveByIdAsync(Guid id) => await _productRepository.GetActiveByIdAsync(id) ?? throw new NotFoundException("Product not found.");

    public async Task<Product> CreateAsync(Guid categoryId, string name, decimal originalPrice, decimal sellingPrice, string? specifications, ProductStatus status, ProductImageUpload? thumbnail, IReadOnlyCollection<ProductImageUpload> images, CancellationToken cancellationToken = default)
    {
        ValidateNameAndPrices(name, originalPrice, sellingPrice);
        ValidateStatus(status);
        ValidateThumbnail(thumbnail);
        ValidateGalleryCount(images.Count);

        var category = await GetCategoryAsync(categoryId);
        EnsureCategoryCanPublish(category, status, hasThumbnail: true);

        var uploadedUrls = new List<string>();
        try
        {
            var thumbnailUrl = await SaveImageAsync(name, thumbnail!, cancellationToken);
            uploadedUrls.Add(thumbnailUrl);
            var galleryUrls = await SaveImagesAsync(name, images, uploadedUrls, cancellationToken);

            var product = new Product(categoryId, name, originalPrice, sellingPrice, thumbnailUrl, specifications, status);
            product.AddImages(galleryUrls);

            _productRepository.Add(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} created with {ImageCount} gallery images", product.Id, product.Images.Count);
            return product;
        }
        catch
        {
            await DeleteFilesAsync(uploadedUrls, cancellationToken);
            throw;
        }
    }

    public async Task<Product> UpdateAsync(Guid id, Guid? categoryId, string? name, decimal? originalPrice, decimal? sellingPrice, string? specifications, ProductStatus? status, ProductImageUpload? thumbnail, IReadOnlyCollection<ProductImageUpload> images, IReadOnlyCollection<string> removeImageUrls, CancellationToken cancellationToken = default)
    {
        if (categoryId is null && name is null && originalPrice is null && sellingPrice is null && specifications is null && status is null && thumbnail is null && images.Count == 0 && removeImageUrls.Count == 0)
            throw new BadRequestException("At least one field must be provided for update.");

        var product = await _productRepository.GetByIdWithImagesAsync(id) ?? throw new NotFoundException("Product not found.");
        var nextCategoryId = categoryId ?? product.CategoryId;
        var nextName = name ?? product.Name;
        var nextOriginalPrice = originalPrice ?? product.OriginalPrice;
        var nextSellingPrice = sellingPrice ?? product.SellingPrice;
        var nextSpecifications = specifications ?? product.Specifications;
        var nextStatus = status ?? product.Status;

        ValidateNameAndPrices(nextName, nextOriginalPrice, nextSellingPrice);
        ValidateStatus(nextStatus);
        ValidateGalleryCount(product.Images.Count - removeImageUrls.Distinct(StringComparer.OrdinalIgnoreCase).Count(url => product.Images.Any(image => string.Equals(image.ImageUrl, url, StringComparison.OrdinalIgnoreCase))) + images.Count);

        var category = await GetCategoryAsync(nextCategoryId);
        EnsureCategoryCanPublish(category, nextStatus, hasThumbnail: !string.IsNullOrWhiteSpace(product.ThumbnailUrl) || thumbnail is not null);

        var uploadedUrls = new List<string>();
        var previousThumbnailUrl = product.ThumbnailUrl;
        var normalizedRemoveImageUrls = removeImageUrls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        IReadOnlyList<string> removedUrls = Array.Empty<string>();
        try
        {
            var nextThumbnailUrl = thumbnail is null ? previousThumbnailUrl : await SaveImageAsync(nextName, thumbnail, cancellationToken);
            if (thumbnail is not null) uploadedUrls.Add(nextThumbnailUrl);

            var galleryUrls = await SaveImagesAsync(nextName, images, uploadedUrls, cancellationToken);
            if (normalizedRemoveImageUrls.Length > 0)
                removedUrls = product.RemoveImages(normalizedRemoveImageUrls);
            product.AddImages(galleryUrls);
            product.Update(nextCategoryId, nextName, nextOriginalPrice, nextSellingPrice, nextThumbnailUrl, nextSpecifications, nextStatus);

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
        }
        catch
        {
            await DeleteFilesAsync(uploadedUrls, cancellationToken);
            throw;
        }

        if (thumbnail is not null)
            await _productImageStorage.DeleteAsync(previousThumbnailUrl, cancellationToken);
        await DeleteFilesAsync(removedUrls, cancellationToken);

        _logger.LogInformation("Product {ProductId} updated with {ImageCount} gallery images", product.Id, product.Images.Count);
        return product;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdWithImagesAsync(id) ?? throw new NotFoundException("Product not found.");
        if (await _productRepository.HasOrderItemsAsync(id)) throw new ConflictException("Cannot delete a product that exists in an order.");
        if (await _productRepository.HasCartItemsAsync(id)) throw new ConflictException("Cannot delete a product that exists in a cart.");

        var imageUrls = product.Images.Select(x => x.ImageUrl).Append(product.ThumbnailUrl).ToList();
        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync();
        await DeleteFilesAsync(imageUrls, CancellationToken.None);

        _logger.LogInformation("Product {ProductId} deleted", id);
    }

    private async Task<Category> GetCategoryAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty) throw new BadRequestException("CategoryId is required.");
        return await _categoryRepository.GetByIdAsync(categoryId) ?? throw new NotFoundException("Category not found.");
    }

    private async Task<IReadOnlyList<string>> SaveImagesAsync(string productName, IEnumerable<ProductImageUpload> images, ICollection<string> uploadedUrls, CancellationToken cancellationToken)
    {
        var urls = new List<string>();
        foreach (var image in images)
        {
            var url = await SaveImageAsync(productName, image, cancellationToken);
            urls.Add(url);
            uploadedUrls.Add(url);
        }
        return urls;
    }

    private async Task<string> SaveImageAsync(string productName, ProductImageUpload image, CancellationToken cancellationToken)
    {
        ValidateImage(image);
        return await _productImageStorage.SaveAsync(productName, image.Content, Path.GetExtension(image.FileName), cancellationToken);
    }

    private static void ValidateThumbnail(ProductImageUpload? thumbnail)
    {
        if (thumbnail is null) throw new BadRequestException("Product thumbnail is required.");
        ValidateImage(thumbnail);
    }

    private static void ValidateImage(ProductImageUpload image)
    {
        if (image.Length <= 0) throw new BadRequestException("Image file is required.");
        if (image.Length > MaxImageFileSize) throw new BadRequestException("Image file cannot exceed 10 MB.");
        var extension = Path.GetExtension(image.FileName);
        if (string.IsNullOrWhiteSpace(image.FileName) || string.IsNullOrWhiteSpace(image.ContentType) || !AllowedImageExtensions.Contains(extension) || !AllowedImageContentTypes.Contains(image.ContentType))
            throw new BadRequestException("Only JPG, JPEG, PNG, and WEBP image files are allowed.");
    }

    private static void ValidateGalleryCount(int count)
    {
        if (count > MaxGalleryImages) throw new BadRequestException($"A product can have at most {MaxGalleryImages} gallery images.");
    }

    private static void EnsureCategoryCanPublish(Category category, ProductStatus status, bool hasThumbnail)
    {
        if (status == ProductStatus.Active && !category.IsActive) throw new ConflictException("An inactive category cannot contain a published product.");
        if (status == ProductStatus.Active && !hasThumbnail) throw new BadRequestException("A published product must have a thumbnail.");
    }

    private static void ValidateNameAndPrices(string name, decimal originalPrice, decimal sellingPrice)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BadRequestException("Product name cannot be empty.");
        if (originalPrice < 0 || sellingPrice < 0) throw new BadRequestException("Product prices cannot be negative.");
    }

    private static void ValidateStatus(ProductStatus status)
    {
        if (!Enum.IsDefined(status)) throw new BadRequestException("Product status is invalid.");
    }

    private async Task DeleteFilesAsync(IEnumerable<string> imageUrls, CancellationToken cancellationToken)
    {
        foreach (var imageUrl in imageUrls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct(StringComparer.OrdinalIgnoreCase))
            await _productImageStorage.DeleteAsync(imageUrl, cancellationToken);
    }
}
