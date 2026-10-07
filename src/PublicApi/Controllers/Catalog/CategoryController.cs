using ApplicationCore.Entities.Catalog;
using ApplicationCore.Interfaces.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Requests.Catalog;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Catalog;

namespace PublicApi.Controllers.Catalog;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        return Ok(new ApiResponse<IReadOnlyList<CategoryResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Categories retrieved successfully.",
            Data = categories.Select(ToResponse).ToList()
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        return Ok(new ApiResponse<CategoryResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Category retrieved successfully.",
            Data = ToResponse(category)
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateCategoryAsync(request.Name, request.Description, request.IconKey);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, new ApiResponse<CategoryResponse>
        {
            StatusCode = StatusCodes.Status201Created,
            Message = "Category created successfully.",
            Data = ToResponse(category)
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest request)
    {
        var category = await _categoryService.UpdateCategoryAsync(id, request.Name, request.Description, request.IconKey, request.IsActive);

        return Ok(new ApiResponse<CategoryResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Category updated successfully.",
            Data = ToResponse(category)
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _categoryService.DeleteCategoryAsync(id);

        return Ok(new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Category deleted successfully.",
            Data = null
        });
    }

    private static CategoryResponse ToResponse(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IconKey = category.IconKey,
        IsActive = category.IsActive,
        CreatedAt = category.CreatedAt
    };
}
