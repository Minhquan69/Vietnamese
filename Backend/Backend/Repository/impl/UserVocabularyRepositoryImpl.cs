using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class UserVocabularyRepositoryImpl : UserVocabularyRepository
    {
        private readonly AppDbContext _context;

        public UserVocabularyRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserVocabulary?> GetAsync(int userId, int vocabularyId)
        {
            return await _context.UserVocabularies
                .FirstOrDefaultAsync(x => x.UserId == userId && x.VocabularyId == vocabularyId);
        }

        public async Task<UserVocabulary> AddAsync(UserVocabulary entity)
        {
            await _context.UserVocabularies.AddAsync(entity);
            return entity;
        }

        public void Update(UserVocabulary entity)
        {
            _context.UserVocabularies.Update(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserVocabulary>> GetDueAsync(int userId, DateTime asOfUtc, int limit)
        {
            return await _context.UserVocabularies
                .AsNoTracking()
                .Include(x => x.Vocabulary)
                .Where(x => x.UserId == userId && x.NextReviewUtc <= asOfUtc)
                .OrderBy(x => x.NextReviewUtc)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<UserVocabulary>> GetSavedWithVocabularyAsync(int userId)
        {
            return await _context.UserVocabularies
                .AsNoTracking()
                .Include(x => x.Vocabulary)
                .Where(x => x.UserId == userId && x.Saved)
                .OrderByDescending(x => x.LastReviewedUtc ?? DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<int> CountSavedAsync(int userId)
        {
            return await _context.UserVocabularies.CountAsync(x => x.UserId == userId && x.Saved);
        }

        public async Task<int> CountDueAsync(int userId, DateTime asOfUtc)
        {
            return await _context.UserVocabularies.CountAsync(
                x => x.UserId == userId && x.NextReviewUtc <= asOfUtc);
        }

        public async Task<int> CountMasteredAsync(int userId, decimal masteryThreshold)
        {
            return await _context.UserVocabularies.CountAsync(
                x => x.UserId == userId && x.MasteryScore >= masteryThreshold);
        }

        public async Task<decimal?> AverageMasteryAsync(int userId)
        {
            return await _context.UserVocabularies
                .Where(x => x.UserId == userId)
                .AverageAsync(x => (decimal?)x.MasteryScore);
        }

        public async Task<List<int>> GetKnownVocabularyIdsAsync(int userId)
        {
            return await _context.UserVocabularies
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.VocabularyId)
                .ToListAsync();
        }
    }
}
