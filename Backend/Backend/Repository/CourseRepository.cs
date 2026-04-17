using Backend.Models;

namespace Backend.Repository
{
    public interface CourseRepository
    {
        Task<List<Course>> GetAllCourses(int levelId, bool? isActive);
        Task<Course> GetCourseById(int id);
        Task AddCourse(Course course);
        Task UpdateCourse(Course course);
        Task DeleteCourses(List<int> ids);
        Task SaveCourse();
        Task<List<UserCourse>> GetUserCourses(int userId, int levelId);
        Task<int> GetMaxOrderIndex(int levelId);
    }
}
