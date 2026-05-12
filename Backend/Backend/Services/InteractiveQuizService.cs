using Backend.dto;

namespace Backend.Services
{
    public interface InteractiveQuizService
    {
        Task<PlayerQuizPackageDto?> GetQuizForPlayerAsync(int quizId);

        Task<InteractiveQuizResultDto?> SubmitAsync(InteractiveQuizSubmitDto dto);

        Task<List<QuizAttemptSummaryDto>> GetRecentAttemptsAsync(int? quizId);
    }
}
