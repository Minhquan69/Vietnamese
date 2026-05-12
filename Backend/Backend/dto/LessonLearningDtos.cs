namespace Backend.dto
{
    public class CourseCatalogItemDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int UnitCount { get; set; }
        public int LessonTotal { get; set; }
        public int LessonsCompleted { get; set; }
        public int ProgressPercent { get; set; }
    }

    public class CourseLearnDetailDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int LessonTotal { get; set; }
        public int LessonsCompleted { get; set; }
        public int ProgressPercent { get; set; }
        public List<UnitLearnSummaryDto> Units { get; set; } = new();
        public int? ContinueUnitId { get; set; }
        public int? ContinueLessonId { get; set; }
    }

    public class UnitLearnSummaryDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string? Objective { get; set; }
        public int OrderIndex { get; set; }
        public int LessonTotal { get; set; }
        public int LessonsCompleted { get; set; }
        public int ProgressPercent { get; set; }
        public bool UnitUnlocked { get; set; }
        public bool? UnitPathComplete { get; set; }
    }

    public class UnitOutlineDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int LessonTotal { get; set; }
        public int LessonsCompleted { get; set; }
        public int ProgressPercent { get; set; }
        public int? ContinueLessonId { get; set; }
        public List<LessonOutlineDto> Lessons { get; set; } = new();
        public int? QuizUnitId { get; set; }
    }

    public class LessonOutlineDto
    {
        public int LessonId { get; set; }
        public string LessonType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public int OrderIndex { get; set; }
        public int DurationMinutes { get; set; }
        public bool Completed { get; set; }
    }

    public class LessonPlayerDto
    {
        public int LessonId { get; set; }
        public string LessonType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public int OrderIndex { get; set; }
        public int DurationMinutes { get; set; }
        public string? ContentJson { get; set; }
        public bool Completed { get; set; }

        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }
        public int? PreviousLessonId { get; set; }
        public int? NextLessonId { get; set; }
        public int? QuizUnitId { get; set; }
    }

    public class LessonCompleteResultDto
    {
        public int LessonId { get; set; }
        public bool Completed { get; set; }
        public int UnitProgressPercent { get; set; }
        public int? NextLessonId { get; set; }
    }
}
