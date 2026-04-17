using System.Net.Http.Json;
using Backend.Common;
using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;
namespace Backend.Repository.impl
{
    public class VideoRepositoryImpl : VideoRepository
    {
        private readonly HttpClient _http;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserContextUtil _userContext;
        public VideoRepositoryImpl(HttpClient http, AppDbContext context, IHttpContextAccessor httpContextAccessor, UserContextUtil userContext)
        {
            _http = http;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userContext = userContext;
        }
        /*
         * thêm video mới vào hệ thống
         * gửi yêu cầu crawl video từ Youtube vào hệ thống
         * 13/03/2026
         * thuphuong21072004
         */
        public async Task ImportVideo(string youtubeId)
        {
            var userId = _userContext.GetUserId();

            var data = new
            {
                youtubeId = youtubeId,
                createdBy = userId
            };

            await _http.PostAsJsonAsync(
                "http://localhost:5001/crawl",
                data
            );
        }
        /*
         * lấy danh sách video theo trạng thái (phân trang, sắp xếp mới nhất)
         * 14/03/2026
         * thuphuong21072004
         */
        public async Task<List<Video>> GetAllVideos(int? status, int page, int pageSize)
        {
            var query = _context.Videos.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            return await query
                .OrderByDescending(v => v.VideoId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        /*
         * tìm kiếm video theo từ khóa trong transcript (phân trang)
         * 07/03/2026
         * thuphuong21072004
         */
        public async Task<List<Transcript>> Search(string keyword,int page, int pageSize)
        {
            var query = _context.Transcripts
                    .Include(t => t.Video)
                    .Where(t => t.Sentence.Contains(keyword)
                                && t.Video != null
                                && t.Video.Status == 1);

            return await query
                .GroupBy(t => t.VideoId)
                .Select(g => g.OrderBy(t => t.StartTime).First())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)             
                .ToListAsync();
        }
        /*
         * cập nhật trạng thái video
         * 14/03/2026
         * thuphuong21072004
         */
        public async Task UpdateVideo(int videoId, int status)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video != null)
            {
                video.Status = status;
                await _context.SaveChangesAsync();
            }
        }
        /*
         * tìm kiếm video theo YoutubeId
         * 18/03/2026
         * thuphuong21072004
         */
        public async Task<Video?> SearchVideo(string youtubeId)
        {
            return await _context.Videos
                .FirstOrDefaultAsync(v => v.YoutubeId == youtubeId);
        }
    }
}
