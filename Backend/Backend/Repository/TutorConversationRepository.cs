using Backend.Models;

namespace Backend.Repository
{
    public interface TutorConversationRepository
    {
        Task<TutorConversation?> GetAsync(int userId, int conversationId);

        Task<List<TutorConversation>> ListAsync(int userId, int take);

        Task<TutorConversation> CreateAsync(TutorConversation row);

        Task TouchAsync(int conversationId, string? title);

        Task<List<TutorMessage>> GetMessagesAsync(int conversationId, int takeLast);

        Task<TutorMessage> AddMessageAsync(TutorMessage row);
    }
}
