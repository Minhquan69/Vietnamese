using Backend.Models;

namespace Backend.Repository
{
    public interface VocabularyRepository
    {
        Task<(List<Vocabulary> Items, int Total)> SearchAsync(string? search, int skip, int take);

        Task<Vocabulary?> GetByIdAsync(int vocabularyId);

        Task<List<Vocabulary>> GetRandomActiveAsync(int take, HashSet<int> excludeIds);

        Task<List<Vocabulary>> GetByIdsAsync(IEnumerable<int> vocabularyIds);

        Task<List<Vocabulary>> FindByWordsAsync(IEnumerable<string> words);
    }
}
