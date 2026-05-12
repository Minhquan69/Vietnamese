namespace Backend.dto
{
    public class SpeakingEvaluateResponseDto
    {
        public int AttemptId { get; set; }

        public string Transcript { get; set; } = string.Empty;

        public string? AudioUrl { get; set; }

        public int DurationMs { get; set; }

        public decimal PronunciationScore { get; set; }

        public decimal FluencyScore { get; set; }

        public decimal ToneScore { get; set; }

        public decimal OverallScore { get; set; }

        public string? Feedback { get; set; }

        public List<string> Tips { get; set; } = new();
    }

    public class SpeakingAttemptSummaryDto
    {
        public int AttemptId { get; set; }

        public string? ReferenceText { get; set; }

        public string TranscriptPreview { get; set; } = string.Empty;

        public int DurationMs { get; set; }

        public decimal OverallScore { get; set; }

        public DateTime CreatedUtc { get; set; }
    }

    public class SpeakingAnalyticsDto
    {
        public int AttemptCount { get; set; }

        public decimal AverageOverall { get; set; }

        public decimal AveragePronunciation { get; set; }

        public decimal AverageFluency { get; set; }

        public decimal AverageTone { get; set; }

        public List<SpeakingAttemptSummaryDto> Recent { get; set; } = new();
    }
}
