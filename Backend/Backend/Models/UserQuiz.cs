namespace Backend.Models
{
    public class UserQuiz
    {
        public int UserQuizId { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public double Score { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsPassed { get; set; }

        public Quiz Quiz { get; set; }
    }
}
