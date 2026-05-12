using Backend.dto;

namespace Backend.Services
{
    public interface LessonLearningService
    {
        Task<List<CourseCatalogItemDto>> GetCourseCatalogAsync(int levelId);

        Task<CourseLearnDetailDto?> GetCourseLearnDetailAsync(int courseId);

        Task<UnitOutlineDto?> GetUnitOutlineAsync(int unitId);

        Task<LessonPlayerDto?> GetLessonPlayerAsync(int lessonId);

        Task<LessonCompleteResultDto?> CompleteLessonAsync(int lessonId);
    }
}
