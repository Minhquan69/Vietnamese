namespace Backend.Services.AiTutor
{
    /// <summary>Used when no API key is configured — deterministic demo for local development.</summary>
    public sealed class StubAiTutorCompletionClient : IAiTutorCompletionClient
    {
        public Task<string> CompleteAsync(
            IReadOnlyList<TutorChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            var last = messages.LastOrDefault(m => m.Role == "user")?.Content?.Trim() ?? "";
            var reply =
                "Xin chào! Hiện tại chưa cấu hình OpenAI API (AiTutor:ApiKey trong appsettings). " +
                "Đây là phản hồi mẫu để bạn thử giao diện.\n\n" +
                "Bạn có thể viết tiếng Việt để luyện hội thoại. " +
                (last.Length > 0
                    ? $"Bạn vừa viết: «{last}» — câu này nghe tự nhiên. Bạn có thể thử thêm: «Xin lỗi, tôi muốn…».\n\n"
                    : "\n") +
                "Khi đã cấu hình API, tôi sẽ sửa ngữ pháp, gợi ý câu hay hơn và đóng vai theo kịch bản bạn chọn.\n\n" +
                "<<<SUGGESTIONS>>>\n" +
                "Sửa giúp tôi câu này|Gợi ý cách nói lịch sự hơn|Luyện đoạn hội thoại ngắn";

            return Task.FromResult(reply);
        }
    }
}
