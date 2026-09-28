using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RCommerce.WebApp.Models;

public class Product
{
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string ProductName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string ProductDescription { get; set; } = string.Empty;

    [Required]
    [Range(1, 200000)]
    public decimal Price { get; set; }

    [Required]
    public bool IsAvailable { get; set; }

    public string? ImagePath { get; set; }

    public Category Category { get; set; } = new();

    public int CategoryId { get; set; }

    public List<SelectListItem> CategoryList { get; set; } = new();
}
