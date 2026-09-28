namespace RCommerce.WebApp.Models;

public class CartItem
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImagePath { get; set; }

    public int Quantity { get; set; } = 1;

    public decimal TotalPrice { get => Price * Quantity; }
}
