namespace Backend.Models
{
    public class Level
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Course> Courses { get; set; }
        public ICollection<UserLevel> UserLevels { get; set; }
    }
}
