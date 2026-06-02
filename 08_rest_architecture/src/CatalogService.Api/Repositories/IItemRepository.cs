using CatalogService.Api.Domain;

namespace CatalogService.Api.Repositories;

public interface IItemRepository
{
    Task<(IReadOnlyList<Item> Items, int TotalCount)> GetPagedAsync(
        int? categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Item> AddAsync(Item item, CancellationToken cancellationToken = default);

    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);

    Task DeleteAsync(Item item, CancellationToken cancellationToken = default);

    Task<int> DeleteByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
}
