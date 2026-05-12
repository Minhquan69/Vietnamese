namespace Backend.dto
{
    public class InteractiveQuizSubmitDto
    {
        public int QuizId { get; set; }

        public int DurationSeconds { get; set; }

        public List<QuizResponseItemDto> Responses { get; set; } = new();
    }

    public class QuizResponseItemDto
    {
        public int QuestionId { get; set; }

        public int? AnswerId { get; set; }

        public Dictionary<string, string>? FillBlank { get; set; }

        public List<int>? OrderedAnswerIds { get; set; }

        /// <summary>Slot key → AnswerId chosen by learner.</summary>
        public Dictionary<string, int>? DragDrop { get; set; }
    }

    public class PlayerQuizPackageDto
    {
        public int QuizId { get; set; }

        public string QuizName { get; set; } = string.Empty;

        public int? TimeLimitMinutes { get; set; }

        public decimal? PassScore { get; set; }

        public List<PlayerQuestionDto> Questions { get; set; } = new();
    }

    public class PlayerQuestionDto
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public string QuestionType { get; set; } = "MultipleChoice";

        public string? ImageUrl { get; set; }

        public string? AudioUrl { get; set; }

        public decimal Score { get; set; }

        public int OrderIndex { get; set; }

        /// <summary>Sanitized payload (no answers).</summary>
        public string? InteractivePayload { get; set; }

        public List<PlayerAnswerDto> Answers { get; set; } = new();
    }

    public class PlayerAnswerDto
    {
        public int AnswerId { get; set; }

        public string AnswerText { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? AudioUrl { get; set; }

        public int OrderIndex { get; set; }
    }

    public class InteractiveQuizResultDto
    {
        public decimal ScorePercent { get; set; }

        public bool Passed { get; set; }

        public int CorrectCount { get; set; }

        public int WrongCount { get; set; }

        public int SkippedCount { get; set; }

        public List<InteractiveQuestionOutcomeDto> Items { get; set; } = new();
    }

    public class InteractiveQuestionOutcomeDto
    {
        public int QuestionId { get; set; }

        public string QuestionType { get; set; } = "";

        public bool Correct { get; set; }

        public bool Skipped { get; set; }

        public decimal PointsEarned { get; set; }

        public string? Explanation { get; set; }

        /// <summary>Optional JSON string describing the keyed solution for review UI.</summary>
        public string? ReviewHintJson { get; set; }
    }

    public class QuizAttemptSummaryDto
    {
        public int QuizAttemptId { get; set; }

        public int QuizId { get; set; }

        public decimal ScorePercent { get; set; }

        public int DurationSeconds { get; set; }

        public int CorrectCount { get; set; }

        public int WrongCount { get; set; }

        public int SkippedCount { get; set; }

        public DateTime SubmittedUtc { get; set; }
    }
}
