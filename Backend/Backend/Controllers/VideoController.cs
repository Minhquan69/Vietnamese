using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideoController : ControllerBase
    {
        private readonly VideoService _videoService;

        public VideoController(VideoService videoService)
        {
            _videoService = videoService;
        }
        /*
         * tìm kiếm video theo từ khóa
         * 07/03/2025
         * thuphuong21072004
         */
        [HttpGet("searchVideo")]
        public async Task<IActionResult> Search(string keyword, [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var result = await _videoService.Search(keyword,page,pageSize);

            return Ok(result);
        }
        /*
         * thêm video từ youtube vào hệ thống
         * 13/03/2025
         * thuphuong21072004
         */
        [Authorize]
        [HttpPost("insertVideo")]
        public async Task<IActionResult> ImportVideo([FromBody] VideoDTO request)
        {
            await _videoService.ImportVideo(request.YoutubeId);

            return Ok("Video imported successfully");
        }
        /*
         * trả danh sách video theo trạng thái
         * 14/03/2025
         * thuphuong21072004
         */
        [HttpGet("listVideo")]
        public async Task<IActionResult> GetVideos([FromQuery] int? status,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var videos = await _videoService.GetAllVideos(status,page,pageSize);

            return Ok(videos);
        }

        /*
         * cập nhật trạng thái video
         * 14/03/2025
         * thuphuong21072004
         */
        [HttpPut("updateVideo")]
        public async Task<IActionResult> UpdateVideo([FromQuery] int videoId, [FromQuery] int status)
        {
            try
            {
                await _videoService.updateVideo(videoId, status);
                return Ok("Video updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /*
         * tìm kiếm video qua youtubeId
         * 18/03/2026
         * thuphuong21072004
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVideo(string id)
        {
            var video = await _videoService.GetVideo(id);

            if (video == null)
                return NotFound(new { message = "Không tìm thấy video" });

            return Ok(video);
        }
    }
}