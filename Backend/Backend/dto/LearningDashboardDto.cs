namespace Backend.dto
{
    public class LearningDashboardDto
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public List<DashboardDailyPointDto> ActivitySeries { get; set; } = new();
        public DashboardContinueDto? ContinueLearning { get; set; }
        public List<DashboardContinueDto> Recommended { get; set; } = new();
        public List<DashboardChallengeDto> Challenges { get; set; } = new();
        public List<DashboardAchievementDto> Achievements { get; set; } = new();
        public DashboardVocabReminderDto? VocabReminder { get; set; }
    }

    public class DashboardStatsDto
    {
        public int XpTotal { get; set; }
        public int XpToday { get; set; }
        public int StreakDays { get; set; }
        public int DailyGoalXp { get; set; }
        public int DailyGoalProgressPercent { get; set; }
        public int QuizzesPassedTotal { get; set; }
        public int UnitsCompletedTotal { get; set; }
    }

    public class DashboardDailyPointDto
    {
        public string Date { get; set; } = string.Empty;
        public int Xp { get; set; }
        public bool HadActivity { get; set; }
    }

    public class DashboardContinueDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }

    public class DashboardChallengeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Current { get; set; }
        public int Target { get; set; }
        public bool Completed { get; set; }
    }

    public class DashboardAchievementDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool Unlocked { get; set; }
    }

    public class DashboardVocabReminderDto
    {
        public int ReviewCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
