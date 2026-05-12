using Backend.dto;
using Backend.Models;
using Backend.Options;
using Backend.Repository;
using Backend.Services.AiTutor;
using Microsoft.Extensions.Options;

namespace Backend.Services.impl
{
    public sealed class AiTutorServiceImpl : AiTutorService
    {
        private readonly TutorConversationRepository _repo;
        private readonly IAiTutorCompletionClient _ai;
        private readonly AiTutorOptions _opt;

        public AiTutorServiceImpl(
            TutorConversationRepository repo,
            IAiTutorCompletionClient ai,
            IOptions<AiTutorOptions> options)
        {
            _repo = repo;
            _ai = ai;
            _opt = options.Value;
        }

        public async Task<AiTutorChatResponseDto> ChatAsync(int userId, AiTutorChatRequestDto request)
        {
            var text = (request.Message ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                throw new ArgumentException("Message is required.");
            }

            TutorConversation conv;
            if (request.ConversationId.HasValue && request.ConversationId.Value > 0)
            {
                var cid = request.ConversationId.Value;
                var existing = await _repo.GetAsync(userId, cid);
                if (existing == null)
                {
                    throw new InvalidOperationException("Conversation not found.");
                }

                conv = existing;
            }
            else
            {
                var scenario = NormalizeScenario(request.ScenarioKey);
                conv = await _repo.CreateAsync(new TutorConversation
                {
                    UserId = userId,
                    Title = text.Length > 80 ? text.Substring(0, 80) + "…" : text,
                    ScenarioKey = scenario,
                });
            }

            await _repo.AddMessageAsync(new TutorMessage
            {
                TutorConversationId = conv.TutorConversationId,
                Role = "user",
                Content = text,
            });

            var history = await _repo.GetMessagesAsync(conv.TutorConversationId, _opt.HistoryMessageLimit);
            var scenarioForPrompt = conv.ScenarioKey ?? NormalizeScenario(request.ScenarioKey);

            var messages = new List<TutorChatMessage>
            {
                new() { Role = "system", Content = TutorSystemPrompt.Build(scenarioForPrompt) },
            };

            foreach (var m in history)
            {
                messages.Add(new TutorChatMessage { Role = m.Role, Content = m.Content });
            }

            var raw = await _ai.CompleteAsync(messages);
            var (assistant, suggestions) = SplitSuggestions(raw);

            await _repo.AddMessageAsync(new TutorMessage
            {
                TutorConversationId = conv.TutorConversationId,
                Role = "assistant",
                Content = assistant,
            });

            await _repo.TouchAsync(conv.TutorConversationId, null);

            return new AiTutorChatResponseDto
            {
                ConversationId = conv.TutorConversationId,
                AssistantMessage = assistant,
                Suggestions = suggestions,
            };
        }

        public async Task<List<TutorConversationSummaryDto>> ListConversationsAsync(int userId)
        {
            var rows = await _repo.ListAsync(userId, 40);
            return rows.Select(c => new TutorConversationSummaryDto
            {
                ConversationId = c.TutorConversationId,
                Title = c.Title,
                ScenarioKey = c.ScenarioKey,
                UpdatedUtc = c.UpdatedUtc,
            }).ToList();
        }

        public async Task<List<TutorMessageDto>> GetMessagesAsync(int userId, int conversationId)
        {
            var conv = await _repo.GetAsync(userId, conversationId);
            if (conv == null)
            {
                return new List<TutorMessageDto>();
            }

            var msgs = await _repo.GetMessagesAsync(conversationId, 200);
            return msgs.Select(m => new TutorMessageDto
            {
                MessageId = m.TutorMessageId,
                Role = m.Role,
                Content = m.Content,
                CreatedUtc = m.CreatedUtc,
            }).ToList();
        }

        private static string? NormalizeScenario(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var k = key.Trim().ToLowerInvariant();
            return k switch
            {
                "restaurant" or "airport" or "hotel" or "shopping" or "daily" or "daily_communication" => k == "daily_communication" ? "daily" : k,
                _ => k,
            };
        }

        private static (string body, List<string> chips) SplitSuggestions(string raw)
        {
            const string marker = "<<<SUGGESTIONS>>>";
            var idx = raw.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
            {
                return (raw.Trim(), new List<string>());
            }

            var body = raw[..idx].Trim();
            var tail = raw[(idx + marker.Length)..].Trim();
            var chips = tail
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => s.Length > 0)
                .Take(6)
                .ToList();

            return (body, chips);
        }
    }
}
