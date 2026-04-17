namespace Backend.dto
{
    public class QuestionDTO
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string? QuestionText { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public bool IsDelete { get; set; }
        public List<AnswerDTO> Answers { get; set; }
    }
}
