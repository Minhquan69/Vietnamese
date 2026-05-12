using Backend.Common;
using Backend.dto;
using Backend.Models;
using Backend.Repository;

namespace Backend.Services.impl
{
    public class VocabularyLearningServiceImpl : VocabularyLearningService
    {
        private const decimal MasteredThreshold = 85m;

        private readonly VocabularyRepository _vocabularyRepository;
        private readonly UserVocabularyRepository _userVocabularyRepository;
        private readonly UserContextUtil _userContext;

        public VocabularyLearningServiceImpl(
            VocabularyRepository vocabularyRepository,
            UserVocabularyRepository userVocabularyRepository,
            UserContextUtil userContext)
        {
            _vocabularyRepository = vocabularyRepository;
            _userVocabularyRepository = userVocabularyRepository;
            _userContext = userContext;
        }

        public async Task<VocabularyListResultDto> SearchAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var skip = (page - 1) * pageSize;

            var (items, total) = await _vocabularyRepository.SearchAsync(search, skip, pageSize);

            return new VocabularyListResultDto
            {
                Total = total,
                Items = items.Select(MapVocabulary).ToList(),
            };
        }

        public async Task<VocabularyCardDto?> GetVocabularyAsync(int vocabularyId)
        {
            var v = await _vocabularyRepository.GetByIdAsync(vocabularyId);
            return v == null ? null : MapVocabulary(v);
        }

        public async Task<UserVocabularyCardDto?> GetUserCardAsync(int vocabularyId)
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                return null;
            }

            var v = await _vocabularyRepository.GetByIdAsync(vocabularyId);
            if (v == null)
            {
                return null;
            }

            var uv = await _userVocabularyRepository.GetAsync(userId, vocabularyId);
            var now = DateTime.UtcNow;
            return MapUserCard(v, uv, now);
        }

        public async Task<List<UserVocabularyCardDto>> GetDeckAsync(int limit)
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            limit = Math.Clamp(limit, 1, 60);
            var now = DateTime.UtcNow;

            var due = await _userVocabularyRepository.GetDueAsync(userId, now, limit);
            var result = new List<UserVocabularyCardDto>();
            foreach (var row in due)
            {
                if (row.Vocabulary != null)
                {
                    result.Add(MapUserCard(row.Vocabulary, row, now));
                }
            }

            var known = await _userVocabularyRepository.GetKnownVocabularyIdsAsync(userId);
            var exclude = known.ToHashSet();

            var remaining = limit - result.Count;
            if (remaining > 0)
            {
                var fresh = await _vocabularyRepository.GetRandomActiveAsync(remaining, exclude);
                foreach (var v in fresh)
                {
                    result.Add(MapUserCard(v, null, now));
                }
            }

            return result;
        }

        public async Task<ReviewResultDto> SubmitReviewAsync(int vocabularyId, string grade)
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var v = await _vocabularyRepository.GetByIdAsync(vocabularyId)
                ?? throw new InvalidOperationException("Vocabulary not found");

            var g = ParseGrade(grade);
            var uv = await _userVocabularyRepository.GetAsync(userId, vocabularyId);
            if (uv == null)
            {
                uv = new UserVocabulary
                {
                    UserId = userId,
                    VocabularyId = vocabularyId,
                    Saved = false,
                    EaseFactor = 2.5m,
                    IntervalDays = 0,
                    Repetitions = 0,
                    NextReviewUtc = DateTime.UtcNow,
                    MasteryScore = 0,
                    Familiarity = 0,
                };
                await _userVocabularyRepository.AddAsync(uv);
            }

            ApplyScheduling(uv, g);
            ApplyMastery(uv, g);

            _userVocabularyRepository.Update(uv);
            await _userVocabularyRepository.SaveAsync();

            return new ReviewResultDto
            {
                VocabularyId = vocabularyId,
                MasteryScore = uv.MasteryScore,
                Familiarity = uv.Familiarity,
                IntervalDays = uv.IntervalDays,
                Repetitions = uv.Repetitions,
                NextReviewUtc = uv.NextReviewUtc,
                EaseFactor = uv.EaseFactor,
            };
        }

        public async Task<SavedToggleResultDto> SetSavedAsync(int vocabularyId, bool saved)
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _ = await _vocabularyRepository.GetByIdAsync(vocabularyId)
                ?? throw new InvalidOperationException("Vocabulary not found");

            var uv = await _userVocabularyRepository.GetAsync(userId, vocabularyId);
            if (uv == null)
            {
                uv = new UserVocabulary
                {
                    UserId = userId,
                    VocabularyId = vocabularyId,
                    Saved = saved,
                    EaseFactor = 2.5m,
                    IntervalDays = 0,
                    Repetitions = 0,
                    NextReviewUtc = DateTime.UtcNow,
                    MasteryScore = 0,
                    Familiarity = 0,
                };
                await _userVocabularyRepository.AddAsync(uv);
            }
            else
            {
                uv.Saved = saved;
                if (saved && uv.NextReviewUtc > DateTime.UtcNow.AddDays(30))
                {
                    uv.NextReviewUtc = DateTime.UtcNow;
                }
            }

            await _userVocabularyRepository.SaveAsync();

            return new SavedToggleResultDto { VocabularyId = vocabularyId, Saved = uv.Saved };
        }

        public async Task<VocabularyStatsDto> GetStatsAsync()
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var now = DateTime.UtcNow;
            var saved = await _userVocabularyRepository.CountSavedAsync(userId);
            var due = await _userVocabularyRepository.CountDueAsync(userId, now);
            var mastered = await _userVocabularyRepository.CountMasteredAsync(userId, MasteredThreshold);
            var avg = await _userVocabularyRepository.AverageMasteryAsync(userId);

            return new VocabularyStatsDto
            {
                SavedCount = saved,
                DueCount = due,
                MasteredCount = mastered,
                AverageMastery = avg ?? 0,
            };
        }

        public async Task<List<UserVocabularyCardDto>> GetSavedListAsync()
        {
            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var now = DateTime.UtcNow;
            var rows = await _userVocabularyRepository.GetSavedWithVocabularyAsync(userId);
            var list = new List<UserVocabularyCardDto>();
            foreach (var uv in rows)
            {
                if (uv.Vocabulary != null)
                {
                    list.Add(MapUserCard(uv.Vocabulary, uv, now));
                }
            }

            return list;
        }

        private static int ParseGrade(string grade)
        {
            return grade?.Trim().ToLowerInvariant() switch
            {
                "again" => 1,
                "hard" => 2,
                "good" => 3,
                "easy" => 4,
                _ => 3,
            };
        }

        private static void ApplyScheduling(UserVocabulary uv, int grade)
        {
            var now = DateTime.UtcNow;
            var ef = (double)uv.EaseFactor;
            var interval = uv.IntervalDays;
            var reps = uv.Repetitions;

            if (grade == 1)
            {
                uv.Repetitions = 0;
                uv.IntervalDays = 1;
                ef = Math.Max(1.3, ef - 0.2);
            }
            else
            {
                if (reps == 0)
                {
                    interval = 1;
                }
                else if (reps == 1)
                {
                    interval = 6;
                }
                else
                {
                    interval = Math.Max(1, (int)Math.Round(interval * ef));
                }

                if (grade == 2)
                {
                    ef = Math.Max(1.3, ef - 0.15);
                }

                if (grade == 4)
                {
                    ef = Math.Min(3.0, ef + 0.15);
                }

                reps++;
                uv.Repetitions = reps;
                uv.IntervalDays = interval;
            }

            uv.EaseFactor = (decimal)ef;
            uv.LastReviewedUtc = now;
            uv.NextReviewUtc = now.AddDays(Math.Max(1, uv.IntervalDays));
        }

        private static void ApplyMastery(UserVocabulary uv, int grade)
        {
            var bump = grade switch
            {
                1 => -8m,
                2 => 5m,
                3 => 12m,
                4 => 18m,
                _ => 10m,
            };

            uv.MasteryScore = Math.Clamp(uv.MasteryScore + bump, 0m, 100m);
            if (uv.Repetitions >= 5 && uv.MasteryScore < 95)
            {
                uv.MasteryScore = Math.Min(100m, uv.MasteryScore + 4m);
            }

            uv.Familiarity = (byte)(uv.MasteryScore switch
            {
                < 25 => 0,
                < 55 => 1,
                < 85 => 2,
                _ => 3,
            });
        }

        private static VocabularyCardDto MapVocabulary(Vocabulary v)
        {
            return new VocabularyCardDto
            {
                VocabularyId = v.VocabularyId,
                Word = v.Word,
                Ipa = v.Ipa,
                AudioUrl = v.AudioUrl,
                MeaningEn = v.MeaningEn,
                PartOfSpeech = v.PartOfSpeech,
                ExampleSentence = v.ExampleSentence,
                ExampleTranslation = v.ExampleTranslation,
                ContextNote = v.ContextNote,
            };
        }

        private static UserVocabularyCardDto MapUserCard(Vocabulary v, UserVocabulary? uv, DateTime nowUtc)
        {
            var dto = new UserVocabularyCardDto
            {
                VocabularyId = v.VocabularyId,
                Word = v.Word,
                Ipa = v.Ipa,
                AudioUrl = v.AudioUrl,
                MeaningEn = v.MeaningEn,
                PartOfSpeech = v.PartOfSpeech,
                ExampleSentence = v.ExampleSentence,
                ExampleTranslation = v.ExampleTranslation,
                ContextNote = v.ContextNote,
                Saved = uv?.Saved ?? false,
                MasteryScore = uv?.MasteryScore ?? 0,
                Familiarity = uv?.Familiarity ?? 0,
                IntervalDays = uv?.IntervalDays ?? 0,
                Repetitions = uv?.Repetitions ?? 0,
                NextReviewUtc = uv?.NextReviewUtc ?? nowUtc,
                IsDue = uv == null || uv.NextReviewUtc <= nowUtc,
            };
            return dto;
        }
    }
}
