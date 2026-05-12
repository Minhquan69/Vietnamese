using Backend.dto;

namespace Backend.Services
{
    public interface GamificationService
    {
        Task<GamificationStateDto> GetMyStateAsync();

        Task<GamificationDashboardSupplementDto> GetDashboardSupplementAsync(
            int userId,
            DateTime todayUtc,
            DateTime chartStartUtc);

        Task<IReadOnlyList<GamificationLeaderboardRowDto>> GetLeaderboardAsync(int take);

        Task RecordLessonCompletedAsync(int userId, int lessonId);

        Task RecordSpeakingEvaluatedAsync(int userId, int attemptId);

        Task RecordQuizPassedAsync(int userId, int quizId, decimal scorePercent);
    }
}
