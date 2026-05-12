using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("BadgeDefinitions")]
    public class BadgeDefinition
    {
        public int BadgeDefinitionId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public byte Tier { get; set; } = 1;

        public string RuleType { get; set; } = string.Empty;

        public int RuleThreshold { get; set; }

        public ICollection<UserBadge>? UserBadges { get; set; }
    }
}
