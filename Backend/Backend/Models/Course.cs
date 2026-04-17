using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        public int LevelId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey("LevelId")] 
        public virtual Level Level { get; set; }
        public ICollection<Unit> Units { get; set; }
        public ICollection<UserCourse> UserCourses { get; set; }
    }
}
