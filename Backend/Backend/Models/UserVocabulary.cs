using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("UserVocabularies")]
    public class UserVocabulary
    {
        public int UserVocabularyId { get; set; }

        public int UserId { get; set; }

        public int VocabularyId { get; set; }

        /// <summary>Saved / starred for study lists.</summary>
        public bool Saved { get; set; }

        /// <summary>SM-2 style ease factor (typically 1.3–3.0).</summary>
        public decimal EaseFactor { get; set; } = 2.5m;

        public int IntervalDays { get; set; }

        public int Repetitions { get; set; }

        public DateTime NextReviewUtc { get; set; }

        public DateTime? LastReviewedUtc { get; set; }

        /// <summary>0–100 mastery estimate.</summary>
        public decimal MasteryScore { get; set; }

        /// <summary>0=new, 1=learning, 2=familiar, 3=mastered (derived).</summary>
        public byte Familiarity { get; set; }

        public User? User { get; set; }

        public Vocabulary? Vocabulary { get; set; }
    }
}
