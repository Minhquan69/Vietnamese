using Backend.dto;
using Backend.Models;

namespace Backend.Repository
{
    public interface UserAnswerRepository
    {
        Task SaveUserAnswer(UserAnswer userAnswer);

        Task AddUserAnswers(IEnumerable<UserAnswer> rows);

        Task<List<UserAnswerDTO>> GetUserAnswers(int userId, int quizId);
        Task Save();

        Task DeleteByUserQuizId(
            int userQuizId);
    }
}
