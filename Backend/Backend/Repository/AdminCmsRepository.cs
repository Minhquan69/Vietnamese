using Backend.dto;

namespace Backend.Repository
{
    public interface AdminCmsRepository
    {
        Task<(List<AdminUserRowDto> Items, int Total)> ListUsersAsync(
            string? email,
            int? status,
            int? roleId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<AdminCourseRowDto> Items, int Total)> ListCoursesAsync(
            int? levelId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<AdminLessonRowDto> Items, int Total)> ListLessonsAsync(
            int? levelId,
            int? courseId,
            int? unitId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<AdminVocabularyRowDto> Items, int Total)> ListVocabulariesAsync(
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<AdminQuizRowDto> Items, int Total)> ListQuizzesAsync(
            string? q,
            string? refType,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<AdminAnalyticsSummaryDto> GetAnalyticsSummaryAsync(CancellationToken cancellationToken = default);
    }
}
