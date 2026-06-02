namespace CatalogService.Api.Services;

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(int categoryId)
        : base($"Category with id {categoryId} does not exist.")
    {
        CategoryId = categoryId;
    }

    public int CategoryId { get; }
}
