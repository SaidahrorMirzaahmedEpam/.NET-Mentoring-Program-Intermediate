using System.ComponentModel.DataAnnotations;

namespace CatalogService.Api.Contracts;

public record CreateItemRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    [Range(0.0, double.MaxValue)]
    public decimal Price { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}
