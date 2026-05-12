using Backend.dto;

namespace Backend.Services
{
    public interface VideoLearningService
    {
        Task<VideoLearningSessionDto?> GetSessionAsync(string youtubeId);

        Task<VideoExtractResultDto?> ExtractAsync(string youtubeId, int transcriptId);

        Task<bool> LinkVocabularyAsync(VideoVocabularyLinkDto dto);
    }
}
