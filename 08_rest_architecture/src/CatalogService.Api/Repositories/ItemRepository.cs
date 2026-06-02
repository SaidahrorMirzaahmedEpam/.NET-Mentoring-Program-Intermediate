using CatalogService.Api.Data;
using CatalogService.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly CatalogDbContext _context;

    public ItemRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Item> Items, int TotalCount)> GetPagedAsync(
        int? categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Items.AsNoTracking();

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Items.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Item> AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        _context.Items.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Item item, CancellationToken cancellationToken = default)
    {
        _context.Items.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var items = await _context.Items
            .Where(i => i.CategoryId == categoryId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return 0;
        }

        _context.Items.RemoveRange(items);
        await _context.SaveChangesAsync(cancellationToken);
        return items.Count;
    }
}
