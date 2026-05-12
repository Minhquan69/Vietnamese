using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class TutorConversationRepositoryImpl : TutorConversationRepository
    {
        private readonly AppDbContext _db;

        public TutorConversationRepositoryImpl(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TutorConversation?> GetAsync(int userId, int conversationId)
        {
            return await _db.TutorConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.TutorConversationId == conversationId && c.UserId == userId);
        }

        public async Task<List<TutorConversation>> ListAsync(int userId, int take)
        {
            return await _db.TutorConversations
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedUtc)
                .Take(take)
                .ToListAsync();
        }

        public async Task<TutorConversation> CreateAsync(TutorConversation row)
        {
            row.CreatedUtc = DateTime.UtcNow;
            row.UpdatedUtc = row.CreatedUtc;
            await _db.TutorConversations.AddAsync(row);
            await _db.SaveChangesAsync();
            return row;
        }

        public async Task TouchAsync(int conversationId, string? title)
        {
            var row = await _db.TutorConversations.FirstOrDefaultAsync(c => c.TutorConversationId == conversationId);
            if (row == null)
            {
                return;
            }

            row.UpdatedUtc = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(title))
            {
                row.Title = title.Trim().Length > 200 ? title.Trim().Substring(0, 200) : title.Trim();
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<TutorMessage>> GetMessagesAsync(int conversationId, int takeLast)
        {
            var q = _db.TutorMessages
                .AsNoTracking()
                .Where(m => m.TutorConversationId == conversationId)
                .OrderByDescending(m => m.TutorMessageId);

            var last = await q.Take(takeLast).ToListAsync();
            last.Reverse();
            return last;
        }

        public async Task<TutorMessage> AddMessageAsync(TutorMessage row)
        {
            row.CreatedUtc = DateTime.UtcNow;
            await _db.TutorMessages.AddAsync(row);
            await _db.SaveChangesAsync();
            return row;
        }
    }
}
