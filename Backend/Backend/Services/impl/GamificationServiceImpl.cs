using Backend.common;
using Backend.Common;
using Backend.Data;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.impl
{
    public class GamificationServiceImpl : GamificationService
    {
        private const int XpBand = 300;
        private const int MaxLevel = 99;
        private const int LessonXp = 28;
        private const int SpeakingXp = 32;

        private readonly AppDbContext _db;
        private readonly GamificationRepository _gm;
        private readonly LearningDashboardRepository _dash;
        private readonly UserContextUtil _user;

        public GamificationServiceImpl(
            AppDbContext db,
            GamificationRepository gm,
            LearningDashboardRepository dash,
            UserContextUtil user)
        {
            _db = db;
            _gm = gm;
            _dash = dash;
            _user = user;
        }

        public async Task<GamificationStateDto> GetMyStateAsync()
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var today = DateTime.UtcNow.Date;
            await EnsureProfileAsync(userId);
            await SyncDailyChallengesAsync(userId, today);
            await _gm.SaveChangesAsync();

            return await BuildStateDtoAsync(userId, today);
        }

        public async Task<GamificationDashboardSupplementDto> GetDashboardSupplementAsync(
            int userId,
            DateTime todayUtc,
            DateTime chartStartUtc)
        {
            if (userId <= 0)
            {
                return new GamificationDashboardSupplementDto();
            }

            var today = todayUtc.Date;
            var chartStart = chartStartUtc.Date;
            await EnsureProfileAsync(userId);
            await SyncDailyChallengesAsync(userId, today);
            await _gm.SaveChangesAsync();

            var profile = await _gm.GetProfileTrackedAsync(userId);
            var totalXp = profile?.TotalXp ?? 0;
            var streak = profile?.CurrentStreak ?? 0;

            var ledgerDaily = await _gm.SumLedgerByDayAsync(
                userId,
                chartStart,
                today,
                new[] { GamificationRules.SourceQuiz });
            var xpTodayRewards = await _gm.SumLedgerOnDateExcludingAsync(userId, today, GamificationRules.SourceQuiz);
            var ledgerDates = await _gm.GetLedgerActivityDatesAsync(userId, today.AddDays(-400));
            var progressDates = await _dash.GetActivityDatesUtcAsync(userId, today.AddDays(-400));
            foreach (var d in ledgerDates)
            {
                progressDates.Add(d);
            }

            var dailyRows = await _gm.ListUserDailyChallengesTrackedAsync(userId, today);
            var defs = await _gm.ListDailyChallengeDefinitionsAsync();
            var challenges = new List<DashboardChallengeDto>();
            foreach (var def in defs)
            {
                var row = dailyRows.FirstOrDefault(r => r.DailyChallengeDefinitionId == def.DailyChallengeDefinitionId);
                var progress = row?.Progress ?? 0;
                var completed = row?.CompletedUtc != null;
                challenges.Add(
                    new DashboardChallengeDto
                    {
                        Id = def.Code,
                        Title = def.Title,
                        Description = def.Description,
                        Current = completed ? def.TargetValue : progress,
                        Target = def.TargetValue,
                        Completed = completed,
                    });
            }

            var ach = await BuildAchievementDashboardRowsAsync(userId);

            return new GamificationDashboardSupplementDto
            {
                TotalXp = totalXp,
                StreakDays = streak,
                XpTodayNonQuizRewards = xpTodayRewards,
                LedgerXpByDay = ledgerDaily.ToDictionary(k => k.Key.ToString("yyyy-MM-dd"), v => (decimal)v.Value),
                Challenges = challenges,
                Achievements = ach,
            };
        }

        public async Task<IReadOnlyList<GamificationLeaderboardRowDto>> GetLeaderboardAsync(int take)
        {
            var raw = await _gm.ListLeaderboardAsync(take);
            var list = new List<GamificationLeaderboardRowDto>();
            var rank = 1;
            foreach (var row in raw)
            {
                list.Add(
                    new GamificationLeaderboardRowDto
                    {
                        Rank = rank++,
                        UserId = row.UserId,
                        Name = row.Name,
                        AvatarUrl = row.AvatarUrl,
                        TotalXp = row.TotalXp,
                        DisplayLevel = row.DisplayLevel,
                        CurrentStreak = row.CurrentStreak,
                    });
            }

            return list;
        }

        public Task RecordLessonCompletedAsync(int userId, int lessonId)
        {
            return ApplyXpEventAsync(userId, LessonXp, GamificationRules.SourceLesson, $"lesson:{lessonId}");
        }

        public Task RecordSpeakingEvaluatedAsync(int userId, int attemptId)
        {
            return ApplyXpEventAsync(userId, SpeakingXp, GamificationRules.SourceSpeaking, $"speaking:{attemptId}");
        }

        public Task RecordQuizPassedAsync(int userId, int quizId, decimal scorePercent)
        {
            var xp = (int)Math.Clamp((int)(scorePercent / 3m), 12, 55);
            return ApplyXpEventAsync(userId, xp, GamificationRules.SourceQuiz, $"quiz:{quizId}");
        }

        private async Task<GamificationStateDto> BuildStateDtoAsync(int userId, DateTime today)
        {
            var profile = await _gm.GetProfileTrackedAsync(userId);
            var profDto = MapProfile(profile);
            var achievements = await BuildAchievementRowsAsync(userId);
            var badges = await BuildBadgeRowsAsync(userId);
            var daily = await BuildDailyDtosAsync(userId, today);

            return new GamificationStateDto
            {
                Profile = profDto,
                Achievements = achievements,
                Badges = badges,
                DailyChallenges = daily,
            };
        }

        private static GamificationProfileDto MapProfile(UserGamificationProfile? p)
        {
            if (p == null)
            {
                return new GamificationProfileDto();
            }

            var lvl = ComputeLevel(p.TotalXp);
            var into = p.TotalXp % XpBand;
            return new GamificationProfileDto
            {
                TotalXp = p.TotalXp,
                DisplayLevel = lvl,
                XpIntoCurrentLevel = into,
                XpRequiredForNextLevel = XpBand,
                CurrentStreak = p.CurrentStreak,
                LongestStreak = p.LongestStreak,
                LastActivityDate = p.LastActivityDate,
            };
        }

        private static int ComputeLevel(int totalXp)
        {
            return Math.Min(MaxLevel, 1 + totalXp / XpBand);
        }

        private async Task ApplyXpEventAsync(int userId, int amount, string source, string refKey)
        {
            if (userId <= 0 || amount <= 0)
            {
                return;
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                await EnsureProfileAsync(userId);
                var profile = await _gm.GetProfileTrackedAsync(userId);
                if (profile == null)
                {
                    await tx.RollbackAsync();
                    return;
                }

                var today = DateTime.UtcNow.Date;
                ApplyStreak(profile, today);

                profile.TotalXp += amount;
                profile.DisplayLevel = ComputeLevel(profile.TotalXp);
                profile.UpdatedUtc = DateTime.UtcNow;

                await _gm.AddLedgerEntryAsync(
                    new XpLedgerEntry
                    {
                        UserId = userId,
                        Amount = amount,
                        Source = source,
                        RefKey = refKey,
                    });

                await _gm.SaveChangesAsync();
                await UnlockAchievementsAndBadgesAsync(userId, profile);
                await _gm.SaveChangesAsync();
                await SyncDailyChallengesAsync(userId, today);
                await CompleteDueDailyChallengesAsync(userId, profile, today);
                await _gm.SaveChangesAsync();
                await UnlockAchievementsAndBadgesAsync(userId, profile);
                await _gm.SaveChangesAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static void ApplyStreak(UserGamificationProfile profile, DateTime todayUtc)
        {
            var today = todayUtc.Date;
            if (profile.LastActivityDate == null)
            {
                profile.CurrentStreak = 1;
            }
            else
            {
                var last = profile.LastActivityDate.Value.Date;
                if (last == today)
                {
                    /* already counted today */
                }
                else if (last == today.AddDays(-1))
                {
                    profile.CurrentStreak += 1;
                }
                else
                {
                    profile.CurrentStreak = 1;
                }
            }

            profile.LastActivityDate = today;
            profile.LongestStreak = Math.Max(profile.LongestStreak, profile.CurrentStreak);
            profile.UpdatedUtc = DateTime.UtcNow;
        }

        private async Task EnsureProfileAsync(int userId)
        {
            var profile = await _gm.GetProfileTrackedAsync(userId);
            if (profile != null)
            {
                if (!profile.LegacyXpImported)
                {
                    var legacy = await _gm.SumLegacyQuizScoresAsync(userId);
                    profile.TotalXp = (int)Math.Clamp((int)Math.Round(legacy, MidpointRounding.AwayFromZero), 0, int.MaxValue);
                    profile.LegacyXpImported = true;
                    profile.DisplayLevel = ComputeLevel(profile.TotalXp);
                    profile.UpdatedUtc = DateTime.UtcNow;
                }

                return;
            }

            var created = await _gm.CreateProfileAsync(userId);
            var legacySum = await _gm.SumLegacyQuizScoresAsync(userId);
            created.TotalXp = (int)Math.Clamp((int)Math.Round(legacySum, MidpointRounding.AwayFromZero), 0, int.MaxValue);
            created.LegacyXpImported = true;
            created.DisplayLevel = ComputeLevel(created.TotalXp);
            created.UpdatedUtc = DateTime.UtcNow;
        }

        private async Task UnlockAchievementsAndBadgesAsync(int userId, UserGamificationProfile profile)
        {
            var unlocked = await _gm.GetUnlockedAchievementIdsAsync(userId);
            var defs = await _gm.ListAchievementDefinitionsAsync();
            var quizzes = await _dash.CountPassedQuizzesAsync(userId);
            var units = await _dash.CountUnitsCompletedAsync(userId);
            var lessons = await _gm.CountLessonsCompletedAsync(userId);
            var speaking = await _gm.CountSpeakingAttemptsAsync(userId);

            foreach (var d in defs)
            {
                if (unlocked.Contains(d.AchievementDefinitionId))
                {
                    continue;
                }

                if (!IsAchievementRuleMet(d, profile, quizzes, units, lessons, speaking))
                {
                    continue;
                }

                await _gm.AddUserAchievementAsync(
                    new UserAchievement
                    {
                        UserId = userId,
                        AchievementDefinitionId = d.AchievementDefinitionId,
                    });
                unlocked.Add(d.AchievementDefinitionId);

                if (d.XpReward > 0)
                {
                    profile.TotalXp += d.XpReward;
                    profile.DisplayLevel = ComputeLevel(profile.TotalXp);
                    await _gm.AddLedgerEntryAsync(
                        new XpLedgerEntry
                        {
                            UserId = userId,
                            Amount = d.XpReward,
                            Source = GamificationRules.SourceAchievement,
                            RefKey = $"ach:{d.Code}",
                        });
                }
            }

            var achTotal = unlocked.Count;
            var earnedBadges = await _gm.GetEarnedBadgeIdsAsync(userId);
            foreach (var b in await _gm.ListBadgeDefinitionsAsync())
            {
                if (earnedBadges.Contains(b.BadgeDefinitionId))
                {
                    continue;
                }

                if (!IsBadgeRuleMet(b, profile, achTotal))
                {
                    continue;
                }

                await _gm.AddUserBadgeAsync(
                    new UserBadge
                    {
                        UserId = userId,
                        BadgeDefinitionId = b.BadgeDefinitionId,
                    });
                earnedBadges.Add(b.BadgeDefinitionId);
            }
        }

        private static bool IsAchievementRuleMet(
            AchievementDefinition d,
            UserGamificationProfile profile,
            int quizzes,
            int units,
            int lessons,
            int speaking)
        {
            return d.RuleType switch
            {
                GamificationRules.TotalXp => profile.TotalXp >= d.RuleThreshold,
                GamificationRules.StreakDays => profile.CurrentStreak >= d.RuleThreshold,
                GamificationRules.QuizzesPassed => quizzes >= d.RuleThreshold,
                GamificationRules.UnitsCompleted => units >= d.RuleThreshold,
                GamificationRules.LessonsCompleted => lessons >= d.RuleThreshold,
                GamificationRules.SpeakingAttempts => speaking >= d.RuleThreshold,
                _ => false,
            };
        }

        private static bool IsBadgeRuleMet(BadgeDefinition b, UserGamificationProfile profile, int achievementCount)
        {
            return b.RuleType switch
            {
                GamificationRules.TotalXp => profile.TotalXp >= b.RuleThreshold,
                GamificationRules.StreakDays => profile.CurrentStreak >= b.RuleThreshold,
                GamificationRules.AchievementsUnlocked => achievementCount >= b.RuleThreshold,
                _ => false,
            };
        }

        private async Task SyncDailyChallengesAsync(int userId, DateTime today)
        {
            var defs = await _gm.ListDailyChallengeDefinitionsAsync();
            var rows = await _gm.ListUserDailyChallengesTrackedAsync(userId, today);
            foreach (var def in defs)
            {
                if (rows.Any(r => r.DailyChallengeDefinitionId == def.DailyChallengeDefinitionId))
                {
                    continue;
                }

                await _gm.AddUserDailyChallengeAsync(
                    new UserDailyChallenge
                    {
                        UserId = userId,
                        ChallengeDate = today,
                        DailyChallengeDefinitionId = def.DailyChallengeDefinitionId,
                        Progress = 0,
                    });
            }

            await _gm.SaveChangesAsync();
            rows = await _gm.ListUserDailyChallengesTrackedAsync(userId, today);
            var defById = defs.ToDictionary(x => x.DailyChallengeDefinitionId);
            var ledgerToday = await _gm.SumLedgerOnDateAsync(userId, today);
            var quizzesToday = await _dash.CountPassedQuizzesOnDateAsync(userId, today);
            var lessonsToday = await _gm.CountLessonsCompletedOnDateAsync(userId, today);
            var speakingToday = await _db.SpeakingAttempts
                .AsNoTracking()
                .CountAsync(x =>
                    x.UserId == userId
                    && x.CreatedUtc >= today
                    && x.CreatedUtc < today.AddDays(1));

            foreach (var row in rows)
            {
                if (row.CompletedUtc != null)
                {
                    continue;
                }

                if (!defById.TryGetValue(row.DailyChallengeDefinitionId, out var def))
                {
                    continue;
                }

                row.Progress = def.TargetKind switch
                {
                    GamificationRules.TargetQuizzesToday => quizzesToday,
                    GamificationRules.TargetXpToday => ledgerToday,
                    GamificationRules.TargetLessonsToday => lessonsToday,
                    GamificationRules.TargetSpeakingToday => speakingToday,
                    GamificationRules.TargetUnitsToday => await _dash.CountUnitsCompletedOnDateAsync(userId, today),
                    _ => row.Progress,
                };

                if (row.Progress > def.TargetValue)
                {
                    row.Progress = def.TargetValue;
                }
            }
        }

        private async Task CompleteDueDailyChallengesAsync(
            int userId,
            UserGamificationProfile profile,
            DateTime today)
        {
            var defs = await _gm.ListDailyChallengeDefinitionsAsync();
            var defById = defs.ToDictionary(x => x.DailyChallengeDefinitionId);
            var rows = await _gm.ListUserDailyChallengesTrackedAsync(userId, today);

            foreach (var row in rows)
            {
                if (row.CompletedUtc != null)
                {
                    continue;
                }

                if (!defById.TryGetValue(row.DailyChallengeDefinitionId, out var def))
                {
                    continue;
                }

                if (row.Progress < def.TargetValue || def.XpReward <= 0)
                {
                    continue;
                }

                row.CompletedUtc = DateTime.UtcNow;
                profile.TotalXp += def.XpReward;
                profile.DisplayLevel = ComputeLevel(profile.TotalXp);
                await _gm.AddLedgerEntryAsync(
                    new XpLedgerEntry
                    {
                        UserId = userId,
                        Amount = def.XpReward,
                        Source = GamificationRules.SourceDaily,
                        RefKey = $"daily:{def.Code}:{today:yyyyMMdd}",
                    });
            }
        }

        private async Task<List<GamificationAchievementDto>> BuildAchievementRowsAsync(int userId)
        {
            var defs = await _gm.ListAchievementDefinitionsAsync();
            var unlocked = await _gm.GetUnlockedAchievementIdsAsync(userId);
            var times = await _db.UserAchievements
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToDictionaryAsync(x => x.AchievementDefinitionId, x => x.UnlockedUtc);

            return defs
                .Select(d => new GamificationAchievementDto
                {
                    Code = d.Code,
                    Title = d.Title,
                    Description = d.Description,
                    IconKey = d.IconKey,
                    Unlocked = unlocked.Contains(d.AchievementDefinitionId),
                    UnlockedUtc = times.TryGetValue(d.AchievementDefinitionId, out var u) ? u : null,
                    XpReward = d.XpReward,
                })
                .ToList();
        }

        private async Task<List<DashboardAchievementDto>> BuildAchievementDashboardRowsAsync(int userId)
        {
            var rows = await BuildAchievementRowsAsync(userId);
            return rows
                .Select(x => new DashboardAchievementDto
                {
                    Id = x.Code,
                    Title = x.Title,
                    Description = x.Description,
                    Icon = MapIcon(x.IconKey),
                    Unlocked = x.Unlocked,
                })
                .ToList();
        }

        private static string MapIcon(string key) =>
            key switch
            {
                "target" => "🎯",
                "star" => "⭐",
                "flame" => "🔥",
                "brick" => "🧱",
                "book" => "📚",
                "mic" => "🎙️",
                "bolt" => "⚡",
                _ => "✨",
            };

        private async Task<List<GamificationBadgeDto>> BuildBadgeRowsAsync(int userId)
        {
            var defs = await _gm.ListBadgeDefinitionsAsync();
            var earned = await _gm.GetEarnedBadgeIdsAsync(userId);
            var times = await _db.UserBadges
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToDictionaryAsync(x => x.BadgeDefinitionId, x => x.EarnedUtc);

            return defs
                .Select(b => new GamificationBadgeDto
                {
                    Code = b.Code,
                    Title = b.Title,
                    Description = b.Description,
                    Tier = b.Tier,
                    Earned = earned.Contains(b.BadgeDefinitionId),
                    EarnedUtc = times.TryGetValue(b.BadgeDefinitionId, out var t) ? t : null,
                })
                .ToList();
        }

        private async Task<List<GamificationDailyChallengeDto>> BuildDailyDtosAsync(int userId, DateTime today)
        {
            var defs = await _gm.ListDailyChallengeDefinitionsAsync();
            var rows = await _gm.ListUserDailyChallengesTrackedAsync(userId, today);
            var list = new List<GamificationDailyChallengeDto>();
            foreach (var def in defs)
            {
                var row = rows.FirstOrDefault(r => r.DailyChallengeDefinitionId == def.DailyChallengeDefinitionId);
                var completed = row?.CompletedUtc != null;
                list.Add(
                    new GamificationDailyChallengeDto
                    {
                        Code = def.Code,
                        Title = def.Title,
                        Description = def.Description,
                        Progress = row?.Progress ?? 0,
                        Target = def.TargetValue,
                        Completed = completed,
                        XpReward = def.XpReward,
                    });
            }

            return list;
        }
    }
}
