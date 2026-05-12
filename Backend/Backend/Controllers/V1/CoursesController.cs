using Backend.Contracts;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly LearningService _learningService;

        public CoursesController(LearningService learningService)
        {
            _learningService = learningService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetCourses([FromQuery] int levelId)
        {
            var result = await _learningService.GetCourses(levelId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCourseById([FromRoute] int id)
        {
            var result = await _learningService.GetCourseById(id);
            if (result == null) return NotFound(ApiResponse<object>.Fail("Course not found"));
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id:int}/units")]
        public async Task<ActionResult<ApiResponse<object>>> GetUnits([FromRoute] int id)
        {
            var result = await _learningService.GetUnits(id);
            return Ok(ApiResponse<object>.Ok(result));
        }
    }
}

