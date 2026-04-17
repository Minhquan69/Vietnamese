using System.ComponentModel.DataAnnotations.Schema;

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
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

    }
}
