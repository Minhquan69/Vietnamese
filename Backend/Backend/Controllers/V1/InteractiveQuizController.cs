using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/interactive-quiz")]
    public class InteractiveQuizController : ControllerBase
    {
        private readonly InteractiveQuizService _interactiveQuiz;

        public InteractiveQuizController(InteractiveQuizService interactiveQuiz)
        {
            _interactiveQuiz = interactiveQuiz;
        }

        [Authorize]
        [HttpGet("take")]
        public async Task<ActionResult<ApiResponse<PlayerQuizPackageDto>>> Take([FromQuery] int quizId)
        {
            var data = await _interactiveQuiz.GetQuizForPlayerAsync(quizId);
            if (data == null)
            {
                return NotFound(ApiResponse<PlayerQuizPackageDto>.Fail("Quiz not found or inactive."));
            }

            return Ok(ApiResponse<PlayerQuizPackageDto>.Ok(data));
        }

        [Authorize]
        [HttpPost("submit")]
        public async Task<ActionResult<ApiResponse<InteractiveQuizResultDto>>> Submit(
            [FromBody] InteractiveQuizSubmitDto body)
        {
            try
            {
                var data = await _interactiveQuiz.SubmitAsync(body);
                if (data == null)
                {
                    return NotFound(ApiResponse<InteractiveQuizResultDto>.Fail("Quiz not found or inactive."));
                }

                return Ok(ApiResponse<InteractiveQuizResultDto>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("attempts")]
        public async Task<ActionResult<ApiResponse<List<QuizAttemptSummaryDto>>>> Attempts(
            [FromQuery] int? quizId)
        {
            try
            {
                var data = await _interactiveQuiz.GetRecentAttemptsAsync(quizId);
                return Ok(ApiResponse<List<QuizAttemptSummaryDto>>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}
