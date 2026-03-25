using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Video
    {
        public int VideoId { get; set; }

        public string YoutubeId { get; set; }

        public string Title { get; set; }

        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User User { get; set; }
        public int Status { get; set; }

        public ICollection<Transcript> Transcripts { get; set; }
    }
}
