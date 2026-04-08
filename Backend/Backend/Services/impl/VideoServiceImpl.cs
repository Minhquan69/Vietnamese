using Backend.dto;
using Backend.Mapper;
using Backend.Models;
using Backend.Repository;
using AutoMapper;

namespace Backend.Services.impl
{
    public class VideoServiceImpl : VideoService
    {
        private readonly VideoRepository _videoRepository;
        private readonly IMapper _mapper;
        public VideoServiceImpl(VideoRepository videoRepository, IMapper mapper)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
        }
        /*
         * thêm video mới vào hệ thống
         * 13/03/2026
         * thuphuong21072004
         */
        public async Task ImportVideo(string youtubeId)
        {
            await _videoRepository.ImportVideo(youtubeId);
        }
        /*
         * lấy danh sách theo trạng thái
         * 14/03/2026
         * thuphuong21072004
         */
        public async Task<List<VideoDTO>> GetAllVideos(int? status, int page, int pageSize)
        {
            var videos = await _videoRepository.GetAllVideos(status,page,pageSize);

            return _mapper.Map<List<Video>, List<VideoDTO>>(videos);
        }
        /*
         * tìm kiếm video theo từ khóa
         * 07/03/2026
         * thuphuong21072004
         */
        public async Task<List<TranscriptDTO>> Search(string keyword, int page, int pageSize)
        {
            var transcripts = await _videoRepository.Search(keyword,page,pageSize);

            return _mapper.Map<List<Transcript>, List<TranscriptDTO>>(transcripts);
        }
        /*
         * cập nhật trạng thái video
         * 14/03/2026
         * thuphuong21072004
         */
        public async Task updateVideo(int videoId, int status)
        {
            await _videoRepository.UpdateVideo(videoId, status);
        }
        /*
         * tìm kiếm video theo youtubeId
         * 18/03/2026
         * thuphuong21072004
         */
        public async Task<VideoDTO> GetVideo(string videoId)
        {
            var video = await _videoRepository.SearchVideo(videoId);

            if (video == null) return null;

            return _mapper.Map<Video, VideoDTO>(video);
        }
    }
}
