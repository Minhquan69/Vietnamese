using System.ComponentModel.DataAnnotations;

namespace Backend.dto
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public string Password { get; set; } = string.Empty;
    }
}
