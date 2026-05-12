namespace Backend.dto
{
    public class VideoLearningSessionDto
    {
        public VideoDTO Video { get; set; } = new();

        public List<TranscriptDTO> Transcripts { get; set; } = new();

        public List<VocabularyCardDto> LinkedVocabulary { get; set; } = new();
    }

    public class VideoExtractResultDto
    {
        public int TranscriptId { get; set; }

        public List<ExtractedTokenDto> Tokens { get; set; } = new();
    }

    public class ExtractedTokenDto
    {
        public string Text { get; set; } = "";

        public int StartIndex { get; set; }

        public int Length { get; set; }

        public int? VocabularyId { get; set; }
    }

    public class VideoVocabularyLinkDto
    {
        public string YoutubeId { get; set; } = "";

        public int VocabularyId { get; set; }

        public int? TranscriptId { get; set; }

        public string? ContextSnippet { get; set; }
    }
}
