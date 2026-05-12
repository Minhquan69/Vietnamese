using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Transcript
    {
        public int TranscriptId { get; set; }
        public int VideoId { get; set; }

        public string Sentence { get; set; } = string.Empty;
        public double StartTime { get; set; }

        /// <summary>Seconds; if null, derived from next cue or default gap.</summary>
        public double? EndTime { get; set; }

        public Video? Video { get; set; }
    }
}
