using CatalogService.Api.Contracts;
using CatalogService.Api.Domain;
using CatalogService.Api.Repositories;
using CatalogService.Api.Services;
using Moq;

namespace CatalogService.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IItemRepository> _items = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_categories.Object, _items.Object);
    }

    [Fact]
    public async Task GetAllAsync_MapsEntitiesToResponses()
    {
        _categories
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>
            {
                new() { Id = 1, Name = "Electronics", Description = "Gadgets" }
            });

        var result = await _sut.GetAllAsync();

        var category = Assert.Single(result);
        Assert.Equal(1, category.Id);
        Assert.Equal("Electronics", category.Name);
        Assert.Equal("Gadgets", category.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        _categories
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_PersistsCategory_AndReturnsResponse()
    {
        _categories
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category c, CancellationToken _) =>
            {
                c.Id = 5;
                return c;
            });

        var response = await _sut.CreateAsync(new CreateCategoryRequest { Name = "Books" });

        Assert.Equal(5, response.Id);
        Assert.Equal("Books", response.Name);
        _categories.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenCategoryMissing()
    {
        _categories
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var updated = await _sut.UpdateAsync(7, new UpdateCategoryRequest { Name = "X" });

        Assert.False(updated);
        _categories.Verify(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields_WhenCategoryExists()
    {
        var existing = new Category { Id = 3, Name = "Old", Description = "Old desc" };
        _categories
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var updated = await _sut.UpdateAsync(3, new UpdateCategoryRequest { Name = "New", Description = "New desc" });

        Assert.True(updated);
        Assert.Equal("New", existing.Name);
        Assert.Equal("New desc", existing.Description);
        _categories.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRelatedItems_ThenCategory()
    {
        var existing = new Category { Id = 2, Name = "Electronics" };
        _categories
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var deleted = await _sut.DeleteAsync(2);

        Assert.True(deleted);
        _items.Verify(r => r.DeleteByCategoryAsync(2, It.IsAny<CancellationToken>()), Times.Once);
        _categories.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenCategoryMissing()
    {
        _categories
            .Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var deleted = await _sut.DeleteAsync(404);

        Assert.False(deleted);
        _items.Verify(r => r.DeleteByCategoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _categories.Verify(r => r.DeleteAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
