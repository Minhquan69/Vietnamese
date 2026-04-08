namespace Backend.Models
{
    public class UserProgress
    {
        public int UserProgressId { get; set; }
        public int UserId { get; set; }
        public int LessonId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool Status { get; set; }
        public Lesson Lesson { get; set; }
    }
    
}
