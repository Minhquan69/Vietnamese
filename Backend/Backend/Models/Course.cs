namespace Backend.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public int LevelId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public int CreatedBy { get; set; }
        public bool IsActive { get; set; }

        public Level Level { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<UserCourse> UserCourses { get; set; }
    }
}
