using RCommerce.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using RCommerce.WebApp.Models;
using Microsoft.AspNetCore.Identity;

namespace RCommerce.WebApp.Controllers;

public class SignupController : Controller
{
    private readonly RCommerceContext _context;

    public SignupController(RCommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new Signup());
    }

    [HttpPost]
    public async Task<IActionResult> Index(Signup signup)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "There were some errors in your form");
            return View(signup);
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == signup.Username);

        if (user != null)
        {
            ModelState.AddModelError(string.Empty, "Username already beeing used");
            return View(signup);
        }

        var hasher = new PasswordHasher<IdentityUser>();

        var identityUser = new IdentityUser()
        {
            UserName = signup.Username,
            PasswordHash = signup.Password
        };

        var passwordHash = hasher.HashPassword(identityUser, signup.Password);

        user = new Core.User
        {
            Email = signup.Username,
            Password = passwordHash,
            IsAdmin = true,
        };

        await _context.AddAsync(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Login");
    }
}
