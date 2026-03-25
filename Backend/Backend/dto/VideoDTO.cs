namespace Backend.dto
{
    public class VideoDTO
    {
        public int VideoId { get; set; }
        public string YoutubeId { get; set; } = "";
        public string Title { get; set; } = "";
        public int Status { get; set; }
    }
}
