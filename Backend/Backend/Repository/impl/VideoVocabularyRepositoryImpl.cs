using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class VideoVocabularyRepositoryImpl : VideoVocabularyRepository
    {
        private readonly AppDbContext _context;

        public VideoVocabularyRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VideoVocabulary>> ListByVideoWithVocabularyAsync(int videoId)
        {
            return await _context.VideoVocabularies
                .AsNoTracking()
                .Include(x => x.Vocabulary)
                .Where(x => x.VideoId == videoId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.VideoVocabularyId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int videoId, int vocabularyId)
        {
            return await _context.VideoVocabularies
                .AsNoTracking()
                .AnyAsync(x => x.VideoId == videoId && x.VocabularyId == vocabularyId);
        }

        public async Task AddAsync(VideoVocabulary link)
        {
            await _context.VideoVocabularies.AddAsync(link);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
