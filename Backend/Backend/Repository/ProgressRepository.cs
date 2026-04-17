using Backend.Models;

namespace Backend.Repository
{
    public interface ProgressRepository
    {

        Task<UserLevel?> GetUserLevel(int userId, int levelId);
        Task<UserCourse?> GetUserCourseByCourseId(int userId, int courseId);
        Task<List<UserProgress>> GetUserUnits(int userId, int courseId);
        Task<UserLevel?> GetCurrentLevel(int userId);
        Task<UserCourse?> GetCurrentCourse(int userId);
        Task AddUserLevel(UserLevel userLevel);
        Task AddUserCourse(UserCourse userCourse);
        Task AddUserProgress(UserProgress userProgress);
        Task Save();

        Task<List<UserCourse>> GetUserCourses(int userId, int levelId);
        Task<UserProgress?> GetUserUnitByUnitId(int userId, int UnitId);

        Task<bool> HasUserLevel(int userId);
        Task<bool> HasUserCourse(int userId);
        Task<bool> HasUserUnit(int userId);

    }
}
