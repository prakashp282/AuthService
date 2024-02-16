using System.ComponentModel.DataAnnotations;

namespace Auth.Models.Dtos
{
    public class SignInDto
    {
        [Required] [EmailAddress] public string Email { get; set; }

        [Required] [MinLength(8)] public string Password { get; set; }
    }
}