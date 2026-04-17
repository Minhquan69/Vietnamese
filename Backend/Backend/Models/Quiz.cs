using System.ComponentModel.DataAnnotations.Schema;
namespace Backend.Models
{
    public class Quiz
    {
        public int QuizId { get; set; }
        public int UnitId { get; set; }
        public string QuizName { get; set; }
        public double PassScore { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<UserQuiz> UserQuizzes { get; set; }
    }
}
