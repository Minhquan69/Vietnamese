namespace Backend.Models
{
    public class Lesson
    {
        public int LessonId { get; set; }
        public int CourseId { get; set; }
        public string LessonName { get; set; }
        public string VideoUrl { get; set; }
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }

        public Course Course { get; set; }
        public Quiz Quiz { get; set; }

        public ICollection<UserProgress> UserProgresses { get; set; }
        public ICollection<UserFavorite> UserFavorites { get; set; }
    }
}
