using Backend.common;
using Backend.Common;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using Backend.Services;
using UserProgressEntity = Backend.Models.UserProgress;

namespace Backend.Services.impl
{
    public class LessonLearningServiceImpl : LessonLearningService
    {
        private readonly LessonRepository _lessonRepository;
        private readonly ProgressRepository _progressRepository;
        private readonly CourseRepository _courseRepository;
        private readonly UnitRepository _unitRepository;
        private readonly LevelRepository _levelRepository;
        private readonly UserContextUtil _userContext;
        private readonly GamificationService _gamification;

        public LessonLearningServiceImpl(
            LessonRepository lessonRepository,
            ProgressRepository progressRepository,
            CourseRepository courseRepository,
            UnitRepository unitRepository,
            LevelRepository levelRepository,
            UserContextUtil userContext,
            GamificationService gamification)
        {
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _levelRepository = levelRepository;
            _userContext = userContext;
            _gamification = gamification;
        }

        public async Task<List<CourseCatalogItemDto>> GetCourseCatalogAsync(int levelId)
        {
            var userId = _userContext.GetUserId();
            var level = await _levelRepository.GetLevelById(levelId);
            if (level == null || !level.IsActive)
            {
                return new List<CourseCatalogItemDto>();
            }

            var courses = await _courseRepository.GetAllCourses(levelId, true);
            var list = new List<CourseCatalogItemDto>();

            foreach (var course in courses.OrderBy(c => c.OrderIndex))
            {
                var lessonIds = await _lessonRepository.GetActiveLessonIdsForCourseAsync(course.CourseId);
                var lessonProg = userId > 0 && lessonIds.Count > 0
                    ? await _progressRepository.GetUserLessonProgressAsync(userId, lessonIds)
                    : new Dictionary<int, UserProgressEntity>();

                var completed = lessonIds.Count(id =>
                    lessonProg.TryGetValue(id, out var p) && p.Status);
                var total = lessonIds.Count;
                var pct = total == 0 ? 0 : (int)Math.Round(100.0 * completed / total);

                var units = await _unitRepository.GetAllUnits(course.CourseId, true);

                list.Add(
                    new CourseCatalogItemDto
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        Description = course.Description,
                        LevelId = level.LevelId,
                        LevelName = level.LevelName,
                        UnitCount = units.Count,
                        LessonTotal = total,
                        LessonsCompleted = completed,
                        ProgressPercent = pct,
                    });
            }

            return list;
        }

        public async Task<CourseLearnDetailDto?> GetCourseLearnDetailAsync(int courseId)
        {
            var course = await _courseRepository.GetCourseById(courseId);
            if (course == null || !course.IsActive)
            {
                return null;
            }

            var level = await _levelRepository.GetLevelById(course.LevelId);
            var levelName = level?.LevelName ?? string.Empty;

            var units = await _unitRepository.GetAllUnits(courseId, true);
            var orderedUnits = units.OrderBy(u => u.OrderIndex).ToList();

            var userId = _userContext.GetUserId();
            var groupedLessons = await _lessonRepository.GetActiveGroupedByCourseAsync(courseId);
            var allLessonIds = groupedLessons.Values.SelectMany(v => v.Select(l => l.LessonId)).ToList();

            var lessonProg = userId > 0 && allLessonIds.Count > 0
                ? await _progressRepository.GetUserLessonProgressAsync(userId, allLessonIds)
                : new Dictionary<int, UserProgressEntity>();

            var unitProgList = userId > 0
                ? await _progressRepository.GetUserUnits(userId, courseId, Constant.RefType.Unit)
                : new List<UserProgressEntity>();
            var unitProgDict = unitProgList.ToDictionary(x => x.RefId, x => x);

            var unitSummaries = new List<UnitLearnSummaryDto>();
            for (var i = 0; i < orderedUnits.Count; i++)
            {
                var u = orderedUnits[i];
                groupedLessons.TryGetValue(u.UnitId, out var lessonsForUnit);
                lessonsForUnit ??= new List<Lesson>();

                var lt = lessonsForUnit.Count;
                var done = lessonsForUnit.Count(l =>
                    lessonProg.TryGetValue(l.LessonId, out var p) && p.Status);
                var pct = lt == 0 ? 0 : (int)Math.Round(100.0 * done / lt);

                unitSummaries.Add(
                    new UnitLearnSummaryDto
                    {
                        UnitId = u.UnitId,
                        UnitName = u.UnitName,
                        Objective = u.Objective,
                        OrderIndex = u.OrderIndex,
                        LessonTotal = lt,
                        LessonsCompleted = done,
                        ProgressPercent = pct,
                        UnitUnlocked = IsUnitUnlocked(orderedUnits, i, unitProgDict),
                        UnitPathComplete = unitProgDict.TryGetValue(u.UnitId, out var up)
                            ? up.Status
                            : null,
                    });
            }

            var totalLessons = allLessonIds.Count;
            var completedLessons = allLessonIds.Count(id =>
                lessonProg.TryGetValue(id, out var p) && p.Status);
            var coursePct = totalLessons == 0
                ? 0
                : (int)Math.Round(100.0 * completedLessons / totalLessons);

            int? continueUnitId = null;
            int? continueLessonId = null;
            foreach (var u in orderedUnits)
            {
                if (!groupedLessons.TryGetValue(u.UnitId, out var ls) || ls.Count == 0)
                {
                    continue;
                }

                foreach (var les in ls.OrderBy(x => x.OrderIndex))
                {
                    if (!lessonProg.TryGetValue(les.LessonId, out var p) || !p.Status)
                    {
                        continueUnitId = u.UnitId;
                        continueLessonId = les.LessonId;
                        goto FoundContinue;
                    }
                }
            }

        FoundContinue:

            return new CourseLearnDetailDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                LevelId = course.LevelId,
                LevelName = levelName,
                LessonTotal = totalLessons,
                LessonsCompleted = completedLessons,
                ProgressPercent = coursePct,
                Units = unitSummaries,
                ContinueUnitId = continueUnitId,
                ContinueLessonId = continueLessonId,
            };
        }

        public async Task<UnitOutlineDto?> GetUnitOutlineAsync(int unitId)
        {
            var unit = await _unitRepository.GetUnitById(unitId);
            if (unit == null || !unit.IsActive)
            {
                return null;
            }

            var course = await _courseRepository.GetCourseById(unit.CourseId);
            if (course == null)
            {
                return null;
            }

            var level = await _levelRepository.GetLevelById(course.LevelId);

            var lessons = await _lessonRepository.GetActiveByUnitOrderedAsync(unitId);
            var userId = _userContext.GetUserId();
            var lessonIds = lessons.Select(l => l.LessonId).ToList();
            var prog = userId > 0 && lessonIds.Count > 0
                ? await _progressRepository.GetUserLessonProgressAsync(userId, lessonIds)
                : new Dictionary<int, UserProgressEntity>();

            var outlineLessons = lessons
                .Select(
                    l => new LessonOutlineDto
                    {
                        LessonId = l.LessonId,
                        LessonType = l.LessonType,
                        Title = l.Title,
                        Summary = l.Summary,
                        OrderIndex = l.OrderIndex,
                        DurationMinutes = l.DurationMinutes,
                        Completed = prog.TryGetValue(l.LessonId, out var p) && p.Status,
                    })
                .ToList();

            var total = lessons.Count;
            var done = outlineLessons.Count(x => x.Completed);
            var pct = total == 0 ? 0 : (int)Math.Round(100.0 * done / total);

            int? continueId = null;
            foreach (var l in lessons)
            {
                if (!prog.TryGetValue(l.LessonId, out var p) || !p.Status)
                {
                    continueId = l.LessonId;
                    break;
                }
            }

            return new UnitOutlineDto
            {
                UnitId = unit.UnitId,
                UnitName = unit.UnitName,
                Description = unit.Description,
                Objective = unit.Objective,
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                LevelId = course.LevelId,
                LevelName = level?.LevelName ?? string.Empty,
                VideoUrl = unit.VideoUrl,
                LessonTotal = total,
                LessonsCompleted = done,
                ProgressPercent = pct,
                ContinueLessonId = continueId,
                Lessons = outlineLessons,
                QuizUnitId = unit.UnitId,
            };
        }

        public async Task<LessonPlayerDto?> GetLessonPlayerAsync(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdWithUnitCourseAsync(lessonId);
            if (lesson == null)
            {
                return null;
            }

            var unit = lesson.Unit!;
            var course = unit.Course!;
            var level = course.Level;

            var ordered = await _lessonRepository.GetActiveByUnitOrderedAsync(unit.UnitId);
            var idx = ordered.FindIndex(l => l.LessonId == lessonId);
            int? prev = idx > 0 ? ordered[idx - 1].LessonId : null;
            int? next = idx >= 0 && idx < ordered.Count - 1 ? ordered[idx + 1].LessonId : null;

            var userId = _userContext.GetUserId();
            var prog = userId > 0
                ? await _progressRepository.GetUserLessonProgressAsync(userId, new List<int> { lessonId })
                : new Dictionary<int, UserProgressEntity>();
            var completed = prog.TryGetValue(lessonId, out var row) && row.Status;

            string? videoUrl = null;
            if (lesson.LessonType.Equals("video", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(unit.VideoUrl))
            {
                videoUrl = unit.VideoUrl;
            }

            return new LessonPlayerDto
            {
                LessonId = lesson.LessonId,
                LessonType = lesson.LessonType,
                Title = lesson.Title,
                Summary = lesson.Summary,
                OrderIndex = lesson.OrderIndex,
                DurationMinutes = lesson.DurationMinutes,
                ContentJson = lesson.ContentJson,
                Completed = completed,
                UnitId = unit.UnitId,
                UnitName = unit.UnitName,
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                LevelId = course.LevelId,
                LevelName = level?.LevelName ?? string.Empty,
                VideoUrl = videoUrl,
                PreviousLessonId = prev,
                NextLessonId = next,
                QuizUnitId = unit.UnitId,
            };
        }

        public async Task<LessonCompleteResultDto?> CompleteLessonAsync(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdWithUnitCourseAsync(lessonId);
            if (lesson == null)
            {
                return null;
            }

            var userId = _userContext.GetUserId();
            if (userId <= 0)
            {
                return null;
            }

            var newlyCompleted = await _progressRepository.CompleteLessonProgressAsync(userId, lessonId);
            if (newlyCompleted)
            {
                try
                {
                    await _gamification.RecordLessonCompletedAsync(userId, lessonId);
                }
                catch
                {
                    /* gamification must not block learning */
                }
            }

            var ordered = await _lessonRepository.GetActiveByUnitOrderedAsync(lesson.UnitId);
            var lessonIds = ordered.Select(l => l.LessonId).ToList();
            var prog = await _progressRepository.GetUserLessonProgressAsync(userId, lessonIds);
            var done = lessonIds.Count(id => prog.TryGetValue(id, out var p) && p.Status);
            var pct = lessonIds.Count == 0
                ? 100
                : (int)Math.Round(100.0 * done / lessonIds.Count);

            int? nextId = null;
            var idx = ordered.FindIndex(l => l.LessonId == lessonId);
            if (idx >= 0 && idx < ordered.Count - 1)
            {
                nextId = ordered[idx + 1].LessonId;
            }

            return new LessonCompleteResultDto
            {
                LessonId = lessonId,
                Completed = true,
                UnitProgressPercent = pct,
                NextLessonId = nextId,
            };
        }

        private static bool IsUnitUnlocked(
            List<Unit> orderedUnits,
            int index,
            Dictionary<int, UserProgressEntity> unitProg)
        {
            var u = orderedUnits[index];
            if (unitProg.ContainsKey(u.UnitId))
            {
                return true;
            }

            if (index == 0)
            {
                return false;
            }

            var prev = orderedUnits[index - 1];
            return unitProg.TryGetValue(prev.UnitId, out var p) && p.Status;
        }
    }
}
