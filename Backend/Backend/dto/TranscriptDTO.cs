namespace Backend.dto
{
    public class TranscriptDTO
    {
        public int TranscriptId { get; set; }

        public string YoutubeId { get; set; } = "";

        public string Sentence { get; set; } = "";

        public double StartTime { get; set; }

        /// <summary>Exclusive upper bound in seconds (derived if omitted in DB).</summary>
        public double EndTime { get; set; }
    }
}
