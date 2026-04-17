using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string? QuestionText { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }
        public ICollection<Answer> Answers { get; set; }
    }
}
