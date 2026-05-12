namespace Backend.Options
{
    public class AiTutorOptions
    {
        public const string SectionName = "AiTutor";

        /// <summary>OpenAI-compatible chat completions URL (default OpenAI v1).</summary>
        public string ChatCompletionsUrl { get; set; } = "https://api.openai.com/v1/chat/completions";

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-4o-mini";

        public double Temperature { get; set; } = 0.65;

        public int MaxTokens { get; set; } = 1200;

        public int HistoryMessageLimit { get; set; } = 40;
    }
}
