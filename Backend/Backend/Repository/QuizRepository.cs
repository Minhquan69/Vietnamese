using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public interface QuizRepository
    {
        Task<Quiz?> GetQuiz(int refId, string refType);

        Task AddQuiz(Quiz quiz);

        Task DeleteQuiz(Quiz quiz);
        Task<Quiz?> GetQuizById(int quizId);

        /// <summary>Quiz metadata only (no nested questions).</summary>
        Task<Quiz?> GetQuizSummary(int quizId);

        Task Save();
    }
}
