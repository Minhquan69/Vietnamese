using Backend.Models;

namespace Backend.Repository
{
    public interface RefreshTokenRepository
    {
        Task AddAsync(RefreshToken token, CancellationToken ct = default);
        Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct = default);
        Task RevokeAsync(int refreshTokenId, string? replacedByHash, CancellationToken ct = default);
        Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
    }
}
