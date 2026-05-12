using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("VideoVocabularies")]
    public class VideoVocabulary
    {
        public int VideoVocabularyId { get; set; }

        public int VideoId { get; set; }

        public int VocabularyId { get; set; }

        public int? TranscriptId { get; set; }

        public string? ContextSnippet { get; set; }

        public int SortOrder { get; set; }

        public Video? Video { get; set; }

        public Vocabulary? Vocabulary { get; set; }

        public Transcript? Transcript { get; set; }
    }
}
