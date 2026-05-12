using Backend.common;
using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class GamificationRepositoryImpl : GamificationRepository
    {
        private readonly AppDbContext _db;

        public GamificationRepositoryImpl(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserGamificationProfile?> GetProfileTrackedAsync(int userId)
        {
            return await _db.UserGamificationProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserGamificationProfile> CreateProfileAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var row = new UserGamificationProfile
            {
                UserId = userId,
                TotalXp = 0,
                DisplayLevel = 1,
                CurrentStreak = 0,
                LongestStreak = 0,
                LegacyXpImported = false,
                CreatedUtc = now,
                UpdatedUtc = now,
            };

            await _db.UserGamificationProfiles.AddAsync(row);
            return row;
        }

        public async Task<decimal> SumLegacyQuizScoresAsync(int userId)
        {
            return await _db.UserQuiz
                .AsNoTracking()
                .Where(q => q.UserId == userId)
                .SumAsync(q => (decimal?)q.Score) ?? 0m;
        }

        public async Task AddLedgerEntryAsync(XpLedgerEntry entry)
        {
            entry.CreatedUtc = DateTime.UtcNow;
            await _db.XpLedger.AddAsync(entry);
        }

        public async Task<int> SumLedgerOnDateAsync(int userId, DateTime dayUtc)
        {
            var start = dayUtc.Date;
            var end = start.AddDays(1);
            return await _db.XpLedger
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.CreatedUtc >= start && x.CreatedUtc < end)
                .SumAsync(x => (int?)x.Amount) ?? 0;
        }

        public async Task<int> SumLedgerOnDateExcludingAsync(int userId, DateTime dayUtc, params string[] excludeSources)
        {
            var start = dayUtc.Date;
            var end = start.AddDays(1);
            var q = _db.XpLedger
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.CreatedUtc >= start && x.CreatedUtc < end);
            if (excludeSources is { Length: > 0 })
            {
                q = q.Where(x => !excludeSources.Contains(x.Source));
            }

            return await q.SumAsync(x => (int?)x.Amount) ?? 0;
        }

        public async Task<Dictionary<DateTime, int>> SumLedgerByDayAsync(
            int userId,
            DateTime startUtc,
            DateTime endUtc,
            string[]? excludeSources = null)
        {
            var start = startUtc.Date;
            var end = endUtc.Date.AddDays(1);
            var q = _db.XpLedger
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.CreatedUtc >= start && x.CreatedUtc < end);
            if (excludeSources is { Length: > 0 })
            {
                q = q.Where(x => !excludeSources.Contains(x.Source));
            }

            var rows = await q
                .GroupBy(x => x.CreatedUtc.Date)
                .Select(g => new { Day = g.Key, Xp = g.Sum(y => y.Amount) })
                .ToListAsync();

            return rows.ToDictionary(r => r.Day, r => r.Xp);
        }

        public async Task<HashSet<DateTime>> GetLedgerActivityDatesAsync(int userId, DateTime sinceUtc)
        {
            var since = sinceUtc.Date;
            var dates = await _db.XpLedger
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.CreatedUtc >= since)
                .Select(x => x.CreatedUtc.Date)
                .Distinct()
                .ToListAsync();

            return dates.ToHashSet();
        }

        public async Task<List<AchievementDefinition>> ListAchievementDefinitionsAsync()
        {
            return await _db.AchievementDefinitions
                .AsNoTracking()
                .OrderBy(x => x.AchievementDefinitionId)
                .ToListAsync();
        }

        public async Task<HashSet<int>> GetUnlockedAchievementIdsAsync(int userId)
        {
            var ids = await _db.UserAchievements
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.AchievementDefinitionId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<bool> HasAchievementAsync(int userId, int definitionId)
        {
            return await _db.UserAchievements
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.AchievementDefinitionId == definitionId);
        }

        public async Task AddUserAchievementAsync(UserAchievement row)
        {
            row.UnlockedUtc = DateTime.UtcNow;
            await _db.UserAchievements.AddAsync(row);
        }

        public async Task<List<BadgeDefinition>> ListBadgeDefinitionsAsync()
        {
            return await _db.BadgeDefinitions
                .AsNoTracking()
                .OrderBy(x => x.BadgeDefinitionId)
                .ToListAsync();
        }

        public async Task<HashSet<int>> GetEarnedBadgeIdsAsync(int userId)
        {
            var ids = await _db.UserBadges
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.BadgeDefinitionId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<bool> HasBadgeAsync(int userId, int badgeDefinitionId)
        {
            return await _db.UserBadges
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.BadgeDefinitionId == badgeDefinitionId);
        }

        public async Task AddUserBadgeAsync(UserBadge row)
        {
            row.EarnedUtc = DateTime.UtcNow;
            await _db.UserBadges.AddAsync(row);
        }

        public async Task<List<DailyChallengeDefinition>> ListDailyChallengeDefinitionsAsync()
        {
            return await _db.DailyChallengeDefinitions
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DailyChallengeDefinitionId)
                .ToListAsync();
        }

        public async Task<List<UserDailyChallenge>> ListUserDailyChallengesTrackedAsync(
            int userId,
            DateTime challengeDate)
        {
            var d = challengeDate.Date;
            return await _db.UserDailyChallenges
                .Where(x => x.UserId == userId && x.ChallengeDate == d)
                .ToListAsync();
        }

        public async Task AddUserDailyChallengeAsync(UserDailyChallenge row)
        {
            row.ChallengeDate = row.ChallengeDate.Date;
            await _db.UserDailyChallenges.AddAsync(row);
        }

        public async Task<int> CountLessonsCompletedAsync(int userId)
        {
            return await _db.UserProgress
                .AsNoTracking()
                .CountAsync(p =>
                    p.UserId == userId
                    && p.RefType == Constant.RefType.Lesson
                    && p.Status
                    && p.CompletedDate != null);
        }

        public async Task<int> CountLessonsCompletedOnDateAsync(int userId, DateTime dayUtc)
        {
            var start = dayUtc.Date;
            var end = start.AddDays(1);
            return await _db.UserProgress
                .AsNoTracking()
                .CountAsync(p =>
                    p.UserId == userId
                    && p.RefType == Constant.RefType.Lesson
                    && p.Status
                    && p.CompletedDate != null
                    && p.CompletedDate >= start
                    && p.CompletedDate < end);
        }

        public async Task<int> CountSpeakingAttemptsAsync(int userId)
        {
            return await _db.SpeakingAttempts.AsNoTracking().CountAsync(x => x.UserId == userId);
        }

        public async Task<List<(int UserId, string Name, string? AvatarUrl, int TotalXp, int DisplayLevel, int CurrentStreak)>>
            ListLeaderboardAsync(int take)
        {
            var cap = Math.Clamp(take, 1, 200);
            var rows = await (
                    from u in _db.Users.AsNoTracking()
                    join r in _db.Roles.AsNoTracking() on u.RoleId equals r.RoleId
                    join p in _db.UserGamificationProfiles.AsNoTracking() on u.UserId equals p.UserId into gp
                    from p in gp.DefaultIfEmpty()
                    where r.RoleName == Constant.Role.User && u.Status == 1
                    orderby (p != null ? p.TotalXp : 0) descending, u.Name
                    select new
                    {
                        u.UserId,
                        u.Name,
                        u.AvatarUrl,
                        TotalXp = p != null ? p.TotalXp : 0,
                        DisplayLevel = p != null ? p.DisplayLevel : 1,
                        CurrentStreak = p != null ? p.CurrentStreak : 0,
                    })
                .Take(cap)
                .ToListAsync();

            return rows
                .Select(x => (x.UserId, x.Name, x.AvatarUrl, x.TotalXp, x.DisplayLevel, x.CurrentStreak))
                .ToList();
        }

        public Task<int> SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
