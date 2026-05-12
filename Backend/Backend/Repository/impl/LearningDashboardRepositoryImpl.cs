using Backend.Data;
using Backend.common;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class LearningDashboardRepositoryImpl : LearningDashboardRepository
    {
        private readonly AppDbContext _context;

        public LearningDashboardRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalXpAsync(int userId)
        {
            return await _context.UserQuiz
                .AsNoTracking()
                .Where(q => q.UserId == userId)
                .SumAsync(q => (decimal?)q.Score) ?? 0m;
        }

        public async Task<decimal> GetXpOnDateAsync(int userId, DateTime dayUtc)
        {
            var start = dayUtc.Date;
            var end = start.AddDays(1);
            return await _context.UserQuiz
                .AsNoTracking()
                .Where(q => q.UserId == userId && q.CompletedDate >= start && q.CompletedDate < end)
                .SumAsync(q => (decimal?)q.Score) ?? 0m;
        }

        public async Task<Dictionary<DateTime, decimal>> GetDailyXpAsync(
            int userId,
            DateTime startUtcInclusive,
            DateTime endUtcInclusive)
        {
            var start = startUtcInclusive.Date;
            var end = endUtcInclusive.Date.AddDays(1);

            var rows = await _context.UserQuiz
                .AsNoTracking()
                .Where(q => q.UserId == userId && q.CompletedDate >= start && q.CompletedDate < end)
                .GroupBy(q => q.CompletedDate.Date)
                .Select(g => new { Day = g.Key, Xp = g.Sum(x => x.Score) })
                .ToListAsync();

            return rows.ToDictionary(r => r.Day, r => r.Xp);
        }

        public async Task<HashSet<DateTime>> GetActivityDatesUtcAsync(int userId, DateTime sinceUtc)
        {
            var since = sinceUtc.Date;
            var dates = new HashSet<DateTime>();

            var quizDays = await _context.UserQuiz
                .AsNoTracking()
                .Where(q => q.UserId == userId && q.CompletedDate >= since)
                .Select(q => q.CompletedDate.Date)
                .Distinct()
                .ToListAsync();

            foreach (var d in quizDays)
            {
                dates.Add(d);
            }

            var progressDays = await _context.UserProgress
                .AsNoTracking()
                .Where(p =>
                    p.UserId == userId
                    && p.CompletedDate != null
                    && p.CompletedDate >= since)
                .Select(p => p.CompletedDate!.Value.Date)
                .Distinct()
                .ToListAsync();

            foreach (var d in progressDays)
            {
                dates.Add(d);
            }

            return dates;
        }

        public async Task<int> CountPassedQuizzesOnDateAsync(int userId, DateTime dayUtc)
        {
            var start = dayUtc.Date;
            var end = start.AddDays(1);
            return await _context.UserQuiz
                .AsNoTracking()
                .CountAsync(q =>
                    q.UserId == userId
                    && q.IsPassed
                    && q.CompletedDate >= start
                    && q.CompletedDate < end);
        }

        public async Task<int> CountUnitsCompletedOnDateAsync(int userId, DateTime dayUtc)
        {
            var start = dayUtc.Date;
            var end = start.AddDays(1);
            return await _context.UserProgress
                .AsNoTracking()
                .CountAsync(p =>
                    p.UserId == userId
                    && p.RefType == Constant.RefType.Unit
                    && p.Status
                    && p.CompletedDate != null
                    && p.CompletedDate >= start
                    && p.CompletedDate < end);
        }

        public async Task<int> CountWrongAnswersSinceAsync(int userId, DateTime sinceUtc)
        {
            return await _context.UserAnswer
                .AsNoTracking()
                .Join(
                    _context.UserQuiz,
                    ua => ua.UserQuizId,
                    uq => uq.UserQuizId,
                    (ua, uq) => new { ua, uq })
                .Join(
                    _context.Answers,
                    x => x.ua.AnswerId,
                    a => a.AnswerId,
                    (x, a) => new { x.ua, x.uq, a })
                .CountAsync(t =>
                    t.uq.UserId == userId
                    && t.uq.CompletedDate >= sinceUtc
                    && !t.a.IsCorrect);
        }

        public async Task<int> CountPassedQuizzesAsync(int userId)
        {
            return await _context.UserQuiz
                .AsNoTracking()
                .CountAsync(q => q.UserId == userId && q.IsPassed);
        }

        public async Task<int> CountUnitsCompletedAsync(int userId)
        {
            return await _context.UserProgress
                .AsNoTracking()
                .CountAsync(p =>
                    p.UserId == userId
                    && p.RefType == Constant.RefType.Unit
                    && p.Status
                    && p.CompletedDate != null);
        }
    }
}
