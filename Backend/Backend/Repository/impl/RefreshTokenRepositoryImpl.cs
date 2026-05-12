using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class RefreshTokenRepositoryImpl : RefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _context.RefreshTokens
                .Include(r => r.User)!.ThenInclude(u => u!.Role)
                .Where(r => r.TokenHash == tokenHash && r.RevokedUtc == null && r.ExpiresUtc > now)
                .FirstOrDefaultAsync(ct);
        }

        public async Task RevokeAsync(int refreshTokenId, string? replacedByHash, CancellationToken ct = default)
        {
            var entity = await _context.RefreshTokens.FindAsync(new object[] { refreshTokenId }, ct);
            if (entity == null)
            {
                return;
            }

            entity.RevokedUtc = DateTime.UtcNow;
            entity.ReplacedByTokenHash = replacedByHash;
            await _context.SaveChangesAsync(ct);
        }

        public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var list = await _context.RefreshTokens
                .Where(r => r.UserId == userId && r.RevokedUtc == null)
                .ToListAsync(ct);

            foreach (var r in list)
            {
                r.RevokedUtc = now;
            }

            if (list.Count > 0)
            {
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
