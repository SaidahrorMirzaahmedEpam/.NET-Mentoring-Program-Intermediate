namespace CatalogService.Api.Contracts;

public record ItemResponse(int Id, string Name, string? Description, decimal Price, int CategoryId);
