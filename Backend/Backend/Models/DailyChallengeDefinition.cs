using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("DailyChallengeDefinitions")]
    public class DailyChallengeDefinition
    {
        public int DailyChallengeDefinitionId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string TargetKind { get; set; } = string.Empty;

        public int TargetValue { get; set; }

        public int XpReward { get; set; }

        public int SortOrder { get; set; }

        public ICollection<UserDailyChallenge>? UserDailyChallenges { get; set; }
    }
}
