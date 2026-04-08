namespace Backend.Models
{
    public class Quiz
    {
        public int QuizId { get; set; }
        public int LessonId { get; set; }
        public string QuizName { get; set; }
        public double PassScore { get; set; }
       
        public Lesson Lesson { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<UserQuiz> UserQuizzes { get; set; }
    }
}
