using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class LessonRepositoryImpl : LessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lesson>> GetActiveByUnitOrderedAsync(int unitId)
        {
            return await _context.Lessons
                .AsNoTracking()
                .Where(l => l.UnitId == unitId && l.IsActive)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task<Lesson?> GetByIdWithUnitCourseAsync(int lessonId)
        {
            return await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Unit!)
                    .ThenInclude(u => u.Course!)
                        .ThenInclude(c => c.Level)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.IsActive);
        }

        public async Task<int> CountActiveByCourseAsync(int courseId)
        {
            return await _context.Lessons
                .AsNoTracking()
                .Where(l => l.IsActive && l.Unit!.CourseId == courseId)
                .CountAsync();
        }

        public async Task<Dictionary<int, int>> CountActiveByUnitsAsync(IEnumerable<int> unitIds)
        {
            var ids = unitIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, int>();
            }

            return await _context.Lessons
                .AsNoTracking()
                .Where(l => l.IsActive && ids.Contains(l.UnitId))
                .GroupBy(l => l.UnitId)
                .Select(g => new { UnitId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.UnitId, x => x.Cnt);
        }

        public async Task<List<int>> GetActiveLessonIdsForCourseAsync(int courseId)
        {
            return await _context.Lessons
                .AsNoTracking()
                .Where(l => l.IsActive && l.Unit!.CourseId == courseId)
                .Select(l => l.LessonId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, List<Lesson>>> GetActiveGroupedByCourseAsync(int courseId)
        {
            var lessons = await _context.Lessons
                .AsNoTracking()
                .Where(l =>
                    l.IsActive
                    && _context.Units.Any(u => u.UnitId == l.UnitId && u.CourseId == courseId))
                .OrderBy(l => l.UnitId)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            return lessons
                .GroupBy(l => l.UnitId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
