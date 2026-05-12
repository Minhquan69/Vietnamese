namespace Backend.dto
{
    public class GamificationProfileDto
    {
        public int TotalXp { get; set; }

        public int DisplayLevel { get; set; }

        public int XpIntoCurrentLevel { get; set; }

        public int XpRequiredForNextLevel { get; set; }

        public int CurrentStreak { get; set; }

        public int LongestStreak { get; set; }

        public DateTime? LastActivityDate { get; set; }
    }

    public class GamificationAchievementDto
    {
        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconKey { get; set; } = string.Empty;

        public bool Unlocked { get; set; }

        public DateTime? UnlockedUtc { get; set; }

        public int XpReward { get; set; }
    }

    public class GamificationBadgeDto
    {
        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Tier { get; set; }

        public bool Earned { get; set; }

        public DateTime? EarnedUtc { get; set; }
    }

    public class GamificationDailyChallengeDto
    {
        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Progress { get; set; }

        public int Target { get; set; }

        public bool Completed { get; set; }

        public int XpReward { get; set; }
    }

    public class GamificationLeaderboardRowDto
    {
        public int Rank { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public int TotalXp { get; set; }

        public int DisplayLevel { get; set; }

        public int CurrentStreak { get; set; }
    }

    public class GamificationStateDto
    {
        public GamificationProfileDto Profile { get; set; } = new();

        public List<GamificationAchievementDto> Achievements { get; set; } = new();

        public List<GamificationBadgeDto> Badges { get; set; } = new();

        public List<GamificationDailyChallengeDto> DailyChallenges { get; set; } = new();
    }

    /// <summary>Extra data merged into the learning dashboard.</summary>
    public class GamificationDashboardSupplementDto
    {
        public int TotalXp { get; set; }

        public int StreakDays { get; set; }

        /// <summary>Non-quiz ledger XP per day (merge with quiz-based series to avoid double counting).</summary>
        public Dictionary<string, decimal> LedgerXpByDay { get; set; } = new();

        /// <summary>Reward ledger XP today excluding quiz_pass (add to legacy quiz XP for today bar).</summary>
        public int XpTodayNonQuizRewards { get; set; }

        public List<DashboardChallengeDto> Challenges { get; set; } = new();

        public List<DashboardAchievementDto> Achievements { get; set; } = new();
    }
}
