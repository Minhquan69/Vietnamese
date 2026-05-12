namespace Backend.dto
{
    public class AiTutorChatRequestDto
    {
        public int? ConversationId { get; set; }

        /// <summary>User message in Vietnamese or English.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Optional: daily, restaurant, airport, hotel, shopping.</summary>
        public string? ScenarioKey { get; set; }
    }

    public class AiTutorChatResponseDto
    {
        public int ConversationId { get; set; }

        public string AssistantMessage { get; set; } = string.Empty;

        public List<string> Suggestions { get; set; } = new();
    }

    public class TutorConversationSummaryDto
    {
        public int ConversationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? ScenarioKey { get; set; }

        public DateTime UpdatedUtc { get; set; }
    }

    public class TutorMessageDto
    {
        public int MessageId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }
    }
}
