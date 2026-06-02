using CatalogService.Api.Contracts;
using CatalogService.Api.Domain;
using CatalogService.Api.Repositories;
using CatalogService.Api.Services;
using Moq;

namespace CatalogService.Tests;

public class ItemServiceTests
{
    private readonly Mock<IItemRepository> _items = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly ItemService _sut;

    public ItemServiceTests()
    {
        _sut = new ItemService(_items.Object, _categories.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsPagedResponse_WithMetadata()
    {
        var query = new ItemQueryParameters { CategoryId = 1, PageNumber = 2, PageSize = 10 };
        _items
            .Setup(r => r.GetPagedAsync(1, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Item> { new() { Id = 11, Name = "Phone", CategoryId = 1 } }, 25));

        var result = await _sut.GetAsync(query);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCategoryDoesNotExist()
    {
        _categories
            .Setup(r => r.ExistsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateItemRequest { Name = "Widget", Price = 1m, CategoryId = 42 };

        var exception = await Assert.ThrowsAsync<CategoryNotFoundException>(
            () => _sut.CreateAsync(request));
        Assert.Equal(42, exception.CategoryId);
        _items.Verify(r => r.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PersistsItem_WhenCategoryExists()
    {
        _categories
            .Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _items
            .Setup(r => r.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item i, CancellationToken _) =>
            {
                i.Id = 100;
                return i;
            });

        var request = new CreateItemRequest { Name = "Mouse", Description = "Wireless", Price = 25.5m, CategoryId = 1 };
        var response = await _sut.CreateAsync(request);

        Assert.Equal(100, response.Id);
        Assert.Equal("Mouse", response.Name);
        Assert.Equal(25.5m, response.Price);
        Assert.Equal(1, response.CategoryId);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenItemMissing()
    {
        _items
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        var updated = await _sut.UpdateAsync(5, new UpdateItemRequest { Name = "X", Price = 1m, CategoryId = 1 });

        Assert.False(updated);
        _items.Verify(r => r.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenTargetCategoryMissing()
    {
        _items
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Item { Id = 5, Name = "Old", CategoryId = 1 });
        _categories
            .Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<CategoryNotFoundException>(
            () => _sut.UpdateAsync(5, new UpdateItemRequest { Name = "New", Price = 1m, CategoryId = 99 }));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenItemExists()
    {
        var item = new Item { Id = 8, Name = "Keyboard", CategoryId = 1 };
        _items
            .Setup(r => r.GetByIdAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var deleted = await _sut.DeleteAsync(8);

        Assert.True(deleted);
        _items.Verify(r => r.DeleteAsync(item, It.IsAny<CancellationToken>()), Times.Once);
    }
}
