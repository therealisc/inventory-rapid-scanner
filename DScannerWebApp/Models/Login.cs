using System.ComponentModel.DataAnnotations;

namespace RCommerce.WebApp.Models;

public class Login
{
    [EmailAddress]
    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;
}
