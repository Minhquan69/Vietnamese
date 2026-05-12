using Backend.dto;

namespace Backend.Services
{
    public interface SpeakingEvaluationService
    {
        Task<SpeakingEvaluateResponseDto> EvaluateAsync(
            int userId,
            Stream audioStream,
            string originalFileName,
            string? referenceText,
            int durationMs,
            CancellationToken cancellationToken = default);

        Task<SpeakingAnalyticsDto> GetAnalyticsAsync(int userId);

        Task<List<SpeakingAttemptSummaryDto>> GetHistoryAsync(int userId, int take = 50);
    }
}
