using System.ComponentModel.DataAnnotations;

namespace RCommerce.WebApp.Models;

public class Signup
{
    [EmailAddress]
    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = string.Empty;

    [MinLength(5)]
    [MaxLength(50)]
    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;
}
