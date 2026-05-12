using Backend.Models;

namespace Backend.Repository
{
    public interface GamificationRepository
    {
        Task<UserGamificationProfile?> GetProfileTrackedAsync(int userId);

        Task<UserGamificationProfile> CreateProfileAsync(int userId);

        Task<decimal> SumLegacyQuizScoresAsync(int userId);

        Task AddLedgerEntryAsync(XpLedgerEntry entry);

        Task<int> SumLedgerOnDateAsync(int userId, DateTime dayUtc);

        Task<int> SumLedgerOnDateExcludingAsync(int userId, DateTime dayUtc, params string[] excludeSources);

        Task<Dictionary<DateTime, int>> SumLedgerByDayAsync(
            int userId,
            DateTime startUtc,
            DateTime endUtc,
            string[]? excludeSources = null);

        Task<HashSet<DateTime>> GetLedgerActivityDatesAsync(int userId, DateTime sinceUtc);

        Task<List<AchievementDefinition>> ListAchievementDefinitionsAsync();

        Task<HashSet<int>> GetUnlockedAchievementIdsAsync(int userId);

        Task<bool> HasAchievementAsync(int userId, int definitionId);

        Task AddUserAchievementAsync(UserAchievement row);

        Task<List<BadgeDefinition>> ListBadgeDefinitionsAsync();

        Task<HashSet<int>> GetEarnedBadgeIdsAsync(int userId);

        Task<bool> HasBadgeAsync(int userId, int badgeDefinitionId);

        Task AddUserBadgeAsync(UserBadge row);

        Task<List<DailyChallengeDefinition>> ListDailyChallengeDefinitionsAsync();

        Task<List<UserDailyChallenge>> ListUserDailyChallengesTrackedAsync(int userId, DateTime challengeDate);

        Task AddUserDailyChallengeAsync(UserDailyChallenge row);

        Task<int> CountLessonsCompletedAsync(int userId);

        Task<int> CountLessonsCompletedOnDateAsync(int userId, DateTime dayUtc);

        Task<int> CountSpeakingAttemptsAsync(int userId);

        Task<List<(int UserId, string Name, string? AvatarUrl, int TotalXp, int DisplayLevel, int CurrentStreak)>>
            ListLeaderboardAsync(int take);

        Task<int> SaveChangesAsync();
    }
}
