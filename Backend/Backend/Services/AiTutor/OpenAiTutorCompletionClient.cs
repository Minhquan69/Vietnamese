using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Backend.Options;
using Microsoft.Extensions.Options;

namespace Backend.Services.AiTutor
{
    public sealed class OpenAiTutorCompletionClient : IAiTutorCompletionClient
    {
        private readonly HttpClient _http;
        private readonly AiTutorOptions _opt;

        public OpenAiTutorCompletionClient(HttpClient http, IOptions<AiTutorOptions> options)
        {
            _http = http;
            _opt = options.Value;
        }

        public async Task<string> CompleteAsync(
            IReadOnlyList<TutorChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            {
                throw new InvalidOperationException("AiTutor:ApiKey is not configured.");
            }

            var payload = new
            {
                model = _opt.Model,
                temperature = _opt.Temperature,
                max_tokens = _opt.MaxTokens,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToList(),
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, _opt.ChatCompletionsUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey.Trim());
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var res = await _http.SendAsync(req, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);

            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"OpenAI error {(int)res.StatusCode}: {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Empty completion from AI provider.");
            }

            return content;
        }
    }
}
