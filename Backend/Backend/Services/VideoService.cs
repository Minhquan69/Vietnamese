using Backend.dto;
public interface VideoService
{
    Task ImportVideo(string youtubeId);
    Task<List<VideoDTO>> GetAllVideos(int? status, int page, int pageSize);
    Task<List<TranscriptDTO>> Search(string keyword, int page, int pageSize);
    Task updateVideo(int videoId, int status);
    Task<VideoDTO> GetVideo(string videoId);
}