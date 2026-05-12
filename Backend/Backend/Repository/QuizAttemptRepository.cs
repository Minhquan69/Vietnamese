using Backend.Models;

namespace Backend.Repository
{
    public interface QuizAttemptRepository
    {
        Task AddAttempt(QuizAttempt attempt);

        Task<List<QuizAttempt>> ListRecentByUser(int userId, int? quizId, int take = 20);
    }
}
