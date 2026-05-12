namespace Backend.Models
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresUtc { get; set; }
        public DateTime? RevokedUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        public User? User { get; set; }
    }
}
