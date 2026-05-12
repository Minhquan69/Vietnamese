using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("SpeakingAttempts")]
    public class SpeakingAttempt
    {
        public int SpeakingAttemptId { get; set; }

        public int UserId { get; set; }

        public string? ReferenceText { get; set; }

        public string Transcript { get; set; } = string.Empty;

        public string? AudioRelativePath { get; set; }

        public int DurationMs { get; set; }

        public decimal PronunciationScore { get; set; }

        public decimal FluencyScore { get; set; }

        public decimal ToneScore { get; set; }

        public decimal OverallScore { get; set; }

        public string? Feedback { get; set; }

        public string? AnalyticsJson { get; set; }

        public DateTime CreatedUtc { get; set; }

        public User? User { get; set; }
    }
}
