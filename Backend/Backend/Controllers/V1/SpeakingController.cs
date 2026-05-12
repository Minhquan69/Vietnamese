using Backend.Common;
using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/speaking")]
    [Authorize]
    public class SpeakingController : ControllerBase
    {
        private readonly SpeakingEvaluationService _speaking;
        private readonly UserContextUtil _user;

        public SpeakingController(SpeakingEvaluationService speaking, UserContextUtil user)
        {
            _speaking = speaking;
            _user = user;
        }

        [HttpPost("evaluate")]
        [RequestSizeLimit(5_242_880)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<SpeakingEvaluateResponseDto>>> Evaluate(
            [FromForm] IFormFile? audio,
            [FromForm] string? referenceText,
            [FromForm] int? durationMs,
            CancellationToken cancellationToken)
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            if (audio == null || audio.Length == 0)
            {
                return BadRequest(ApiResponse<SpeakingEvaluateResponseDto>.Fail("Audio file is required."));
            }

            await using var stream = audio.OpenReadStream();
            try
            {
                var data = await _speaking.EvaluateAsync(
                    userId,
                    stream,
                    audio.FileName,
                    referenceText,
                    durationMs ?? 0,
                    cancellationToken);
                return Ok(ApiResponse<SpeakingEvaluateResponseDto>.Ok(data));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<SpeakingEvaluateResponseDto>.Fail(ex.Message));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ApiResponse<SpeakingEvaluateResponseDto>.Fail(ex.Message));
            }
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<ApiResponse<SpeakingAnalyticsDto>>> Analytics()
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var data = await _speaking.GetAnalyticsAsync(userId);
            return Ok(ApiResponse<SpeakingAnalyticsDto>.Ok(data));
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<SpeakingAttemptSummaryDto>>>> History(
            [FromQuery] int take = 50)
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var data = await _speaking.GetHistoryAsync(userId, take);
            return Ok(ApiResponse<List<SpeakingAttemptSummaryDto>>.Ok(data));
        }
    }
}
