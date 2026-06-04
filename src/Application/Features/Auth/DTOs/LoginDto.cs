using System.ComponentModel.DataAnnotations;

namespace Application.Features.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Loginni kiriting")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Parol kiritilmagan")]
    public string Password { get; set; } = null!;
}