using System.ComponentModel.DataAnnotations;

namespace CatalogService.Api.Contracts;

public record CreateCategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }
}
