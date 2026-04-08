namespace Backend.dto
{
    public class LessonDTO
    {
        public int LessonId { get; set; }
        public int CourseId { get; set; }
        public string LessonName { get; set; }
        public string VideoUrl { get; set; }
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
        public bool? Status { get; set; }
        public bool IsDelete { get; set; }
    }
}
