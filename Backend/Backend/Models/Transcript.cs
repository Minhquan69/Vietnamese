using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Backend.Models
{
    public class Transcript
    {
        public int TranscriptId { get; set; }

        public int VideoId { get; set; }

        public string Sentence { get; set; }

        public double StartTime { get; set; }

        public Video Video { get; set; }
    }
}
