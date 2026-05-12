using Backend.Common;
using Backend.Contracts;
using Backend.Data;
using Backend.dto;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/videos")]
    public class VideosController : ControllerBase
    {
        private readonly VideoService _videoService;
        private readonly AppDbContext _db;

        public VideosController(VideoService videoService, AppDbContext db)
        {
            _videoService = videoService;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetVideos([FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _videoService.GetAllVideos(status, page, pageSize);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetVideo([FromRoute] string id)
        {
            var result = await _videoService.GetVideo(id);
            if (result == null) return NotFound(ApiResponse<object>.Fail("Video not found"));
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id}/transcript")]
        public async Task<ActionResult<ApiResponse<List<TranscriptDTO>>>> GetTranscript([FromRoute] string id)
        {
            // `id` is the YoutubeId in the current system (matches existing GET /api/videos/{id})
            var transcripts = await _db.Transcripts
                .Include(t => t.Video)
                .Where(t => t.Video != null && t.Video.YoutubeId == id)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            var dto = TranscriptCueMapper.MapCues(id, transcripts);
            return Ok(ApiResponse<List<TranscriptDTO>>.Ok(dto));
        }
    }
}

