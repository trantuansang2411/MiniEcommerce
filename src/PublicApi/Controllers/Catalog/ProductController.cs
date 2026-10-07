using ApplicationCore.Entities.Catalog;
using ApplicationCore.Interfaces.Catalog;
using ApplicationCore.Interfaces.Warehousing;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Requests.Catalog;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Catalog;

namespace PublicApi.Controllers.Catalog;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IInventoryRepository _inventoryRepository;

    public ProductController(IProductService productService, IInventoryRepository inventoryRepository)
    {
        _productService = productService;
        _inventoryRepository = inventoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllActiveAsync();
        return Ok(Success(await ToResponsesWithAvailabilityAsync(products), "Products retrieved successfully."));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _productService.SearchActiveAsync(search, categoryId, page, pageSize);
        var response = new PagedResponse<ProductResponse>
        {
            Items = await ToResponsesWithAvailabilityAsync(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };

        return Ok(Success(response, "Products retrieved successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("management")]
    public async Task<IActionResult> GetAllForManagement()
    {
        var products = await _productService.GetAllForManagementAsync();
        return Ok(Success(products.Select(ToResponse).ToList(), "Products retrieved successfully."));
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var products = await _productService.GetActiveByCategoryAsync(categoryId);
        return Ok(Success(await ToResponsesWithAvailabilityAsync(products), "Products retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetActiveByIdAsync(id);
        var availableStock = await _inventoryRepository.GetTotalAvailableForProductAsync(id);
        return Ok(Success(ToDetailResponse(product, availableStock), "Product retrieved successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateProductReq request, CancellationToken cancellationToken)
    {
        using var thumbnailContent = request.Thumbnail?.OpenReadStream();
        var galleryContents = request.Images.Select(image => image.OpenReadStream()).ToList();
        try
        {
            var thumbnail = CreateUpload(request.Thumbnail, thumbnailContent);
            var images = request.Images.Zip(galleryContents, CreateGalleryUpload).ToList();
            var product = await _productService.CreateAsync(
                request.CategoryId,
                request.Name,
                request.OriginalPrice,
                request.SellingPrice,
                request.Specifications,
                request.Status ?? ProductStatus.Draft,
                thumbnail,
                images,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created,
                Success(ToResponse(product), "Product created successfully.", StatusCodes.Status201Created));
        }
        finally
        {
            foreach (var content in galleryContents) content.Dispose();
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductReq request, CancellationToken cancellationToken)
    {
        using var thumbnailContent = request.Thumbnail?.OpenReadStream();
        var galleryContents = request.Images.Select(image => image.OpenReadStream()).ToList();
        try
        {
            var thumbnail = CreateUpload(request.Thumbnail, thumbnailContent);
            var images = request.Images.Zip(galleryContents, CreateGalleryUpload).ToList();
            var product = await _productService.UpdateAsync(
                id,
                request.CategoryId,
                request.Name,
                request.OriginalPrice,
                request.SellingPrice,
                request.Specifications,
                request.Status,
                thumbnail,
                images,
                request.RemoveImageUrls,
                cancellationToken);

            return Ok(Success(ToResponse(product), "Product updated successfully."));
        }
        finally
        {
            foreach (var content in galleryContents) content.Dispose();
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);
        return Ok(Success<object?>(null, "Product deleted successfully."));
    }

    private static ApiResponse<T> Success<T>(T data, string message, int statusCode = StatusCodes.Status200OK) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    private static ProductResponse ToResponse(Product product, int availableStock = 0) => new()
    {
        Id = product.Id,
        CategoryId = product.CategoryId,
        Name = product.Name,
        OriginalPrice = product.OriginalPrice,
        SellingPrice = product.SellingPrice,
        ThumbnailUrl = product.ThumbnailUrl,
        ImageUrls = product.Images.OrderBy(image => image.SortOrder).Select(image => image.ImageUrl).ToList(),
        Specifications = product.Specifications,
        Status = product.Status,
        CreatedAt = product.CreatedAt,
        AvailableStock = availableStock
    };

    private static ProductDetailResponse ToDetailResponse(Product product, int availableStock) => new()
    {
        Id = product.Id,
        CategoryId = product.CategoryId,
        Name = product.Name,
        OriginalPrice = product.OriginalPrice,
        SellingPrice = product.SellingPrice,
        ThumbnailUrl = product.ThumbnailUrl,
        ImageUrls = product.Images.OrderBy(image => image.SortOrder).Select(image => image.ImageUrl).ToList(),
        Specifications = product.Specifications,
        Status = product.Status,
        CreatedAt = product.CreatedAt,
        AvailableStock = availableStock
    };

    private async Task<IReadOnlyList<ProductResponse>> ToResponsesWithAvailabilityAsync(IReadOnlyList<Product> products)
    {
        var stockByProductId = await _inventoryRepository.GetTotalAvailableForProductsAsync(products.Select(product => product.Id));
        return products.Select(product => ToResponse(product, stockByProductId.GetValueOrDefault(product.Id))).ToList();
    }

    private static ProductImageUpload? CreateUpload(IFormFile? file, Stream? content) =>
        file is null || content is null ? null : new ProductImageUpload(content, file.FileName, file.ContentType, file.Length);

    private static ProductImageUpload CreateGalleryUpload(IFormFile file, Stream content) =>
        new(content, file.FileName, file.ContentType, file.Length);
}
