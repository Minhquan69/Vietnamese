using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int QuizId { get; set; }

        public int? PartId { get; set; }

        public int? PassageId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        /// <summary>MultipleChoice, FillBlank, DragDrop, ReorderSentence, Listening</summary>
        public string QuestionType { get; set; } = "MultipleChoice";

        public string? Explanation { get; set; }

        /// <summary>JSON: grading metadata (blanks, correct order, drag targets). Redacted for players.</summary>
        public string? InteractivePayload { get; set; }

        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }

        public int OrderIndex { get; set; }

        public decimal Score { get; set; }

        public Quiz? Quiz { get; set; }

        public Part? Part { get; set; }

        public Passage? Passage { get; set; }

        public ICollection<Answer>? Answers { get; set; }
    }
}
