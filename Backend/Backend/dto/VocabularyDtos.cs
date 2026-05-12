namespace Backend.dto
{
    public class VocabularyCardDto
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string? Ipa { get; set; }
        public string? AudioUrl { get; set; }
        public string? MeaningEn { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? ExampleSentence { get; set; }
        public string? ExampleTranslation { get; set; }
        public string? ContextNote { get; set; }
    }

    public class UserVocabularyCardDto : VocabularyCardDto
    {
        public bool Saved { get; set; }
        public decimal MasteryScore { get; set; }
        public byte Familiarity { get; set; }
        public int IntervalDays { get; set; }
        public int Repetitions { get; set; }
        public DateTime NextReviewUtc { get; set; }
        public bool IsDue { get; set; }
    }

    public class VocabularyListResultDto
    {
        public List<VocabularyCardDto> Items { get; set; } = new();
        public int Total { get; set; }
    }

    public class ReviewGradeDto
    {
        /// <summary>again | hard | good | easy</summary>
        public string Grade { get; set; } = "good";
    }

    public class ReviewResultDto
    {
        public int VocabularyId { get; set; }
        public decimal MasteryScore { get; set; }
        public byte Familiarity { get; set; }
        public int IntervalDays { get; set; }
        public int Repetitions { get; set; }
        public DateTime NextReviewUtc { get; set; }
        public decimal EaseFactor { get; set; }
    }

    public class VocabularyStatsDto
    {
        public int SavedCount { get; set; }
        public int DueCount { get; set; }
        public decimal AverageMastery { get; set; }
        public int MasteredCount { get; set; }
    }

    public class SavedToggleResultDto
    {
        public int VocabularyId { get; set; }
        public bool Saved { get; set; }
    }

    public class SavedRequestDto
    {
        public bool Saved { get; set; }
    }
}
