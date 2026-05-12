using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("TutorConversations")]
    public class TutorConversation
    {
        public int TutorConversationId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = "Chat";

        /// <summary>daily, restaurant, airport, hotel, shopping, or null.</summary>
        public string? ScenarioKey { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public User? User { get; set; }

        public ICollection<TutorMessage>? Messages { get; set; }
    }
}
