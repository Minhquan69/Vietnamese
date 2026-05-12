using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/admin/cms")]
    [Authorize(Policy = "AdminOrModerator")]
    public class AdminCmsController : ControllerBase
    {
        private readonly AdminCmsService _cms;

        public AdminCmsController(AdminCmsService cms)
        {
            _cms = cms;
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<ApiResponse<AdminAnalyticsSummaryDto>>> Analytics(
            CancellationToken cancellationToken)
        {
            var data = await _cms.GetAnalyticsSummaryAsync(cancellationToken);
            return Ok(ApiResponse<AdminAnalyticsSummaryDto>.Ok(data));
        }

        [HttpGet("users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AdminUserRowDto>>>> Users(
            [FromQuery] string? email,
            [FromQuery] int? status,
            [FromQuery] int? roleId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var data = await _cms.ListUsersAsync(email, status, roleId, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResultDto<AdminUserRowDto>>.Ok(data));
        }

        [HttpGet("courses")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AdminCourseRowDto>>>> Courses(
            [FromQuery] int? levelId,
            [FromQuery] string? q,
            [FromQuery] bool? activeOnly,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var data = await _cms.ListCoursesAsync(levelId, q, activeOnly, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResultDto<AdminCourseRowDto>>.Ok(data));
        }

        [HttpGet("lessons")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AdminLessonRowDto>>>> Lessons(
            [FromQuery] int? levelId,
            [FromQuery] int? courseId,
            [FromQuery] int? unitId,
            [FromQuery] string? q,
            [FromQuery] bool? activeOnly,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var data = await _cms.ListLessonsAsync(
                levelId,
                courseId,
                unitId,
                q,
                activeOnly,
                page,
                pageSize,
                cancellationToken);
            return Ok(ApiResponse<PagedResultDto<AdminLessonRowDto>>.Ok(data));
        }

        [HttpGet("vocabulary")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AdminVocabularyRowDto>>>> Vocabulary(
            [FromQuery] string? q,
            [FromQuery] bool? activeOnly,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var data = await _cms.ListVocabulariesAsync(q, activeOnly, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResultDto<AdminVocabularyRowDto>>.Ok(data));
        }

        [HttpGet("quizzes")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AdminQuizRowDto>>>> Quizzes(
            [FromQuery] string? q,
            [FromQuery] string? refType,
            [FromQuery] bool? activeOnly,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var data = await _cms.ListQuizzesAsync(q, refType, activeOnly, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResultDto<AdminQuizRowDto>>.Ok(data));
        }
    }
}
