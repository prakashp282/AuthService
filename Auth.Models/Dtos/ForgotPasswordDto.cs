using System.ComponentModel.DataAnnotations;

namespace Auth.Models.Dtos
{
    public class ForgotPasswordDto
    {
        /// <summary>
        /// The Email the mail would be se
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}