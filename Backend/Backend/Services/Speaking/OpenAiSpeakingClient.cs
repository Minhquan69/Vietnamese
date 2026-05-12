using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Backend.Options;
using Microsoft.Extensions.Options;

namespace Backend.Services.Speaking
{
    public sealed class OpenAiSpeakingClient
    {
        private readonly HttpClient _http;
        private readonly SpeakingOptions _speaking;
        private readonly AiTutorOptions _tutor;

        public OpenAiSpeakingClient(
            HttpClient http,
            IOptions<SpeakingOptions> speaking,
            IOptions<AiTutorOptions> tutor)
        {
            _http = http;
            _speaking = speaking.Value;
            _tutor = tutor.Value;
        }

        private string ResolveApiKey()
        {
            if (!string.IsNullOrWhiteSpace(_speaking.ApiKey))
            {
                return _speaking.ApiKey.Trim();
            }

            return _tutor.ApiKey?.Trim() ?? "";
        }

        public bool HasApiKey => ResolveApiKey().Length > 0;

        public async Task<string> TranscribeAsync(
            Stream audioStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var key = ResolveApiKey();
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("No API key configured for speech services.");
            }

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(_speaking.WhisperModel), "model");
            form.Add(new StringContent("json"), "response_format");
            form.Add(new StreamContent(audioStream), "file", fileName);

            using var req = new HttpRequestMessage(HttpMethod.Post, _speaking.TranscriptionsUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            req.Content = form;

            using var res = await _http.SendAsync(req, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Whisper error {(int)res.StatusCode}: {body}");
            }

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("text", out var t))
            {
                return t.GetString()?.Trim() ?? "";
            }

            return body.Trim();
        }

        public async Task<SpeakingScoreResult> ScoreAsync(
            string transcript,
            string? referenceText,
            CancellationToken cancellationToken = default)
        {
            var key = ResolveApiKey();
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("No API key configured for speech services.");
            }

            var userPayload = new
            {
                transcript,
                referencePhrase = referenceText ?? "",
            };

            var system =
                """
You evaluate Vietnamese learner speaking from an automatic speech-to-text transcript (ASR may be imperfect).
Score 0-100 for: pronunciation (plausibility of Vietnamese spelling/diacritics vs intended sounds), fluency (smoothness, completeness, filler), tone (appropriate Northern/Southern neutral tone marks and phrasing for learners).
Also compute overall as weighted average: 0.45*pronunciation + 0.35*fluency + 0.20*tone.
Return ONLY compact JSON (no markdown fences) with keys: pronunciation, fluency, tone, overall, feedback (2-4 sentences mixing Vietnamese examples and brief English), tips (array of 3-5 short actionable strings in Vietnamese).
""";

            var payload = new
            {
                model = _speaking.EvaluatorModel,
                temperature = 0.3,
                max_tokens = 700,
                messages = new object[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = JsonSerializer.Serialize(userPayload) },
                },
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, _speaking.ChatCompletionsUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var res = await _http.SendAsync(req, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Evaluator error {(int)res.StatusCode}: {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var raw = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
            try
            {
                return ParseScoreJson(raw);
            }
            catch (JsonException)
            {
                return new SpeakingScoreResult
                {
                    Pronunciation = 60,
                    Fluency = 60,
                    Tone = 60,
                    Overall = 60,
                    Feedback = "Không đọc được kết quả chấm điểm từ AI. Thử ghi âm rõ hơn hoặc thử lại sau.",
                    Tips = new List<string> { "Ghi âm trong môi trường yên tĩnh.", "Nói gần micro hơn.", "Thử câu ngắn hơn." },
                };
            }
        }

        private static SpeakingScoreResult ParseScoreJson(string raw)
        {
            var cleaned = StripCodeFences(raw.Trim());
            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            var tips = new List<string>();
            if (root.TryGetProperty("tips", out var tipsEl) && tipsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var x in tipsEl.EnumerateArray())
                {
                    var s = x.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        tips.Add(s.Trim());
                    }
                }
            }

            return new SpeakingScoreResult
            {
                Pronunciation = ClampScore(GetDec(root, "pronunciation")),
                Fluency = ClampScore(GetDec(root, "fluency")),
                Tone = ClampScore(GetDec(root, "tone")),
                Overall = ClampScore(GetDec(root, "overall")),
                Feedback = root.TryGetProperty("feedback", out var f) ? f.GetString()?.Trim() : null,
                Tips = tips,
            };
        }

        private static string StripCodeFences(string s)
        {
            if (s.StartsWith("```", StringComparison.Ordinal))
            {
                var i = s.IndexOf('\n');
                if (i > 0)
                {
                    s = s[(i + 1)..];
                }

                var j = s.LastIndexOf("```", StringComparison.Ordinal);
                if (j > 0)
                {
                    s = s[..j];
                }
            }

            return s.Trim();
        }

        private static decimal GetDec(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var p))
            {
                return 0;
            }

            return p.ValueKind switch
            {
                JsonValueKind.Number => p.GetDecimal(),
                JsonValueKind.String => decimal.TryParse(p.GetString(), out var d) ? d : 0,
                _ => 0,
            };
        }

        private static decimal ClampScore(decimal v)
        {
            if (v < 0)
            {
                return 0;
            }

            return v > 100 ? 100 : Math.Round(v, 1);
        }
    }

    public sealed class SpeakingScoreResult
    {
        public decimal Pronunciation { get; init; }

        public decimal Fluency { get; init; }

        public decimal Tone { get; init; }

        public decimal Overall { get; init; }

        public string? Feedback { get; init; }

        public List<string> Tips { get; init; } = new();
    }
}
