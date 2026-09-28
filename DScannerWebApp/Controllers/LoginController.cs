using RCommerce.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RCommerce.WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


namespace RCommerce.WebApp.Controllers;

public class LoginController : Controller
{
    private readonly RCommerceContext _context;

    public LoginController(RCommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new Login());
    }

    [HttpPost]
    public async Task<IActionResult> Index(Login login)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "There were some errors in your form");
            return View(login);
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == login.Username);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid Credentials");
            return View(login);
        }

        var hasher = new PasswordHasher<IdentityUser>();

        var identityUser = new IdentityUser()
        {
            Id = user.Id.ToString(),
            UserName = user.Email,
            PasswordHash = user.Password
        };

        if (PasswordVerificationResult.Failed == hasher.VerifyHashedPassword(identityUser, identityUser.PasswordHash, login.Password))
        {
            ModelState.AddModelError("Password", "Password is wrong");
            return View(login);
        }

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, login.Username),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new Claim("Id", user.Id.ToString()),
        };

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        AuthenticationProperties properties = new AuthenticationProperties()
        {
            AllowRefresh = true,
            IsPersistent = true,
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
