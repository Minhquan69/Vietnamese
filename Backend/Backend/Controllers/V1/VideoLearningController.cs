using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/video-learning")]
    public class VideoLearningController : ControllerBase
    {
        private readonly VideoLearningService _videoLearning;

        public VideoLearningController(VideoLearningService videoLearning)
        {
            _videoLearning = videoLearning;
        }

        [HttpGet("session")]
        public async Task<ActionResult<ApiResponse<VideoLearningSessionDto>>> Session(
            [FromQuery] string youtubeId)
        {
            var data = await _videoLearning.GetSessionAsync(youtubeId);
            if (data == null)
            {
                return NotFound(ApiResponse<VideoLearningSessionDto>.Fail("Video not found."));
            }

            return Ok(ApiResponse<VideoLearningSessionDto>.Ok(data));
        }

        [HttpGet("extract")]
        public async Task<ActionResult<ApiResponse<VideoExtractResultDto>>> Extract(
            [FromQuery] string youtubeId,
            [FromQuery] int transcriptId)
        {
            var data = await _videoLearning.ExtractAsync(youtubeId, transcriptId);
            if (data == null)
            {
                return NotFound(ApiResponse<VideoExtractResultDto>.Fail("Transcript not found."));
            }

            return Ok(ApiResponse<VideoExtractResultDto>.Ok(data));
        }

        [Authorize]
        [HttpPost("link")]
        public async Task<ActionResult<ApiResponse<bool>>> Link([FromBody] VideoVocabularyLinkDto body)
        {
            var ok = await _videoLearning.LinkVocabularyAsync(body);
            if (!ok)
            {
                return BadRequest(ApiResponse<bool>.Fail("Unable to link vocabulary to this video."));
            }

            return Ok(ApiResponse<bool>.Ok(true));
        }
    }
}
