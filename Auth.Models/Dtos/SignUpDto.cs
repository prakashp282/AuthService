using System.ComponentModel.DataAnnotations;

namespace Auth.Models.Dtos
{
    public class SignUpDto
    {
        [Required] [MinLength(3)] public string UserName { get; set; }

        [Required] [EmailAddress] public string Email { get; set; }

        [Required] [MinLength(8)] public string Password { get; set; }

        [Required] [MinLength(10)] public string PhoneNumber { get; set; }
    }
}