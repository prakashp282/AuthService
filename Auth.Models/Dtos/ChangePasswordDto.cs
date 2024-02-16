using System.ComponentModel.DataAnnotations;

namespace Auth.Models.Dtos
{
    public class ChangePasswordDto : SignInDto
    {
        /// <summary>
        /// The new Password
        /// </summary>
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }
}