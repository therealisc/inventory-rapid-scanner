using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCommerce.Infrastructure;
using RCommerce.WebApp.Models;

namespace RCommerce.WebApp.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly RCommerceContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserController(RCommerceContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var claim = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "Id");
        var userId = claim?.Value;
        var orders = _context.Orders
            .Where(x => x.UserId == Convert.ToInt32(userId))
            .Include(x => x.OrderProducts)
                .ThenInclude(x => x.Product)
            .ToList();

        var ordersDisplayModels = new List<Order>();

        foreach (var order in orders)
        {
            ordersDisplayModels.Add(new Order
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                OrderedProducts = order.OrderProducts.Select(x => new CartItem
                {
                    Price = x.Product.Price,
                    Quantity = x.Quantity
                }).ToList()
            });
        }

        return View(ordersDisplayModels);
    }
}
