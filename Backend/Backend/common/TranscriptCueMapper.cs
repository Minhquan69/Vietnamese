using Backend.dto;
using Backend.Models;

namespace Backend.Common
{
    public static class TranscriptCueMapper
    {
        public static List<TranscriptDTO> MapCues(string youtubeId, IEnumerable<Transcript> transcripts)
        {
            var ordered = transcripts
                .OrderBy(t => t.StartTime)
                .ThenBy(t => t.TranscriptId)
                .ToList();

            var list = new List<TranscriptDTO>(ordered.Count);

            for (var i = 0; i < ordered.Count; i++)
            {
                var t = ordered[i];
                double end = i < ordered.Count - 1
                    ? (t.EndTime ?? ordered[i + 1].StartTime)
                    : (t.EndTime ?? t.StartTime + 6);

                if (end <= t.StartTime)
                {
                    end = t.StartTime + 1.5;
                }

                list.Add(new TranscriptDTO
                {
                    TranscriptId = t.TranscriptId,
                    YoutubeId = youtubeId,
                    Sentence = t.Sentence,
                    StartTime = t.StartTime,
                    EndTime = end,
                });
            }

            return list;
        }
    }
}
