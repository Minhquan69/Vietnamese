using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("UserBadges")]
    public class UserBadge
    {
        public int UserId { get; set; }

        public int BadgeDefinitionId { get; set; }

        public DateTime EarnedUtc { get; set; }

        public User? User { get; set; }

        public BadgeDefinition? Definition { get; set; }
    }
}
