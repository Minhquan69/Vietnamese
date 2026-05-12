using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class QuizAttemptRepositoryImpl : QuizAttemptRepository
    {
        private readonly AppDbContext _context;

        public QuizAttemptRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAttempt(QuizAttempt attempt)
        {
            await _context.QuizAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();
        }

        public async Task<List<QuizAttempt>> ListRecentByUser(int userId, int? quizId, int take = 20)
        {
            var q = _context.QuizAttempts.AsNoTracking().Where(x => x.UserId == userId);
            if (quizId != null)
            {
                q = q.Where(x => x.QuizId == quizId);
            }

            return await q
                .OrderByDescending(x => x.SubmittedUtc)
                .Take(take)
                .ToListAsync();
        }
    }
}
