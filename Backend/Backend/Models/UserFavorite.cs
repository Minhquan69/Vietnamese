namespace Backend.Models
{
    public class UserFavorite
    {
        public int UserFavoriteId { get; set; }
        public int UserId { get; set; }
        public int LessonId { get; set; }
        public DateTime SavedDate { get; set; }

        public Lesson Lesson { get; set; }
    }
}
