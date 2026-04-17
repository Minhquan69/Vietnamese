using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Transcript
    {
        public int TranscriptId { get; set; }

        public int VideoId { get; set; }

        public string Sentence { get; set; }

        public double StartTime { get; set; }
        [ForeignKey("VideoId")]
        public virtual Video Video { get; set; }
    }
}
