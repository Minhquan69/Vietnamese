namespace Backend.Options
{
    public class SpeakingOptions
    {
        public const string SectionName = "Speaking";

        public string TranscriptionsUrl { get; set; } = "https://api.openai.com/v1/audio/transcriptions";

        public string ChatCompletionsUrl { get; set; } = "https://api.openai.com/v1/chat/completions";

        /// <summary>If empty, falls back to AiTutor:ApiKey.</summary>
        public string ApiKey { get; set; } = string.Empty;

        public string WhisperModel { get; set; } = "whisper-1";

        public string EvaluatorModel { get; set; } = "gpt-4o-mini";

        public int MaxAudioBytes { get; set; } = 4_194_304;
    }
}
