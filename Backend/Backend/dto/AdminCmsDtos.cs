namespace Backend.dto
{
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }

    public class AdminUserRowDto
    {
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public int Status { get; set; }

        public string? AvatarUrl { get; set; }
    }

    public class AdminCourseRowDto
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public int LevelId { get; set; }

        public string LevelName { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public bool IsActive { get; set; }
    }

    public class AdminLessonRowDto
    {
        public int LessonId { get; set; }

        public int UnitId { get; set; }

        public string UnitName { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string LevelName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string LessonType { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public bool IsActive { get; set; }
    }

    public class AdminVocabularyRowDto
    {
        public int VocabularyId { get; set; }

        public string Word { get; set; } = string.Empty;

        public string? MeaningEn { get; set; }

        public string? PartOfSpeech { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedUtc { get; set; }
    }

    public class AdminQuizRowDto
    {
        public int QuizId { get; set; }

        public string QuizName { get; set; } = string.Empty;

        public string RefType { get; set; } = string.Empty;

        public int RefId { get; set; }

        public decimal? PassScore { get; set; }

        public int? TimeLimit { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class AdminAnalyticsTotalsDto
    {
        public int UsersTotal { get; set; }

        public int UsersActive { get; set; }

        public int CoursesTotal { get; set; }

        public int LessonsTotal { get; set; }

        public int VocabulariesTotal { get; set; }

        public int QuizzesTotal { get; set; }

        public int QuizAttemptsLast30Days { get; set; }

        public int SpeakingAttemptsLast30Days { get; set; }
    }

    public class AdminAnalyticsSeriesPointDto
    {
        public string Date { get; set; } = string.Empty;

        public int QuizCompletions { get; set; }

        public int SpeakingSessions { get; set; }
    }

    public class AdminAnalyticsSummaryDto
    {
        public AdminAnalyticsTotalsDto Totals { get; set; } = new();

        public List<AdminAnalyticsSeriesPointDto> ActivityLast14Days { get; set; } = new();
    }
}
