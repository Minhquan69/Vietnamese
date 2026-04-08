using Backend.Models;

namespace Backend.Repository
{
    public interface LessonRepository
    {
        
        Task<Lesson> GetById(int lessonId);
        Task Add(Lesson lesson);
        Task Update(Lesson lesson);
        Task DeleteLessons(List<int> ids);
        Task Save();
        Task<List<UserProgress>> GetUserProgress(int userId, int courseId);
        Task CompleteLesson(int userId, int lessonId);
        Task<List<Lesson>> GetAllLessons();

        Task<int> GetMaxOrderIndex(int courseId);

        Task<List<Lesson>> GetLessons(int courseId);
        Task<Lesson?> GetLessonById(int lessonId);

    }
}