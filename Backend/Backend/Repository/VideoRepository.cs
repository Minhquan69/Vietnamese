using Backend.Models;

namespace Backend.Repository
{
    public interface VideoRepository
    {
        Task ImportVideo(string youtubeId);
        Task<List<Video>> GetAllVideos(int? status, int page, int pageSize);
        Task<List<Transcript>> Search(string keyword, int page, int pageSize);
        Task UpdateVideo(int videoId, int status);
        Task<Video?> SearchVideo(string youtubeId);
    }
}
