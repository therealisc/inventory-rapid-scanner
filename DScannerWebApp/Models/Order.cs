namespace RCommerce.WebApp.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public List<CartItem> OrderedProducts { get; set; } = new();

    public decimal TotalPrice
    {
        get => OrderedProducts.Select(x => x.TotalPrice).Sum();
    }
}
