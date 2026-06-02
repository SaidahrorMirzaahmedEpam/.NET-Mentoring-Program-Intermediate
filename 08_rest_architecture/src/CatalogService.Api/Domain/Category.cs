namespace CatalogService.Api.Domain;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();
}
