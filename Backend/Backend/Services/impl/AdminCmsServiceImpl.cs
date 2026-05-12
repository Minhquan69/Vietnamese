using Backend.dto;
using Backend.Repository;

namespace Backend.Services.impl
{
    public class AdminCmsServiceImpl : AdminCmsService
    {
        private readonly AdminCmsRepository _repo;

        public AdminCmsServiceImpl(AdminCmsRepository repo)
        {
            _repo = repo;
        }

        private static int TotalPages(int total, int pageSize) =>
            pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        public async Task<PagedResultDto<AdminUserRowDto>> ListUsersAsync(
            string? email,
            int? status,
            int? roleId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (items, total) = await _repo.ListUsersAsync(email, status, roleId, page, pageSize, cancellationToken);
            var sz = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            var p = page < 1 ? 1 : page;
            return new PagedResultDto<AdminUserRowDto>
            {
                Items = items,
                Page = p,
                PageSize = sz,
                TotalCount = total,
                TotalPages = TotalPages(total, sz),
            };
        }

        public async Task<PagedResultDto<AdminCourseRowDto>> ListCoursesAsync(
            int? levelId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (items, total) = await _repo.ListCoursesAsync(levelId, q, activeOnly, page, pageSize, cancellationToken);
            var sz = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            var p = page < 1 ? 1 : page;
            return new PagedResultDto<AdminCourseRowDto>
            {
                Items = items,
                Page = p,
                PageSize = sz,
                TotalCount = total,
                TotalPages = TotalPages(total, sz),
            };
        }

        public async Task<PagedResultDto<AdminLessonRowDto>> ListLessonsAsync(
            int? levelId,
            int? courseId,
            int? unitId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (items, total) = await _repo.ListLessonsAsync(
                levelId,
                courseId,
                unitId,
                q,
                activeOnly,
                page,
                pageSize,
                cancellationToken);
            var sz = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            var p = page < 1 ? 1 : page;
            return new PagedResultDto<AdminLessonRowDto>
            {
                Items = items,
                Page = p,
                PageSize = sz,
                TotalCount = total,
                TotalPages = TotalPages(total, sz),
            };
        }

        public async Task<PagedResultDto<AdminVocabularyRowDto>> ListVocabulariesAsync(
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (items, total) = await _repo.ListVocabulariesAsync(q, activeOnly, page, pageSize, cancellationToken);
            var sz = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            var p = page < 1 ? 1 : page;
            return new PagedResultDto<AdminVocabularyRowDto>
            {
                Items = items,
                Page = p,
                PageSize = sz,
                TotalCount = total,
                TotalPages = TotalPages(total, sz),
            };
        }

        public async Task<PagedResultDto<AdminQuizRowDto>> ListQuizzesAsync(
            string? q,
            string? refType,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (items, total) = await _repo.ListQuizzesAsync(q, refType, activeOnly, page, pageSize, cancellationToken);
            var sz = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            var p = page < 1 ? 1 : page;
            return new PagedResultDto<AdminQuizRowDto>
            {
                Items = items,
                Page = p,
                PageSize = sz,
                TotalCount = total,
                TotalPages = TotalPages(total, sz),
            };
        }

        public Task<AdminAnalyticsSummaryDto> GetAnalyticsSummaryAsync(CancellationToken cancellationToken = default) =>
            _repo.GetAnalyticsSummaryAsync(cancellationToken);
    }
}
