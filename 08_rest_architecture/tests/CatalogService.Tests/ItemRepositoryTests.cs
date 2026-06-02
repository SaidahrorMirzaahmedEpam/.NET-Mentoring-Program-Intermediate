using CatalogService.Api.Data;
using CatalogService.Api.Domain;
using CatalogService.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests;

public class ItemRepositoryTests
{
    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"items-{Guid.NewGuid()}")
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByCategory()
    {
        await using var context = CreateContext();
        context.Items.AddRange(
            new Item { Name = "A", CategoryId = 1, Price = 1m },
            new Item { Name = "B", CategoryId = 2, Price = 1m },
            new Item { Name = "C", CategoryId = 1, Price = 1m });
        await context.SaveChangesAsync();
        var repository = new ItemRepository(context);

        var (items, totalCount) = await repository.GetPagedAsync(categoryId: 1, pageNumber: 1, pageSize: 20);

        Assert.Equal(2, totalCount);
        Assert.All(items, i => Assert.Equal(1, i.CategoryId));
    }

    [Fact]
    public async Task GetPagedAsync_AppliesPagination()
    {
        await using var context = CreateContext();
        for (var i = 0; i < 25; i++)
        {
            context.Items.Add(new Item { Name = $"Item {i}", CategoryId = 1, Price = 1m });
        }

        await context.SaveChangesAsync();
        var repository = new ItemRepository(context);

        var (page2, totalCount) = await repository.GetPagedAsync(categoryId: null, pageNumber: 2, pageSize: 10);

        Assert.Equal(25, totalCount);
        Assert.Equal(10, page2.Count);
    }

    [Fact]
    public async Task DeleteByCategoryAsync_RemovesOnlyMatchingItems()
    {
        await using var context = CreateContext();
        context.Items.AddRange(
            new Item { Name = "Keep", CategoryId = 2, Price = 1m },
            new Item { Name = "Drop1", CategoryId = 1, Price = 1m },
            new Item { Name = "Drop2", CategoryId = 1, Price = 1m });
        await context.SaveChangesAsync();
        var repository = new ItemRepository(context);

        var removed = await repository.DeleteByCategoryAsync(1);

        Assert.Equal(2, removed);
        Assert.Single(context.Items);
        Assert.Equal("Keep", context.Items.Single().Name);
    }
}
