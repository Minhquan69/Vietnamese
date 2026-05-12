using System.Text.Json;
using Backend.dto;
using Backend.Models;
using Backend.Options;
using Backend.Repository;
using Backend.Services;
using Backend.Services.Speaking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Backend.Services.impl
{
    public sealed class SpeakingEvaluationServiceImpl : SpeakingEvaluationService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".webm", ".wav", ".mp3", ".m4a", ".ogg", ".mp4", ".mpeg", ".mpga",
        };

        private const int AnalyticsWindow = 100;
        private const int RecentPreviewChars = 160;

        private readonly OpenAiSpeakingClient _openAi;
        private readonly SpeakingAttemptRepository _attempts;
        private readonly SpeakingOptions _options;
        private readonly IWebHostEnvironment _env;
        private readonly GamificationService _gamification;

        public SpeakingEvaluationServiceImpl(
            OpenAiSpeakingClient openAi,
            SpeakingAttemptRepository attempts,
            IOptions<SpeakingOptions> options,
            IWebHostEnvironment env,
            GamificationService gamification)
        {
            _openAi = openAi;
            _attempts = attempts;
            _options = options.Value;
            _env = env;
            _gamification = gamification;
        }

        public async Task<SpeakingEvaluateResponseDto> EvaluateAsync(
            int userId,
            Stream audioStream,
            string originalFileName,
            string? referenceText,
            int durationMs,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user.");
            }

            await using var capped = await ReadCappedAudioAsync(audioStream, cancellationToken);

            var ext = SanitizeExtension(originalFileName);
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var relative = $"/uploads/speaking/{userId}/{storedName}";

            var wwwroot = _env.WebRootPath
                ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var dir = Path.Combine(wwwroot, "uploads", "speaking", userId.ToString());
            Directory.CreateDirectory(dir);
            var absolute = Path.Combine(dir, storedName);
            await using (var fs = new FileStream(absolute, FileMode.Create, FileAccess.Write))
            {
                capped.Position = 0;
                await capped.CopyToAsync(fs, cancellationToken);
            }

            capped.Position = 0;
            var uploadName = string.IsNullOrWhiteSpace(originalFileName)
                ? $"recording{ext}"
                : Path.GetFileName(originalFileName);

            SpeakingScoreResult score;
            string transcript;

            if (_openAi.HasApiKey)
            {
                transcript = await _openAi.TranscribeAsync(capped, uploadName, cancellationToken);
                if (string.IsNullOrWhiteSpace(transcript))
                {
                    transcript = "(Không nhận diện được lời nói rõ ràng.)";
                }

                score = await _openAi.ScoreAsync(transcript, referenceText, cancellationToken);
            }
            else
            {
                transcript = BuildStubTranscript(referenceText);
                score = StubScores(referenceText);
            }

            var analyticsObj = new { score.Tips, mode = _openAi.HasApiKey ? "live" : "demo" };
            var row = new SpeakingAttempt
            {
                UserId = userId,
                ReferenceText = string.IsNullOrWhiteSpace(referenceText) ? null : referenceText.Trim(),
                Transcript = transcript,
                AudioRelativePath = relative,
                DurationMs = durationMs < 0 ? 0 : durationMs,
                PronunciationScore = score.Pronunciation,
                FluencyScore = score.Fluency,
                ToneScore = score.Tone,
                OverallScore = score.Overall,
                Feedback = score.Feedback,
                AnalyticsJson = JsonSerializer.Serialize(analyticsObj),
            };

            await _attempts.AddAsync(row);

            try
            {
                await _gamification.RecordSpeakingEvaluatedAsync(userId, row.SpeakingAttemptId);
            }
            catch
            {
                /* non-blocking */
            }

            return new SpeakingEvaluateResponseDto
            {
                AttemptId = row.SpeakingAttemptId,
                Transcript = transcript,
                AudioUrl = relative,
                DurationMs = row.DurationMs,
                PronunciationScore = score.Pronunciation,
                FluencyScore = score.Fluency,
                ToneScore = score.Tone,
                OverallScore = score.Overall,
                Feedback = score.Feedback,
                Tips = score.Tips,
            };
        }

        public async Task<SpeakingAnalyticsDto> GetAnalyticsAsync(int userId)
        {
            var rows = await _attempts.ListByUserAsync(userId, AnalyticsWindow);
            return BuildAnalytics(rows);
        }

        public async Task<List<SpeakingAttemptSummaryDto>> GetHistoryAsync(int userId, int take)
        {
            var rows = await _attempts.ListByUserAsync(userId, Math.Clamp(take, 1, 200));
            return rows.Select(MapSummary).ToList();
        }

        private static SpeakingAttemptSummaryDto MapSummary(SpeakingAttempt x)
        {
            var preview = x.Transcript ?? "";
            if (preview.Length > RecentPreviewChars)
            {
                preview = preview[..RecentPreviewChars] + "…";
            }

            return new SpeakingAttemptSummaryDto
            {
                AttemptId = x.SpeakingAttemptId,
                ReferenceText = x.ReferenceText,
                TranscriptPreview = preview,
                DurationMs = x.DurationMs,
                OverallScore = x.OverallScore,
                CreatedUtc = x.CreatedUtc,
            };
        }

        private SpeakingAnalyticsDto BuildAnalytics(List<SpeakingAttempt> rows)
        {
            var dto = new SpeakingAnalyticsDto
            {
                AttemptCount = rows.Count,
                Recent = rows.Take(12).Select(MapSummary).ToList(),
            };

            if (rows.Count == 0)
            {
                return dto;
            }

            dto.AverageOverall = Math.Round(rows.Average(r => r.OverallScore), 1);
            dto.AveragePronunciation = Math.Round(rows.Average(r => r.PronunciationScore), 1);
            dto.AverageFluency = Math.Round(rows.Average(r => r.FluencyScore), 1);
            dto.AverageTone = Math.Round(rows.Average(r => r.ToneScore), 1);
            return dto;
        }

        private async Task<MemoryStream> ReadCappedAudioAsync(Stream source, CancellationToken ct)
        {
            var max = _options.MaxAudioBytes;
            var ms = new MemoryStream(capacity: Math.Min(max + 1, 65_536));
            var buffer = new byte[8192];
            long total = 0;
            int read;
            while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                total += read;
                if (total > max)
                {
                    throw new ArgumentException($"Audio exceeds maximum size of {max} bytes.");
                }

                ms.Write(buffer, 0, read);
            }

            return ms;
        }

        private static string SanitizeExtension(string originalFileName)
        {
            var ext = Path.GetExtension(originalFileName);
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            {
                return ".webm";
            }

            return ext;
        }

        private static string BuildStubTranscript(string? referenceText)
        {
            if (!string.IsNullOrWhiteSpace(referenceText))
            {
                return referenceText.Trim();
            }

            return "Xin chào, tôi đang luyện nói tiếng Việt.";
        }

        private static SpeakingScoreResult StubScores(string? referenceText)
        {
            var hasRef = !string.IsNullOrWhiteSpace(referenceText);
            var p = hasRef ? 78m : 70m;
            var f = hasRef ? 74m : 68m;
            var t = hasRef ? 76m : 72m;
            var o = Math.Round(0.45m * p + 0.35m * f + 0.20m * t, 1);
            return new SpeakingScoreResult
            {
                Pronunciation = p,
                Fluency = f,
                Tone = t,
                Overall = o,
                Feedback =
                    "Đây là chế độ demo khi chưa cấu hình API. " +
                    "Khi bật OpenAI (AiTutor:ApiKey hoặc Speaking:ApiKey), hệ thống sẽ nhận diện giọng nói và chấm điểm thực tế.",
                Tips = new List<string>
                {
                    "Nối âm cuối chữ trước với âm đầu chữ sau cho mượt.",
                    "Giữ nhịp hơi ngắn giữa các cụm từ.",
                    "Đọc rõ thanh điệu trên vowel chính.",
                },
            };
        }
    }
}
