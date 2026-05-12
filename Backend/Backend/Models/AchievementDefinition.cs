using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("AchievementDefinitions")]
    public class AchievementDefinition
    {
        public int AchievementDefinitionId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconKey { get; set; } = "star";

        public string RuleType { get; set; } = string.Empty;

        public int RuleThreshold { get; set; }

        public int XpReward { get; set; }

        public ICollection<UserAchievement>? UserAchievements { get; set; }
    }
}
