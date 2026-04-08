namespace Backend.Models
{
    public class UserCourse
    {
        public int UserCourseId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public bool Status { get; set; }
        
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public Course Course { get; set; }
    }
}
