using Backend.Models;

namespace Backend.Repository
{
    public interface UserVocabularyRepository
    {
        Task<UserVocabulary?> GetAsync(int userId, int vocabularyId);

        Task<UserVocabulary> AddAsync(UserVocabulary entity);

        void Update(UserVocabulary entity);

        Task SaveAsync();

        Task<List<UserVocabulary>> GetDueAsync(int userId, DateTime asOfUtc, int limit);

        Task<List<UserVocabulary>> GetSavedWithVocabularyAsync(int userId);

        Task<int> CountSavedAsync(int userId);

        Task<int> CountDueAsync(int userId, DateTime asOfUtc);

        Task<int> CountMasteredAsync(int userId, decimal masteryThreshold);

        Task<decimal?> AverageMasteryAsync(int userId);

        Task<List<int>> GetKnownVocabularyIdsAsync(int userId);
    }
}
