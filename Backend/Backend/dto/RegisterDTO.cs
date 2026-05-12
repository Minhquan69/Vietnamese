using System.ComponentModel.DataAnnotations;

namespace Backend.dto
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(2)]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).+$", ErrorMessage = "Password must contain letters and numbers")]
        public string Password { get; set; } = string.Empty;
    }
}
