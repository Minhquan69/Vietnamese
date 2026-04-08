namespace Backend.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }
        
        public Quiz Quiz { get; set; }
        public ICollection<Answer> Answers { get; set; }
    }
}
