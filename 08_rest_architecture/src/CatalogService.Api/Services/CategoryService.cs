using CatalogService.Api.Contracts;
using CatalogService.Api.Domain;
using CatalogService.Api.Repositories;

namespace CatalogService.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IItemRepository _items;

    public CategoryService(ICategoryRepository categories, IItemRepository items)
    {
        _categories = categories;
        _items = items;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categories.GetAllAsync(cancellationToken);
        return categories.Select(ToResponse).ToList();
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken);
        return category is null ? null : ToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        await _categories.AddAsync(category, cancellationToken);
        return ToResponse(category);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.Name = request.Name;
        category.Description = request.Description;
        await _categories.UpdateAsync(category, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        await _items.DeleteByCategoryAsync(id, cancellationToken);
        await _categories.DeleteAsync(category, cancellationToken);
        return true;
    }

    private static CategoryResponse ToResponse(Category category)
        => new(category.Id, category.Name, category.Description);
}
