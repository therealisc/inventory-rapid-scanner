using Microsoft.AspNetCore.Mvc;
using RCommerce.Infrastructure;
using MapsterMapper;

namespace RCommerce.WebApp.Controllers;

public class ShoppingCartController : Controller
{
    private readonly RCommerceContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ShoppingCartController(RCommerceContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index()
    {
        var shoppingCart = _httpContextAccessor?.HttpContext?.Session.Get<Dictionary<int, int>>(SessionHelper.ShoppingCart);

        if (shoppingCart == null)
        {
            return View();
        }

        var products = _mapper.Map<List<Models.CartItem>>(
            _context.Products.Where(x => shoppingCart.Keys.Contains(x.Id))
                .ToList());

        products.ForEach(x => x.Quantity = shoppingCart[x.Id]);
        return View(products);
    }

    [Route("Cart/Remove/{id}")]
    public IActionResult Remove(int id)
    {
        var shoppingCart = _httpContextAccessor?.HttpContext?.Session.Get<Dictionary<int, int>>(SessionHelper.ShoppingCart);

        if (shoppingCart.Keys.Contains(id))
            shoppingCart.Remove(id);

        HttpContext.Session.Set(SessionHelper.ShoppingCart, shoppingCart);

        var products = _mapper.Map<List<Models.CartItem>>(
            _context.Products.Where(x => shoppingCart.Keys.Contains(x.Id))
                .ToList());

        products.ForEach(x => x.Quantity = shoppingCart[x.Id]);
        return View("Index", products);
    }

    [Route("Cart/IncreaseQuantity/{id}")]
    public IActionResult IncreaseQuantity(int id)
    {
        var shoppingCart = _httpContextAccessor?.HttpContext?.Session.Get<Dictionary<int, int>>(SessionHelper.ShoppingCart);
        shoppingCart[id] += 1;
        _httpContextAccessor.HttpContext.Session.Set(SessionHelper.ShoppingCart, shoppingCart);

        var products = _mapper.Map<List<Models.CartItem>>(
            _context.Products.Where(x => shoppingCart.Keys.Contains(x.Id))
                .ToList());

        products.ForEach(x => x.Quantity = shoppingCart[x.Id]);

        return View("Index", products);
    }

    [Route("Cart/DecreaseQuantity/{id}")]
    public IActionResult DecreaseQuantity(int id)
    {
        var shoppingCart = _httpContextAccessor?.HttpContext?.Session.Get<Dictionary<int, int>>(SessionHelper.ShoppingCart);

        var products = _mapper.Map<List<Models.CartItem>>(
            _context.Products.Where(x => shoppingCart.Keys.Contains(x.Id))
                .ToList());

        if (shoppingCart[id] <= 1)
        {
            return View("Index", products);
        }

        shoppingCart[id] -= 1;
        _httpContextAccessor?.HttpContext?.Session.Set(SessionHelper.ShoppingCart, shoppingCart);
        products.ForEach(x => x.Quantity = shoppingCart[x.Id]);

        return View("Index", products);
    }
}
