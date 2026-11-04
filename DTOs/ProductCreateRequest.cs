using System.ComponentModel.DataAnnotations;

namespace FTELMarket.DTOs;

public class ProductCreateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}