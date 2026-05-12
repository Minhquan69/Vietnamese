using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("TutorMessages")]
    public class TutorMessage
    {
        public int TutorMessageId { get; set; }

        public int TutorConversationId { get; set; }

        /// <summary>user or assistant (system is injected per request, not stored).</summary>
        public string Role { get; set; } = "user";

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }

        public TutorConversation? Conversation { get; set; }
    }
}
