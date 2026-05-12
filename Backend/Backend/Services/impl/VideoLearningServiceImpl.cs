using System.Text.RegularExpressions;
using Backend.Common;
using Backend.Data;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.impl
{
    public class VideoLearningServiceImpl : VideoLearningService
    {
        private readonly AppDbContext _db;
        private readonly VideoRepository _videoRepository;
        private readonly VideoVocabularyRepository _videoVocabularyRepository;
        private readonly VocabularyRepository _vocabularyRepository;

        public VideoLearningServiceImpl(
            AppDbContext db,
            VideoRepository videoRepository,
            VideoVocabularyRepository videoVocabularyRepository,
            VocabularyRepository vocabularyRepository)
        {
            _db = db;
            _videoRepository = videoRepository;
            _videoVocabularyRepository = videoVocabularyRepository;
            _vocabularyRepository = vocabularyRepository;
        }

        public async Task<VideoLearningSessionDto?> GetSessionAsync(string youtubeId)
        {
            if (string.IsNullOrWhiteSpace(youtubeId))
            {
                return null;
            }

            var video = await _videoRepository.SearchVideo(youtubeId.Trim());
            if (video == null)
            {
                return null;
            }

            var rows = await _db.Transcripts
                .AsNoTracking()
                .Where(t => t.VideoId == video.VideoId)
                .OrderBy(t => t.StartTime)
                .ThenBy(t => t.TranscriptId)
                .ToListAsync();

            var cues = TranscriptCueMapper.MapCues(video.YoutubeId, rows);

            var links = await _videoVocabularyRepository.ListByVideoWithVocabularyAsync(video.VideoId);
            var vocabIds = links.Select(l => l.VocabularyId).Distinct().ToList();
            var vocabs = await _vocabularyRepository.GetByIdsAsync(vocabIds);
            var cards = vocabs.Select(MapVocabulary).OrderBy(c => c.Word).ToList();

            return new VideoLearningSessionDto
            {
                Video = new VideoDTO
                {
                    VideoId = video.VideoId,
                    YoutubeId = video.YoutubeId,
                    Title = video.Title,
                    Status = video.Status,
                },
                Transcripts = cues,
                LinkedVocabulary = cards,
            };
        }

        public async Task<VideoExtractResultDto?> ExtractAsync(string youtubeId, int transcriptId)
        {
            if (string.IsNullOrWhiteSpace(youtubeId))
            {
                return null;
            }

            var tr = await _db.Transcripts
                .AsNoTracking()
                .Include(t => t.Video)
                .FirstOrDefaultAsync(t => t.TranscriptId == transcriptId);

            if (tr?.Video == null || !string.Equals(tr.Video.YoutubeId, youtubeId.Trim(), StringComparison.Ordinal))
            {
                return null;
            }

            var tokens = Tokenize(tr.Sentence);
            var distinct = tokens.Select(t => t.Text).Distinct(StringComparer.Ordinal).ToList();
            var hits = await _vocabularyRepository.FindByWordsAsync(distinct);
            var map = hits.ToDictionary(v => v.Word, v => v.VocabularyId, StringComparer.Ordinal);

            var result = new List<ExtractedTokenDto>();
            foreach (var segment in tokens)
            {
                int? vid = null;
                if (map.TryGetValue(segment.Text, out var id))
                {
                    vid = id;
                }

                result.Add(new ExtractedTokenDto
                {
                    Text = segment.Text,
                    StartIndex = segment.StartIndex,
                    Length = segment.Length,
                    VocabularyId = vid,
                });
            }

            return new VideoExtractResultDto
            {
                TranscriptId = transcriptId,
                Tokens = result,
            };
        }

        public async Task<bool> LinkVocabularyAsync(VideoVocabularyLinkDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.YoutubeId))
            {
                return false;
            }

            var video = await _videoRepository.SearchVideo(dto.YoutubeId.Trim());
            if (video == null)
            {
                return false;
            }

            var vocab = await _vocabularyRepository.GetByIdAsync(dto.VocabularyId);
            if (vocab == null)
            {
                return false;
            }

            if (await _videoVocabularyRepository.ExistsAsync(video.VideoId, dto.VocabularyId))
            {
                return true;
            }

            if (dto.TranscriptId != null)
            {
                var ok = await _db.Transcripts.AsNoTracking()
                    .AnyAsync(t =>
                        t.TranscriptId == dto.TranscriptId &&
                        t.VideoId == video.VideoId);
                if (!ok)
                {
                    return false;
                }
            }

            var maxSort = await _db.VideoVocabularies
                .Where(x => x.VideoId == video.VideoId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync() ?? 0;

            await _videoVocabularyRepository.AddAsync(new VideoVocabulary
            {
                VideoId = video.VideoId,
                VocabularyId = dto.VocabularyId,
                TranscriptId = dto.TranscriptId,
                ContextSnippet = dto.ContextSnippet,
                SortOrder = maxSort + 1,
            });

            await _videoVocabularyRepository.SaveChangesAsync();
            return true;
        }

        private static List<(string Text, int StartIndex, int Length)> Tokenize(string sentence)
        {
            var list = new List<(string, int, int)>();
            if (string.IsNullOrWhiteSpace(sentence))
            {
                return list;
            }

            foreach (Match m in Regex.Matches(
                         sentence,
                         @"[^\s,.!?;:;""'()\[\]–—]+",
                         RegexOptions.CultureInvariant))
            {
                list.Add((m.Value, m.Index, m.Length));
            }

            return list;
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
    }
}
