using Backend.dto;

namespace Backend.Services
{
    public interface AdminCmsService
    {
        Task<PagedResultDto<AdminUserRowDto>> ListUsersAsync(
            string? email,
            int? status,
            int? roleId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PagedResultDto<AdminCourseRowDto>> ListCoursesAsync(
            int? levelId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PagedResultDto<AdminLessonRowDto>> ListLessonsAsync(
            int? levelId,
            int? courseId,
            int? unitId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PagedResultDto<AdminVocabularyRowDto>> ListVocabulariesAsync(
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PagedResultDto<AdminQuizRowDto>> ListQuizzesAsync(
            string? q,
            string? refType,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<AdminAnalyticsSummaryDto> GetAnalyticsSummaryAsync(CancellationToken cancellationToken = default);
    }
}
