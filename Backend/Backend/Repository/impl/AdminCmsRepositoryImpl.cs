using Backend.Data;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class AdminCmsRepositoryImpl : AdminCmsRepository
    {
        private const int MaxPageSize = 100;

        private readonly AppDbContext _db;

        public AdminCmsRepositoryImpl(AppDbContext db)
        {
            _db = db;
        }

        private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
        {
            var p = page < 1 ? 1 : page;
            var s = pageSize < 1 ? 20 : Math.Min(pageSize, MaxPageSize);
            return (p, s);
        }

        public async Task<(List<AdminUserRowDto> Items, int Total)> ListUsersAsync(
            string? email,
            int? status,
            int? roleId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (p, s) = NormalizePage(page, pageSize);
            var q =
                from u in _db.Users.AsNoTracking()
                join r in _db.Roles.AsNoTracking() on u.RoleId equals r.RoleId
                select new { u, r.RoleName };

            if (!string.IsNullOrWhiteSpace(email))
            {
                var term = email.Trim();
                q = q.Where(x => x.u.Email.Contains(term) || x.u.Name.Contains(term));
            }

            if (status.HasValue)
            {
                q = q.Where(x => x.u.Status == status.Value);
            }

            if (roleId.HasValue)
            {
                q = q.Where(x => x.u.RoleId == roleId.Value);
            }

            var total = await q.CountAsync(cancellationToken);
            var items = await q
                .OrderByDescending(x => x.u.UserId)
                .Skip((p - 1) * s)
                .Take(s)
                .Select(x => new AdminUserRowDto
                {
                    UserId = x.u.UserId,
                    Name = x.u.Name,
                    Email = x.u.Email,
                    RoleId = x.u.RoleId,
                    RoleName = x.RoleName,
                    Status = x.u.Status,
                    AvatarUrl = x.u.AvatarUrl,
                })
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<(List<AdminCourseRowDto> Items, int Total)> ListCoursesAsync(
            int? levelId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (p, sz) = NormalizePage(page, pageSize);
            var query =
                from c in _db.Courses.AsNoTracking()
                join l in _db.Levels.AsNoTracking() on c.LevelId equals l.LevelId
                select new { c, l.LevelName };

            if (levelId.HasValue)
            {
                query = query.Where(x => x.c.LevelId == levelId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x => x.c.CourseName.Contains(term));
            }

            if (activeOnly == true)
            {
                query = query.Where(x => x.c.IsActive);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(x => x.c.LevelId)
                .ThenBy(x => x.c.OrderIndex)
                .ThenBy(x => x.c.CourseId)
                .Skip((p - 1) * sz)
                .Take(sz)
                .Select(x => new AdminCourseRowDto
                {
                    CourseId = x.c.CourseId,
                    CourseName = x.c.CourseName,
                    LevelId = x.c.LevelId,
                    LevelName = x.LevelName,
                    OrderIndex = x.c.OrderIndex,
                    IsActive = x.c.IsActive,
                })
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<(List<AdminLessonRowDto> Items, int Total)> ListLessonsAsync(
            int? levelId,
            int? courseId,
            int? unitId,
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (p, sz) = NormalizePage(page, pageSize);
            var query =
                from lesson in _db.Lessons.AsNoTracking()
                join unit in _db.Units.AsNoTracking() on lesson.UnitId equals unit.UnitId
                join course in _db.Courses.AsNoTracking() on unit.CourseId equals course.CourseId
                join level in _db.Levels.AsNoTracking() on course.LevelId equals level.LevelId
                select new { lesson, unit, course, level.LevelName };

            if (levelId.HasValue)
            {
                query = query.Where(x => x.course.LevelId == levelId.Value);
            }

            if (courseId.HasValue)
            {
                query = query.Where(x => x.course.CourseId == courseId.Value);
            }

            if (unitId.HasValue)
            {
                query = query.Where(x => x.unit.UnitId == unitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x => x.lesson.Title.Contains(term));
            }

            if (activeOnly == true)
            {
                query = query.Where(x => x.lesson.IsActive);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(x => x.course.CourseId)
                .ThenBy(x => x.unit.OrderIndex)
                .ThenBy(x => x.lesson.OrderIndex)
                .Skip((p - 1) * sz)
                .Take(sz)
                .Select(x => new AdminLessonRowDto
                {
                    LessonId = x.lesson.LessonId,
                    UnitId = x.unit.UnitId,
                    UnitName = x.unit.UnitName,
                    CourseId = x.course.CourseId,
                    CourseName = x.course.CourseName,
                    LevelName = x.LevelName,
                    Title = x.lesson.Title,
                    LessonType = x.lesson.LessonType,
                    OrderIndex = x.lesson.OrderIndex,
                    IsActive = x.lesson.IsActive,
                })
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<(List<AdminVocabularyRowDto> Items, int Total)> ListVocabulariesAsync(
            string? q,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (p, sz) = NormalizePage(page, pageSize);
            var query = _db.Vocabularies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(v => v.Word.Contains(term) || (v.MeaningEn != null && v.MeaningEn.Contains(term)));
            }

            if (activeOnly == true)
            {
                query = query.Where(v => v.IsActive);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(v => v.Word)
                .Skip((p - 1) * sz)
                .Take(sz)
                .Select(v => new AdminVocabularyRowDto
                {
                    VocabularyId = v.VocabularyId,
                    Word = v.Word,
                    MeaningEn = v.MeaningEn,
                    PartOfSpeech = v.PartOfSpeech,
                    IsActive = v.IsActive,
                    CreatedUtc = v.CreatedUtc,
                })
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<(List<AdminQuizRowDto> Items, int Total)> ListQuizzesAsync(
            string? q,
            string? refType,
            bool? activeOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (p, sz) = NormalizePage(page, pageSize);
            var query = _db.Quizzes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(z => z.QuizName.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(refType))
            {
                var rt = refType.Trim();
                query = query.Where(z => z.RefType == rt);
            }

            if (activeOnly == true)
            {
                query = query.Where(z => z.IsActive);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(z => z.QuizId)
                .Skip((p - 1) * sz)
                .Take(sz)
                .Select(z => new AdminQuizRowDto
                {
                    QuizId = z.QuizId,
                    QuizName = z.QuizName,
                    RefType = z.RefType,
                    RefId = z.RefId,
                    PassScore = z.PassScore,
                    TimeLimit = z.TimeLimit,
                    IsActive = z.IsActive,
                    CreatedDate = z.CreatedDate,
                })
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<AdminAnalyticsSummaryDto> GetAnalyticsSummaryAsync(CancellationToken cancellationToken = default)
        {
            var totals = new AdminAnalyticsTotalsDto
            {
                UsersTotal = await _db.Users.AsNoTracking().CountAsync(cancellationToken),
                UsersActive = await _db.Users.AsNoTracking().CountAsync(u => u.Status == 1, cancellationToken),
                CoursesTotal = await _db.Courses.AsNoTracking().CountAsync(cancellationToken),
                LessonsTotal = await _db.Lessons.AsNoTracking().CountAsync(cancellationToken),
                VocabulariesTotal = await _db.Vocabularies.AsNoTracking().CountAsync(cancellationToken),
                QuizzesTotal = await _db.Quizzes.AsNoTracking().CountAsync(cancellationToken),
            };

            var since30 = DateTime.UtcNow.AddDays(-30);
            totals.QuizAttemptsLast30Days = await _db.UserQuiz
                .AsNoTracking()
                .CountAsync(q => q.CompletedDate >= since30, cancellationToken);
            totals.SpeakingAttemptsLast30Days = await _db.SpeakingAttempts
                .AsNoTracking()
                .CountAsync(s => s.CreatedUtc >= since30, cancellationToken);

            var end = DateTime.UtcNow.Date;
            var start = end.AddDays(-13);
            var next = end.AddDays(1);

            var quizByDay = await _db.UserQuiz
                .AsNoTracking()
                .Where(q => q.IsPassed && q.CompletedDate >= start && q.CompletedDate < next)
                .GroupBy(q => q.CompletedDate.Date)
                .Select(g => new { Day = g.Key, C = g.Count() })
                .ToListAsync(cancellationToken);

            var speakByDay = await _db.SpeakingAttempts
                .AsNoTracking()
                .Where(s => s.CreatedUtc >= start && s.CreatedUtc < next)
                .GroupBy(s => s.CreatedUtc.Date)
                .Select(g => new { Day = g.Key, C = g.Count() })
                .ToListAsync(cancellationToken);

            var qMap = quizByDay.ToDictionary(x => x.Day, x => x.C);
            var sMap = speakByDay.ToDictionary(x => x.Day, x => x.C);

            var series = new List<AdminAnalyticsSeriesPointDto>();
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                series.Add(
                    new AdminAnalyticsSeriesPointDto
                    {
                        Date = d.ToString("yyyy-MM-dd"),
                        QuizCompletions = qMap.TryGetValue(d, out var qc) ? qc : 0,
                        SpeakingSessions = sMap.TryGetValue(d, out var sc) ? sc : 0,
                    });
            }

            return new AdminAnalyticsSummaryDto
            {
                Totals = totals,
                ActivityLast14Days = series,
            };
        }
    }
}
