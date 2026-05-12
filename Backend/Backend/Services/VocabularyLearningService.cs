using Backend.dto;

namespace Backend.Services
{
    public interface VocabularyLearningService
    {
        Task<VocabularyListResultDto> SearchAsync(string? search, int page, int pageSize);

        Task<VocabularyCardDto?> GetVocabularyAsync(int vocabularyId);

        Task<UserVocabularyCardDto?> GetUserCardAsync(int vocabularyId);

        Task<List<UserVocabularyCardDto>> GetDeckAsync(int limit);

        Task<ReviewResultDto> SubmitReviewAsync(int vocabularyId, string grade);

        Task<SavedToggleResultDto> SetSavedAsync(int vocabularyId, bool saved);

        Task<VocabularyStatsDto> GetStatsAsync();

        Task<List<UserVocabularyCardDto>> GetSavedListAsync();
    }
}
