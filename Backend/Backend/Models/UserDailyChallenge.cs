using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("UserDailyChallenges")]
    public class UserDailyChallenge
    {
        public int UserDailyChallengeId { get; set; }

        public int UserId { get; set; }

        public DateTime ChallengeDate { get; set; }

        public int DailyChallengeDefinitionId { get; set; }

        public int Progress { get; set; }

        public DateTime? CompletedUtc { get; set; }

        public User? User { get; set; }

        public DailyChallengeDefinition? Definition { get; set; }
    }
}
