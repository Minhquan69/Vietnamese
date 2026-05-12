using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("UserGamificationProfiles")]
    public class UserGamificationProfile
    {
        [Key]
        public int UserId { get; set; }

        public int TotalXp { get; set; }

        public int DisplayLevel { get; set; } = 1;

        public int CurrentStreak { get; set; }

        public int LongestStreak { get; set; }

        public DateTime? LastActivityDate { get; set; }

        public bool LegacyXpImported { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public User? User { get; set; }
    }
}
