using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("QuizAttempts")]
    public class QuizAttempt
    {
        public int QuizAttemptId { get; set; }

        public int UserId { get; set; }
        public int QuizId { get; set; }

        public decimal ScorePercent { get; set; }

        public int DurationSeconds { get; set; }

        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int SkippedCount { get; set; }

        public DateTime SubmittedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Per-question analytics JSON.</summary>
        public string? DetailJson { get; set; }

        public User? User { get; set; }
        public Quiz? Quiz { get; set; }
    }
}
