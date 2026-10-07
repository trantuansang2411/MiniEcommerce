using ApplicationCore.Entities.Catalog;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Catalog;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services.Catalog;

public sealed class CategoryService : ICategoryService
{
    private readonly ILogger<CategoryService> _logger;
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(
        ILogger<CategoryService> logger,
        ICategoryRepository categoryRepository)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync() => _categoryRepository.GetAllAsync();

    public async Task<Category> GetByIdAsync(Guid id)
    {
        return await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");
    }

    public async Task<Category> CreateCategoryAsync(string categoryName, string? description, string? iconKey)
    {
        var normalizedCategoryName = categoryName.Trim();
        var existingCategory = await _categoryRepository.GetByCategoryAsync(normalizedCategoryName);
        if (existingCategory is not null)
            throw new ConflictException("Category already exists.");

        var category = new Category(normalizedCategoryName, description, iconKey);
        _categoryRepository.Add(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} created successfully", category.Id);
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Guid id, string? categoryName, string? description, string? iconKey, bool? isActive)
    {
        if (categoryName is null && description is null && iconKey is null && !isActive.HasValue)
            throw new BadRequestException("At least one field must be provided for update.");

        var category = await GetByIdAsync(id);

        if (categoryName is not null)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new BadRequestException("Category name cannot be empty.");

            var normalizedCategoryName = categoryName.Trim();
            var existingCategory = await _categoryRepository.GetByCategoryAsync(normalizedCategoryName);

            if (existingCategory is not null && existingCategory.Id != id)
                throw new ConflictException("Category already exists.");

            category.UpdateName(normalizedCategoryName);
        }

        if (isActive.HasValue)
            category.SetActiveStatus(isActive.Value);

        if (description is not null)
            category.UpdateDescription(description);

        if (iconKey is not null)
            category.UpdateIconKey(iconKey);

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} updated successfully", category.Id);
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await GetByIdAsync(id);

        if (await _categoryRepository.HasProductsAsync(id))
            throw new ConflictException("Cannot delete a category that contains products.");

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} deleted successfully", id);
    }
}
