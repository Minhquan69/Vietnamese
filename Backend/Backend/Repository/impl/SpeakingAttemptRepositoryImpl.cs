using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class SpeakingAttemptRepositoryImpl : SpeakingAttemptRepository
    {
        private readonly AppDbContext _db;

        public SpeakingAttemptRepositoryImpl(AppDbContext db)
        {
            _db = db;
        }

        public async Task<SpeakingAttempt> AddAsync(SpeakingAttempt row)
        {
            row.CreatedUtc = DateTime.UtcNow;
            await _db.SpeakingAttempts.AddAsync(row);
            await _db.SaveChangesAsync();
            return row;
        }

        public async Task<List<SpeakingAttempt>> ListByUserAsync(int userId, int take)
        {
            return await _db.SpeakingAttempts
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedUtc)
                .Take(take)
                .ToListAsync();
        }
    }
}
