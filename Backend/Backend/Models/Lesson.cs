using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Lessons")]
    public class Lesson
    {
        public int LessonId { get; set; }

        public int UnitId { get; set; }

        /// <summary>Vocabulary, Grammar, Listening, Speaking, Reading, Video</summary>
        public string LessonType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public int OrderIndex { get; set; }

        public int DurationMinutes { get; set; } = 5;

        /// <summary>Optional rich content (bullets, instructions, asset hints).</summary>
        public string? ContentJson { get; set; }

        public bool IsActive { get; set; } = true;

        public Unit? Unit { get; set; }
    }
}
