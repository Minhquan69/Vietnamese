using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Vocabularies")]
    public class Vocabulary
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

        public bool IsActive { get; set; } = true;

        public DateTime CreatedUtc { get; set; }

        public ICollection<UserVocabulary>? UserVocabularies { get; set; }

        public ICollection<VideoVocabulary>? VideoVocabularies { get; set; }
    }
}
