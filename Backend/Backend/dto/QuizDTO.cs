namespace Backend.dto
{
    public class QuizDTO
    {
        public int QuizId { get; set; }
        public int UnitId { get; set; }
        public string QuizName { get; set; }
        public double PassScore { get; set; }
       
        public List<QuestionDTO> Questions { get; set; }
    }
}
