using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using PayPal.Api;
using RCommerce.Infrastructure;
using MapsterMapper;
using RCommerce.Core;

namespace RCommerce.WebApp.Controllers;

public class PaymentController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly RCommerceContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentController> _logger;
    private Payment? _payment { get; set; }

    public PaymentController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, RCommerceContext context, IMapper mapper, ILogger<PaymentController> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [Authorize]
    public async Task<ActionResult> PaymentWithPayPal(string payerId = "", string guid = "")
    {

        if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == false)
        {
            return Redirect("/Login");
        }

        var shoppingCart = _httpContextAccessor?.HttpContext?.Session.Get<Dictionary<int, int>>(SessionHelper.ShoppingCart) ?? new Dictionary<int, int>();
        if (shoppingCart.Count == 0)
        {
            return Redirect("/ShoppingCart");
        }
        
        var apiContext = PayPalConfiguration.GetAPIContext(_configuration);

        try
        {
            if (string.IsNullOrWhiteSpace(payerId))
            {
                var baseURI = this.Request.Scheme + "://" + this.Request.Host + "/Payment/PaymentWithPayPal?";
                var guidd = Convert.ToString((new Random()).Next(100000));
                guid = guidd;

                var createdPayment = await CreatePayment(shoppingCart, apiContext, baseURI + "guid=" + guid);
                var links = createdPayment.links.GetEnumerator();
                string paypalRedirectUrl = "";
                while (links.MoveNext())
                {
                    var lnk = links.Current;
                    if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                    {
                        paypalRedirectUrl = lnk.href;
                    }
                }

                _httpContextAccessor?.HttpContext?.Session.SetString("payment", createdPayment.id);
                return Redirect(paypalRedirectUrl);
            }
            else
            {
                var paymentId = _httpContextAccessor?.HttpContext?.Session.GetString("payment") ?? "";
                var executedPayment = ExecutePayment(apiContext, payerId, paymentId);
                if (executedPayment.state.ToLower() != "approved")
                {
                    return View("PaymentFailed");
                }

                _httpContextAccessor?.HttpContext?.Session.Remove(SessionHelper.ShoppingCart);
                return View("PaymentSuccess");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.InnerException?.Message);
            return View("PaymentFailed");
        }
    }

    private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
    {
        var paymentExecution = new PaymentExecution()
        {
            payer_id = payerId,
        };

        _payment = new Payment()
        {
            id = paymentId,
        };

        return _payment.Execute(apiContext, paymentExecution);
    }

    private async Task<Payment> CreatePayment(Dictionary<int, int> shoppingCart, APIContext apiContext, string redirectUrl)
    {
        var itemList = new ItemList()
        {
            items = new List<Item>()
        };

        var products = _mapper.Map<List<Models.CartItem>>(
            _context.Products.Where(x => shoppingCart.Keys.Contains(x.Id))
                .ToList());

        products.ForEach(x => x.Quantity = shoppingCart[x.Id]);

        foreach (var item in products)
        {
            var randomGenerator = new Random();
            int randomSku = randomGenerator.Next(10000, 99999);

            itemList.items.Add(new Item()
            {
                name = item.ProductName,
                currency = "USD",
                price = item.Price.ToString(),
                quantity = item.Quantity.ToString(),
                sku = randomSku.ToString()
            });
        }

        var claim = _httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "Id");
        var userId = claim?.Value;

        var user = _context.Users.Find(Convert.ToInt32(userId)) ?? new();

        using var transaction = await _context.Database.BeginTransactionAsync();

        var order = new Core.Order()
        {
            User = user,
            OrderDate = DateTime.Now,
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var orderedProducts = new List<OrderProduct>();
        products.ForEach(x => orderedProducts.Add(new OrderProduct
        {
            OrdersId = order.Id,
            ProductsId = x.Id,
            Quantity = x.Quantity,
        }));

        await _context.OrderProducts.AddRangeAsync(orderedProducts);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        var payer = new Payer()
        {
            payment_method = "paypal"
        };

        var redirectUrls = new RedirectUrls()
        {
            cancel_url = redirectUrl + "&Cancel=true",
            return_url = redirectUrl
        };

        var amount = new Amount()
        {
            currency = "USD",
            total = products.Select(x => x.TotalPrice).Sum().ToString()
        };

        var tramsactionList = new List<Transaction>();

        tramsactionList.Add(new Transaction()
        {
            description = $"Transaction for order with id {order.Id}",
            invoice_number = Guid.NewGuid().ToString(),
            amount = amount,
            item_list = itemList
        });

        _payment = new Payment()
        {
            intent = "sale",
            payer = payer,
            transactions = tramsactionList,
            redirect_urls = redirectUrls
        };

        return _payment.Create(apiContext);
    }
}
