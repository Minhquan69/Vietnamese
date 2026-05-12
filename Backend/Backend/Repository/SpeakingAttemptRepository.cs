using Backend.Models;

namespace Backend.Repository
{
    public interface SpeakingAttemptRepository
    {
        Task<SpeakingAttempt> AddAsync(SpeakingAttempt row);

        Task<List<SpeakingAttempt>> ListByUserAsync(int userId, int take);
    }
}
