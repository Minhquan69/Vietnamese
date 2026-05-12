using Backend.Common;
using Backend.dto;
using Backend.Repository;
using Backend.Services;

namespace Backend.Services.impl
{
    public class LearningDashboardServiceImpl : LearningDashboardService
    {
        private const int DailyGoalXp = 100;
        private const int ActivityChartDays = 7;
        private const int WrongAnswerLookbackDays = 14;
        private const int StreakLookbackDays = 400;

        private readonly LearningDashboardRepository _dashboardRepository;
        private readonly LearningService _learningService;
        private readonly UserContextUtil _userContext;
        private readonly GamificationService _gamification;

        public LearningDashboardServiceImpl(
            LearningDashboardRepository dashboardRepository,
            LearningService learningService,
            UserContextUtil userContext,
            GamificationService gamification)
        {
            _dashboardRepository = dashboardRepository;
            _learningService = learningService;
            _userContext = userContext;
            _gamification = gamification;
        }

        public async Task<LearningDashboardDto> GetDashboardAsync()
        {
            var userId = _userContext.GetUserId();
            var today = DateTime.UtcNow.Date;
            var chartStart = today.AddDays(-(ActivityChartDays - 1));

            GamificationDashboardSupplementDto? gm = null;
            if (userId > 0)
            {
                try
                {
                    gm = await _gamification.GetDashboardSupplementAsync(userId, today, chartStart);
                }
                catch
                {
                    gm = null;
                }
            }

            var dailyXpMap = await _dashboardRepository.GetDailyXpAsync(userId, chartStart, today);
            var activityDates = await _dashboardRepository.GetActivityDatesUtcAsync(
                userId,
                today.AddDays(-StreakLookbackDays));

            if (gm?.LedgerXpByDay != null)
            {
                foreach (var kv in gm.LedgerXpByDay)
                {
                    if (DateTime.TryParse(kv.Key, out var dt))
                    {
                        activityDates.Add(dt.Date);
                    }
                }
            }

            decimal xpTodayDec = await _dashboardRepository.GetXpOnDateAsync(userId, today);
            if (gm != null)
            {
                xpTodayDec += gm.XpTodayNonQuizRewards;
            }

            int xpTotal;
            int streak;
            if (gm != null)
            {
                xpTotal = gm.TotalXp;
                streak = Math.Max(gm.StreakDays, ComputeStreak(activityDates, today));
            }
            else
            {
                var totalXpDec = await _dashboardRepository.GetTotalXpAsync(userId);
                xpTotal = (int)Math.Round(totalXpDec, MidpointRounding.AwayFromZero);
                streak = ComputeStreak(activityDates, today);
            }

            var xpToday = (int)Math.Round(xpTodayDec, MidpointRounding.AwayFromZero);
            var goalPct = DailyGoalXp <= 0
                ? 0
                : Math.Min(100, (int)Math.Round(xpTodayDec / DailyGoalXp * 100m, MidpointRounding.AwayFromZero));

            var quizzesPassed = await _dashboardRepository.CountPassedQuizzesAsync(userId);
            var unitsDone = await _dashboardRepository.CountUnitsCompletedAsync(userId);

            var wrongSince = today.AddDays(-WrongAnswerLookbackDays);
            var wrongCount = await _dashboardRepository.CountWrongAnswersSinceAsync(userId, wrongSince);

            var series = new List<DashboardDailyPointDto>();
            for (var i = 0; i < ActivityChartDays; i++)
            {
                var day = chartStart.AddDays(i);
                dailyXpMap.TryGetValue(day, out var quizXpDay);
                var bonus = 0m;
                if (gm?.LedgerXpByDay != null
                    && gm.LedgerXpByDay.TryGetValue(day.ToString("yyyy-MM-dd"), out var ledgerPart))
                {
                    bonus = ledgerPart;
                }

                var xp = (int)Math.Round(quizXpDay + bonus, MidpointRounding.AwayFromZero);
                series.Add(
                    new DashboardDailyPointDto
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        Xp = xp,
                        HadActivity = xp > 0 || activityDates.Contains(day),
                    });
            }

            var (continues, recommended) = await BuildContinueAndRecommendedAsync();

            List<DashboardChallengeDto> challenges;
            List<DashboardAchievementDto> achievements;
            if (gm != null)
            {
                challenges = gm.Challenges;
                achievements = gm.Achievements;
            }
            else
            {
                var quizzesToday = await _dashboardRepository.CountPassedQuizzesOnDateAsync(userId, today);
                var unitsToday = await _dashboardRepository.CountUnitsCompletedOnDateAsync(userId, today);
                challenges = new List<DashboardChallengeDto>
                {
                    new()
                    {
                        Id = "daily_quiz",
                        Title = "Quiz champion",
                        Description = "Pass at least one quiz today.",
                        Current = Math.Min(1, quizzesToday),
                        Target = 1,
                        Completed = quizzesToday >= 1,
                    },
                    new()
                    {
                        Id = "daily_xp",
                        Title = "XP sprint",
                        Description = "Earn 50 XP today.",
                        Current = Math.Min(50, xpToday),
                        Target = 50,
                        Completed = xpToday >= 50,
                    },
                    new()
                    {
                        Id = "daily_unit",
                        Title = "Unit closer",
                        Description = "Complete one full unit today.",
                        Current = Math.Min(1, unitsToday),
                        Target = 1,
                        Completed = unitsToday >= 1,
                    },
                };
                achievements = BuildAchievements(xpTotal, streak, quizzesPassed, unitsDone);
            }

            DashboardVocabReminderDto? vocab = null;
            if (wrongCount > 0)
            {
                vocab = new DashboardVocabReminderDto
                {
                    ReviewCount = wrongCount,
                    Message =
                        $"{wrongCount} incorrect answers in your recent quizzes — review strengthens retention.",
                };
            }

            return new LearningDashboardDto
            {
                Stats = new DashboardStatsDto
                {
                    XpTotal = xpTotal,
                    XpToday = xpToday,
                    StreakDays = streak,
                    DailyGoalXp = DailyGoalXp,
                    DailyGoalProgressPercent = goalPct,
                    QuizzesPassedTotal = quizzesPassed,
                    UnitsCompletedTotal = unitsDone,
                },
                ActivitySeries = series,
                ContinueLearning = continues,
                Recommended = recommended,
                Challenges = challenges,
                Achievements = achievements,
                VocabReminder = vocab,
            };
        }

        private static int ComputeStreak(HashSet<DateTime> days, DateTime todayUtc)
        {
            if (days.Count == 0)
            {
                return 0;
            }

            var today = todayUtc.Date;
            var anchor = days.Contains(today) ? today : today.AddDays(-1);
            if (!days.Contains(anchor))
            {
                return 0;
            }

            var streak = 0;
            for (var d = anchor; days.Contains(d); d = d.AddDays(-1))
            {
                streak++;
            }

            return streak;
        }

        private async Task<(DashboardContinueDto? Continue, List<DashboardContinueDto> Recommended)>
            BuildContinueAndRecommendedAsync()
        {
            var levels = await _learningService.GetMyProgress();
            var flat = new List<(UnitDTO u, CourseDTO c, LevelDTO l)>();
            foreach (var level in levels.OrderBy(x => x.OrderIndex))
            {
                foreach (var course in (level.Courses ?? new List<CourseDTO>()).OrderBy(x => x.OrderIndex))
                {
                    foreach (var unit in (course.Units ?? new List<UnitDTO>()).OrderBy(x => x.OrderIndex))
                    {
                        flat.Add((unit, course, level));
                    }
                }
            }

            static DashboardContinueDto Map(UnitDTO u, CourseDTO c, LevelDTO l, bool locked) =>
                new()
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName ?? string.Empty,
                    CourseId = c.CourseId,
                    CourseName = c.CourseName ?? string.Empty,
                    LevelId = l.LevelId,
                    LevelName = l.LevelName ?? string.Empty,
                    IsLocked = locked,
                };

            var continueRow = flat.FirstOrDefault(x => x.u.Status == false);
            DashboardContinueDto? continueLearning = continueRow.u == null
                ? null
                : Map(continueRow.u, continueRow.c, continueRow.l, false);

            if (continueLearning == null)
            {
                var preview = flat.FirstOrDefault(x => x.u.Status == null);
                if (preview.u != null)
                {
                    continueLearning = Map(preview.u, preview.c, preview.l, true);
                }
            }

            var recommended = new List<DashboardContinueDto>();
            var startIdx = continueRow.u != null ? flat.FindIndex(x => x.u.UnitId == continueRow.u.UnitId) : -1;
            if (startIdx >= 0)
            {
                for (var i = startIdx + 1; i < flat.Count && recommended.Count < 3; i++)
                {
                    var row = flat[i];
                    if (row.u.Status == true)
                    {
                        continue;
                    }

                    recommended.Add(Map(row.u, row.c, row.l, row.u.Status == null));
                }
            }
            else if (continueLearning != null && continueLearning.IsLocked)
            {
                for (var i = 0; i < flat.Count && recommended.Count < 3; i++)
                {
                    var row = flat[i];
                    if (row.u.Status == true)
                    {
                        continue;
                    }

                    if (row.u.UnitId == continueLearning.UnitId)
                    {
                        continue;
                    }

                    recommended.Add(Map(row.u, row.c, row.l, row.u.Status == null));
                }
            }

            return (continueLearning, recommended);
        }

        private static List<DashboardAchievementDto> BuildAchievements(
            int xpTotal,
            int streak,
            int quizzesPassed,
            int unitsDone)
        {
            return new List<DashboardAchievementDto>
            {
                new()
                {
                    Id = "first_quiz",
                    Title = "First steps",
                    Description = "Pass your first quiz.",
                    Icon = "🎯",
                    Unlocked = quizzesPassed >= 1,
                },
                new()
                {
                    Id = "xp_500",
                    Title = "Rising star",
                    Description = "Reach 500 lifetime XP.",
                    Icon = "⭐",
                    Unlocked = xpTotal >= 500,
                },
                new()
                {
                    Id = "streak_7",
                    Title = "Week warrior",
                    Description = "Maintain a 7-day learning streak.",
                    Icon = "🔥",
                    Unlocked = streak >= 7,
                },
                new()
                {
                    Id = "units_5",
                    Title = "Path builder",
                    Description = "Complete 5 units.",
                    Icon = "🧱",
                    Unlocked = unitsDone >= 5,
                },
                new()
                {
                    Id = "quizzes_25",
                    Title = "Quiz fan",
                    Description = "Pass 25 quizzes.",
                    Icon = "📚",
                    Unlocked = quizzesPassed >= 25,
                },
            };
        }
    }
}
