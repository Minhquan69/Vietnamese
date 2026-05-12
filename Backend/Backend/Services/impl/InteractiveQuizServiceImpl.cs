using System.Text.Json;
using System.Text.Json.Nodes;
using Backend.Common;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using Backend.Services;

namespace Backend.Services.impl
{
    public class InteractiveQuizServiceImpl : InteractiveQuizService
    {
        private readonly QuizRepository _quizRepository;
        private readonly QuestionRepository _questionRepository;
        private readonly UserQuizRepository _userQuizRepository;
        private readonly UserAnswerRepository _userAnswerRepository;
        private readonly QuizAttemptRepository _quizAttemptRepository;
        private readonly UserContextUtil _userContext;
        private readonly GamificationService _gamification;

        public InteractiveQuizServiceImpl(
            QuizRepository quizRepository,
            QuestionRepository questionRepository,
            UserQuizRepository userQuizRepository,
            UserAnswerRepository userAnswerRepository,
            QuizAttemptRepository quizAttemptRepository,
            UserContextUtil userContext,
            GamificationService gamification)
        {
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _userQuizRepository = userQuizRepository;
            _userAnswerRepository = userAnswerRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _userContext = userContext;
            _gamification = gamification;
        }

        public async Task<PlayerQuizPackageDto?> GetQuizForPlayerAsync(int quizId)
        {
            var meta = await _quizRepository.GetQuizSummary(quizId);
            if (meta == null || !meta.IsActive)
            {
                return null;
            }

            var flat = await _questionRepository.GetQuestionsByQuiz(quizId);
            var ordered = flat
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.QuestionId)
                .ToList();

            var pkg = new PlayerQuizPackageDto
            {
                QuizId = meta.QuizId,
                QuizName = meta.QuizName,
                TimeLimitMinutes = meta.TimeLimit,
                PassScore = meta.PassScore,
                Questions = ordered.Select(MapPlayerQuestion).ToList(),
            };

            return pkg;
        }

        public async Task<InteractiveQuizResultDto?> SubmitAsync(InteractiveQuizSubmitDto dto)
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var meta = await _quizRepository.GetQuizSummary(dto.QuizId);
            if (meta == null || !meta.IsActive)
            {
                return null;
            }

            var questions = await _questionRepository.GetQuestionsByQuiz(dto.QuizId);
            var ordered = questions
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.QuestionId)
                .ToList();

            var questionSet = ordered.Select(x => x.QuestionId).ToHashSet();
            var responseByQ = dto.Responses
                .Where(x => questionSet.Contains(x.QuestionId))
                .GroupBy(x => x.QuestionId)
                .ToDictionary(g => g.Key, g => g.Last());

            decimal totalPoints = ordered.Sum(x => x.Score);
            decimal earned = 0;
            var outcomes = new List<InteractiveQuestionOutcomeDto>();
            var rows = new List<UserAnswer>();

            int correct = 0;
            int wrong = 0;
            int skipped = 0;

            foreach (var q in ordered)
            {
                responseByQ.TryGetValue(q.QuestionId, out var resp);
                var type = NormalizeType(q.QuestionType);

                var grade = GradeQuestion(q, type, resp);
                if (grade.Skipped)
                {
                    skipped++;
                }
                else if (grade.Correct)
                {
                    correct++;
                    earned += q.Score;
                }
                else
                {
                    wrong++;
                }

                outcomes.Add(new InteractiveQuestionOutcomeDto
                {
                    QuestionId = q.QuestionId,
                    QuestionType = type,
                    Correct = grade.Correct && !grade.Skipped,
                    Skipped = grade.Skipped,
                    PointsEarned = grade.Correct ? q.Score : 0,
                    Explanation = q.Explanation,
                    ReviewHintJson = grade.ReviewHintJson,
                });

                rows.Add(new UserAnswer
                {
                    QuestionId = q.QuestionId,
                    AnswerId = grade.StoredAnswerId,
                    ResponsePayload = grade.StoredPayloadJson,
                    UserQuizId = 0,
                });
            }

            decimal scorePercent = totalPoints == 0
                ? 0
                : Math.Round(earned / totalPoints * 100, 2);

            bool passed = meta.PassScore != null && scorePercent >= meta.PassScore;

            var userQuiz = new UserQuiz
            {
                UserId = userId,
                QuizId = dto.QuizId,
                Score = scorePercent,
                CompletedDate = DateTime.Now,
                IsPassed = passed,
            };

            await _userQuizRepository.SaveUserQuiz(userQuiz);

            var persisted = await _userQuizRepository.GetUserQuiz(userId, dto.QuizId);
            var userQuizId = persisted?.UserQuizId ?? userQuiz.UserQuizId;

            await _userAnswerRepository.DeleteByUserQuizId(userQuizId);

            foreach (var r in rows)
            {
                r.UserQuizId = userQuizId;
            }

            await _userAnswerRepository.AddUserAnswers(rows);
            await _userAnswerRepository.Save();

            var detailJson = JsonSerializer.Serialize(outcomes);

            await _quizAttemptRepository.AddAttempt(new QuizAttempt
            {
                UserId = userId,
                QuizId = dto.QuizId,
                ScorePercent = scorePercent,
                DurationSeconds = Math.Max(0, dto.DurationSeconds),
                CorrectCount = correct,
                WrongCount = wrong,
                SkippedCount = skipped,
                SubmittedUtc = DateTime.UtcNow,
                DetailJson = detailJson,
            });

            if (passed)
            {
                try
                {
                    await _gamification.RecordQuizPassedAsync(userId, dto.QuizId, scorePercent);
                }
                catch
                {
                    /* non-blocking */
                }
            }

            return new InteractiveQuizResultDto
            {
                ScorePercent = scorePercent,
                Passed = passed,
                CorrectCount = correct,
                WrongCount = wrong,
                SkippedCount = skipped,
                Items = outcomes,
            };
        }

        public async Task<List<QuizAttemptSummaryDto>> GetRecentAttemptsAsync(int? quizId)
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var list = await _quizAttemptRepository.ListRecentByUser(userId, quizId, 25);
            return list.Select(x => new QuizAttemptSummaryDto
            {
                QuizAttemptId = x.QuizAttemptId,
                QuizId = x.QuizId,
                ScorePercent = x.ScorePercent,
                DurationSeconds = x.DurationSeconds,
                CorrectCount = x.CorrectCount,
                WrongCount = x.WrongCount,
                SkippedCount = x.SkippedCount,
                SubmittedUtc = x.SubmittedUtc,
            }).ToList();
        }

        private static PlayerQuestionDto MapPlayerQuestion(Question q)
        {
            var type = NormalizeType(q.QuestionType);
            var answers = (q.Answers ?? Enumerable.Empty<Answer>())
                .OrderBy(a => a.OrderIndex)
                .ThenBy(a => a.AnswerId)
                .Select(a => new PlayerAnswerDto
                {
                    AnswerId = a.AnswerId,
                    AnswerText = a.AnswerText,
                    ImageUrl = a.ImageUrl,
                    AudioUrl = a.AudioUrl,
                    OrderIndex = a.OrderIndex,
                })
                .ToList();

            return new PlayerQuestionDto
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                QuestionType = type,
                ImageUrl = q.ImageUrl,
                AudioUrl = q.AudioUrl,
                Score = q.Score,
                OrderIndex = q.OrderIndex,
                InteractivePayload = RedactPayload(q.InteractivePayload, type),
                Answers = answers,
            };
        }

        private static string? RedactPayload(string? raw, string questionType)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return raw;
            }

            JsonNode? root;
            try
            {
                root = JsonNode.Parse(raw);
            }
            catch
            {
                return "{}";
            }

            var o = root.AsObject();
            if (o == null)
            {
                return "{}";
            }

            switch (questionType)
            {
                case QuizQuestionTypes.FillBlank:
                    if (o["blanks"] is JsonArray blanks)
                    {
                        foreach (var item in blanks)
                        {
                            item?.AsObject()?.Remove("accepted");
                        }
                    }

                    break;
                case QuizQuestionTypes.ReorderSentence:
                    o.Remove("orderedAnswerIds");
                    break;
                case QuizQuestionTypes.DragDrop:
                    o.Remove("correctMap");
                    break;
            }

            return o.ToJsonString();
        }

        private static string NormalizeType(string? t)
        {
            if (string.IsNullOrWhiteSpace(t))
            {
                return QuizQuestionTypes.MultipleChoice;
            }

            return t.Trim();
        }

        private sealed class GradeResult
        {
            public bool Correct { get; init; }
            public bool Skipped { get; init; }
            public int? StoredAnswerId { get; init; }
            public string? StoredPayloadJson { get; init; }
            public string? ReviewHintJson { get; init; }
        }

        private static GradeResult GradeQuestion(Question q, string type, QuizResponseItemDto? resp)
        {
            if (resp == null)
            {
                return new GradeResult { Skipped = true, Correct = false };
            }

            return type switch
            {
                QuizQuestionTypes.FillBlank => GradeFillBlank(q, resp),
                QuizQuestionTypes.ReorderSentence => GradeReorder(q, resp),
                QuizQuestionTypes.DragDrop => GradeDragDrop(q, resp),
                QuizQuestionTypes.Listening => GradeMc(q, resp),
                QuizQuestionTypes.MultipleChoice => GradeMc(q, resp),
                _ => GradeMc(q, resp),
            };
        }

        private static GradeResult GradeMc(Question q, QuizResponseItemDto resp)
        {
            if (resp.AnswerId == null)
            {
                return new GradeResult { Skipped = true, Correct = false };
            }

            var correct = q.Answers?.FirstOrDefault(a => a.IsCorrect);
            var ok = correct != null && correct.AnswerId == resp.AnswerId.Value;
            var hint = correct == null
                ? null
                : JsonSerializer.Serialize(new { correctAnswerId = correct.AnswerId });

            return new GradeResult
            {
                Correct = ok,
                Skipped = false,
                StoredAnswerId = resp.AnswerId,
                StoredPayloadJson = null,
                ReviewHintJson = hint,
            };
        }

        private static GradeResult GradeFillBlank(Question q, QuizResponseItemDto resp)
        {
            var payloadJson = JsonSerializer.Serialize(resp.FillBlank ?? new Dictionary<string, string>());
            if (q.InteractivePayload == null)
            {
                return new GradeResult
                {
                    Correct = false,
                    Skipped = resp.FillBlank == null || resp.FillBlank.Count == 0,
                    StoredPayloadJson = payloadJson,
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(q.InteractivePayload);
                if (!doc.RootElement.TryGetProperty("blanks", out var blanksEl))
                {
                    return new GradeResult { Correct = false, StoredPayloadJson = payloadJson };
                }

                var user = resp.FillBlank ?? new Dictionary<string, string>();
                var hintObj = new Dictionary<string, object?>();
                var allOk = true;
                var anyInput = false;

                foreach (var blank in blanksEl.EnumerateArray())
                {
                    if (!blank.TryGetProperty("key", out var keyEl))
                    {
                        continue;
                    }

                    var key = keyEl.GetString() ?? "";
                    var accepted = blank.TryGetProperty("accepted", out var accEl)
                        ? accEl.EnumerateArray().Select(x => x.GetString() ?? "").Where(s => s.Length > 0).ToList()
                        : new List<string>();

                    var caseInsensitive = blank.TryGetProperty("caseInsensitive", out var ciEl) && ciEl.GetBoolean();
                    var normalizeWs = !blank.TryGetProperty("normalizeWhitespace", out var wsEl) || wsEl.GetBoolean();

                    user.TryGetValue(key, out var rawVal);
                    var val = rawVal ?? "";
                    anyInput |= val.Length > 0;

                    var normalizedUser = NormalizeAnswerText(val, normalizeWs);
                    var match = accepted.Any(a =>
                        string.Equals(
                            NormalizeAnswerText(a, normalizeWs),
                            normalizedUser,
                            caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

                    if (!match)
                    {
                        allOk = false;
                    }

                    hintObj[key] = new { accepted };
                }

                var skipped = !anyInput;
                return new GradeResult
                {
                    Correct = !skipped && allOk && blanksEl.GetArrayLength() > 0,
                    Skipped = skipped,
                    StoredPayloadJson = payloadJson,
                    ReviewHintJson = JsonSerializer.Serialize(hintObj),
                };
            }
            catch
            {
                return new GradeResult { Correct = false, StoredPayloadJson = payloadJson };
            }
        }

        private static GradeResult GradeReorder(Question q, QuizResponseItemDto resp)
        {
            var payloadJson = JsonSerializer.Serialize(resp.OrderedAnswerIds ?? new List<int>());
            if (q.InteractivePayload == null || resp.OrderedAnswerIds == null || resp.OrderedAnswerIds.Count == 0)
            {
                return new GradeResult
                {
                    Correct = false,
                    Skipped = resp.OrderedAnswerIds == null || resp.OrderedAnswerIds.Count == 0,
                    StoredPayloadJson = payloadJson,
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(q.InteractivePayload);
                if (!doc.RootElement.TryGetProperty("orderedAnswerIds", out var ordEl))
                {
                    return new GradeResult { Correct = false, StoredPayloadJson = payloadJson };
                }

                var expected = ordEl.EnumerateArray().Select(x => x.GetInt32()).ToList();
                var user = resp.OrderedAnswerIds;
                var ok = expected.Count == user.Count && expected.SequenceEqual(user);
                var hint = JsonSerializer.Serialize(new { orderedAnswerIds = expected });

                return new GradeResult
                {
                    Correct = ok,
                    Skipped = false,
                    StoredPayloadJson = payloadJson,
                    ReviewHintJson = hint,
                };
            }
            catch
            {
                return new GradeResult { Correct = false, StoredPayloadJson = payloadJson };
            }
        }

        private static GradeResult GradeDragDrop(Question q, QuizResponseItemDto resp)
        {
            var payloadJson = JsonSerializer.Serialize(resp.DragDrop ?? new Dictionary<string, int>());
            if (q.InteractivePayload == null || resp.DragDrop == null || resp.DragDrop.Count == 0)
            {
                return new GradeResult
                {
                    Correct = false,
                    Skipped = resp.DragDrop == null || resp.DragDrop.Count == 0,
                    StoredPayloadJson = payloadJson,
                };
            }

            try
            {
                using var doc = JsonDocument.Parse(q.InteractivePayload);
                if (!doc.RootElement.TryGetProperty("correctMap", out var mapEl))
                {
                    return new GradeResult { Correct = false, StoredPayloadJson = payloadJson };
                }

                var ok = true;
                foreach (var p in mapEl.EnumerateObject())
                {
                    var slot = p.Name;
                    var expectedId = p.Value.GetInt32();
                    if (!resp.DragDrop.TryGetValue(slot, out var uid) || uid != expectedId)
                    {
                        ok = false;
                    }
                }

                foreach (var kv in resp.DragDrop)
                {
                    if (!mapEl.TryGetProperty(kv.Key, out _))
                    {
                        ok = false;
                    }
                }

                var hint = mapEl.GetRawText();

                return new GradeResult
                {
                    Correct = ok,
                    Skipped = false,
                    StoredPayloadJson = payloadJson,
                    ReviewHintJson = hint,
                };
            }
            catch
            {
                return new GradeResult { Correct = false, StoredPayloadJson = payloadJson };
            }
        }

        private static string NormalizeAnswerText(string s, bool normalizeWhitespace)
        {
            var t = s.Trim();
            if (normalizeWhitespace)
            {
                t = string.Join(' ', t.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            }

            return t;
        }
    }
}
