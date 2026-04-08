using Backend.Models;

namespace Backend.Repository
{
    public interface CourseRepository
    {
        
        Task<Course> GetById(int id);
        Task Add(Course course);
        Task Update(Course course);
        Task DeleteCourses(List<int> ids);
        Task<List<UserCourse>> GetUserCourses(int userId, int levelId);
        Task Save();
        Task<List<Course>> GetAllCourses();
        Task<int> GetMaxOrderIndex(int levelId);

        Task<List<Course>> GetCourses(int levelId);
        Task<Course?> GetCourseById(int courseId);
    }
}
