using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/learning-path")]
    public class LearningPathController : ControllerBase
    {
        private readonly LessonLearningService _lessonLearningService;

        public LearningPathController(LessonLearningService lessonLearningService)
        {
            _lessonLearningService = lessonLearningService;
        }

        /// <summary>Courses in a level with aggregated lesson progress.</summary>
        [AllowAnonymous]
        [HttpGet("catalog")]
        public async Task<ActionResult<ApiResponse<List<CourseCatalogItemDto>>>> GetCatalog(
            [FromQuery] int levelId)
        {
            var data = await _lessonLearningService.GetCourseCatalogAsync(levelId);
            return Ok(ApiResponse<List<CourseCatalogItemDto>>.Ok(data));
        }

        [Authorize]
        [HttpGet("courses/{courseId:int}/detail")]
        public async Task<ActionResult<ApiResponse<CourseLearnDetailDto>>> GetCourseDetail(
            [FromRoute] int courseId)
        {
            var data = await _lessonLearningService.GetCourseLearnDetailAsync(courseId);
            if (data == null)
            {
                return NotFound(ApiResponse<CourseLearnDetailDto>.Fail("Course not found"));
            }

            return Ok(ApiResponse<CourseLearnDetailDto>.Ok(data));
        }

        [Authorize]
        [HttpGet("units/{unitId:int}/outline")]
        public async Task<ActionResult<ApiResponse<UnitOutlineDto>>> GetUnitOutline([FromRoute] int unitId)
        {
            var data = await _lessonLearningService.GetUnitOutlineAsync(unitId);
            if (data == null)
            {
                return NotFound(ApiResponse<UnitOutlineDto>.Fail("Unit not found"));
            }

            return Ok(ApiResponse<UnitOutlineDto>.Ok(data));
        }

        [Authorize]
        [HttpGet("lessons/{lessonId:int}")]
        public async Task<ActionResult<ApiResponse<LessonPlayerDto>>> GetLesson([FromRoute] int lessonId)
        {
            var data = await _lessonLearningService.GetLessonPlayerAsync(lessonId);
            if (data == null)
            {
                return NotFound(ApiResponse<LessonPlayerDto>.Fail("Lesson not found"));
            }

            return Ok(ApiResponse<LessonPlayerDto>.Ok(data));
        }

        [Authorize]
        [HttpPost("lessons/{lessonId:int}/complete")]
        public async Task<ActionResult<ApiResponse<LessonCompleteResultDto>>> CompleteLesson(
            [FromRoute] int lessonId)
        {
            var data = await _lessonLearningService.CompleteLessonAsync(lessonId);
            if (data == null)
            {
                return NotFound(ApiResponse<LessonCompleteResultDto>.Fail("Lesson not found"));
            }

            return Ok(ApiResponse<LessonCompleteResultDto>.Ok(data));
        }
    }
}
