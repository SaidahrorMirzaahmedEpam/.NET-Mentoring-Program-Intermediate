using CatalogService.Api.Contracts;

namespace CatalogService.Api.Services;

public interface IItemService
{
    Task<PagedResponse<ItemResponse>> GetAsync(ItemQueryParameters query, CancellationToken cancellationToken = default);

    Task<ItemResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ItemResponse> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, UpdateItemRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
