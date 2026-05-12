using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("UserAchievements")]
    public class UserAchievement
    {
        public int UserId { get; set; }

        public int AchievementDefinitionId { get; set; }

        public DateTime UnlockedUtc { get; set; }

        public User? User { get; set; }

        public AchievementDefinition? Definition { get; set; }
    }
}
