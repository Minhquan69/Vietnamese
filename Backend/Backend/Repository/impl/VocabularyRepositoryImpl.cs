using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class VocabularyRepositoryImpl : VocabularyRepository
    {
        private readonly AppDbContext _context;

        public VocabularyRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Vocabulary> Items, int Total)> SearchAsync(
            string? search,
            int skip,
            int take)
        {
            var query = _context.Vocabularies.AsNoTracking().Where(v => v.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(v => v.Word.Contains(s) || (v.MeaningEn != null && v.MeaningEn.Contains(s)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(v => v.Word)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Vocabulary?> GetByIdAsync(int vocabularyId)
        {
            return await _context.Vocabularies
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VocabularyId == vocabularyId && v.IsActive);
        }

        public async Task<List<Vocabulary>> GetRandomActiveAsync(int take, HashSet<int> excludeIds)
        {
            /* Stable ordering; caller mixes deck. For large catalogs, replace with NEWID() SQL. */
            return await _context.Vocabularies
                .AsNoTracking()
                .Where(v => v.IsActive && !excludeIds.Contains(v.VocabularyId))
                .OrderBy(v => v.VocabularyId)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<Vocabulary>> GetByIdsAsync(IEnumerable<int> vocabularyIds)
        {
            var ids = vocabularyIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new List<Vocabulary>();
            }

            return await _context.Vocabularies
                .AsNoTracking()
                .Where(v => v.IsActive && ids.Contains(v.VocabularyId))
                .ToListAsync();
        }

        public async Task<List<Vocabulary>> FindByWordsAsync(IEnumerable<string> words)
        {
            var set = words
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (set.Count == 0)
            {
                return new List<Vocabulary>();
            }

            return await _context.Vocabularies
                .AsNoTracking()
                .Where(v => v.IsActive && set.Contains(v.Word))
                .ToListAsync();
        }
    }
}
