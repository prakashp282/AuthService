using System.ComponentModel.DataAnnotations;

namespace Auth.Models.Dtos
{
    public class VerifyChallengeDto
    {
        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        public string Otp { get; set; }
    }
}