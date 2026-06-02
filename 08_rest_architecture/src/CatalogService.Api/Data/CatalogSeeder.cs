using CatalogService.Api.Domain;

namespace CatalogService.Api.Data;

public static class CatalogSeeder
{
    public static void Seed(CatalogDbContext context)
    {
        if (context.Categories.Any())
        {
            return;
        }

        var electronics = new Category { Name = "Electronics", Description = "Devices and gadgets" };
        var books = new Category { Name = "Books", Description = "Printed and digital books" };
        context.Categories.AddRange(electronics, books);
        context.SaveChanges();

        context.Items.AddRange(
            new Item { Name = "Laptop", Description = "14-inch ultrabook", Price = 1299.99m, CategoryId = electronics.Id },
            new Item { Name = "Headphones", Description = "Noise-cancelling over-ear", Price = 199.50m, CategoryId = electronics.Id },
            new Item { Name = "Clean Code", Description = "Robert C. Martin", Price = 39.99m, CategoryId = books.Id });
        context.SaveChanges();
    }
}
