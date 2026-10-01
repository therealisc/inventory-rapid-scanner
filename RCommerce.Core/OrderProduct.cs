namespace RCommerce.Core;

public class OrderProduct
{
    public int OrdersId { get; set; }
    public int ProductsId { get; set; }
    public int Quantity { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
