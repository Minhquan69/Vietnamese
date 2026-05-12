using System.ComponentModel.DataAnnotations;

namespace Backend.dto
{
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MinLength(20)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).+$", ErrorMessage = "Password must contain letters and numbers")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
