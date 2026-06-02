using CatalogService.Api.Contracts;
using CatalogService.Api.Domain;
using CatalogService.Api.Repositories;

namespace CatalogService.Api.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _items;
    private readonly ICategoryRepository _categories;

    public ItemService(IItemRepository items, ICategoryRepository categories)
    {
        _items = items;
        _categories = categories;
    }

    public async Task<PagedResponse<ItemResponse>> GetAsync(ItemQueryParameters query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _items.GetPagedAsync(
            query.CategoryId,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var responses = items.Select(ToResponse).ToList();
        return new PagedResponse<ItemResponse>(responses, query.PageNumber, query.PageSize, totalCount);
    }

    public async Task<ItemResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _items.GetByIdAsync(id, cancellationToken);
        return item is null ? null : ToResponse(item);
    }

    public async Task<ItemResponse> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var item = new Item
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId
        };

        await _items.AddAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<bool> UpdateAsync(int id, UpdateItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _items.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        item.Name = request.Name;
        item.Description = request.Description;
        item.Price = request.Price;
        item.CategoryId = request.CategoryId;
        await _items.UpdateAsync(item, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _items.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        await _items.DeleteAsync(item, cancellationToken);
        return true;
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        if (!await _categories.ExistsAsync(categoryId, cancellationToken))
        {
            throw new CategoryNotFoundException(categoryId);
        }
    }

    private static ItemResponse ToResponse(Item item)
        => new(item.Id, item.Name, item.Description, item.Price, item.CategoryId);
}
