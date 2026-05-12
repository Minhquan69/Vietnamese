namespace Backend.Services.AiTutor
{
    public sealed class TutorChatMessage
    {
        public string Role { get; init; } = "user";

        public string Content { get; init; } = string.Empty;
    }

    public interface IAiTutorCompletionClient
    {
        Task<string> CompleteAsync(
            IReadOnlyList<TutorChatMessage> messages,
            CancellationToken cancellationToken = default);
    }
}
