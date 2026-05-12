using Backend.Models;

namespace Backend.Repository
{
    public interface VideoVocabularyRepository
    {
        Task<List<VideoVocabulary>> ListByVideoWithVocabularyAsync(int videoId);

        Task<bool> ExistsAsync(int videoId, int vocabularyId);

        Task AddAsync(VideoVocabulary link);

        Task SaveChangesAsync();
    }
}
