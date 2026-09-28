using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RCommerce.WebApp.Models;
using RCommerce.Infrastructure;
using MapsterMapper;

namespace RCommerce.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly RCommerceContext _context;
    private readonly IMapper _mapper;

    public HomeController(ILogger<HomeController> logger, RCommerceContext context, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
    }

    public IActionResult Index()
    {
        _context.Database.EnsureCreated();

        var products = _mapper.Map<List<WebApp.Models.Product>>(
            _context.Products.Where(x => x.IsAvailable == true)
                .Include(x => x.Category)
                .ToList());

        return View(products);
    }

    [HttpGet]
    [Route("Filter/{categoryId}")]
    public IActionResult Index(int categoryId)
    {
        var products = _mapper.Map<List<WebApp.Models.Product>>(
            _context.Products.Where(x => x.IsAvailable == true && x.Category.Id == categoryId)
                .Include(x => x.Category)
                .ToList());

        return View(products);
    }

    [HttpGet]
    [Route("Details/{id}")]
    public IActionResult Details(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            return View(_mapper.Map<WebApp.Models.Product>(product));
        }
        return View();
    }

    [HttpPost]
    [Route("Add/{id}")]
    public IActionResult Add(int id)
    {
        var shopList = HttpContext.Session.Get<Dictionary<int, int>>(SessionHelper.ShoppingCart);

        if (shopList == null)
            shopList = new Dictionary<int, int>();

        if (!shopList.Keys.Contains(id))
            shopList.Add(id, 1);

        HttpContext.Session.Set(SessionHelper.ShoppingCart, shopList);

        var products = _mapper.Map<List<WebApp.Models.Product>>(
                _context.Products.Where(x => x.IsAvailable == true).ToList());

        return RedirectToAction("Index", "Home", products);
    }

    [HttpPost]
    [Route("Remove/{id}")]
    public IActionResult Remove(int id)
    {
        var shopList = HttpContext.Session.Get<Dictionary<int, int>>(SessionHelper.ShoppingCart);

        if (shopList == null)
            return RedirectToAction("Index", "Home", _context.Products.Where(x => x.IsAvailable == true));

        if (shopList.Keys.Contains(id))
            shopList.Remove(id);

        HttpContext.Session.Set(SessionHelper.ShoppingCart, shopList);

        var products = _mapper.Map<List<WebApp.Models.Product>>(
                _context.Products.Where(x => x.IsAvailable == true).ToList());
        return RedirectToAction("Index", "Home", products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
