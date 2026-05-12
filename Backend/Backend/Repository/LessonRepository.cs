using Backend.Models;

namespace Backend.Repository
{
    public interface LessonRepository
    {
        Task<List<Lesson>> GetActiveByUnitOrderedAsync(int unitId);

        Task<Lesson?> GetByIdWithUnitCourseAsync(int lessonId);

        Task<int> CountActiveByCourseAsync(int courseId);

        Task<Dictionary<int, int>> CountActiveByUnitsAsync(IEnumerable<int> unitIds);

        Task<List<int>> GetActiveLessonIdsForCourseAsync(int courseId);

        Task<Dictionary<int, List<Lesson>>> GetActiveGroupedByCourseAsync(int courseId);
    }
}
