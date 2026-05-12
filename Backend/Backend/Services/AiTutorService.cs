using Backend.dto;

namespace Backend.Services
{
    public interface AiTutorService
    {
        Task<AiTutorChatResponseDto> ChatAsync(int userId, AiTutorChatRequestDto request);

        Task<List<TutorConversationSummaryDto>> ListConversationsAsync(int userId);

        Task<List<TutorMessageDto>> GetMessagesAsync(int userId, int conversationId);
    }
}
