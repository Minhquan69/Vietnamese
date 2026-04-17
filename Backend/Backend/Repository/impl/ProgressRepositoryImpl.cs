using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class ProgressRepositoryImpl : ProgressRepository
    {
        private readonly AppDbContext _context;

        public ProgressRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }
        /*
         * lấy level của user 
         * 
         * thuphuong21072004
         */
        public async Task<UserLevel?> GetUserLevel(int userId, int levelId)
        {
            return await _context.UserLevel
                .FirstOrDefaultAsync(x => x.UserId == userId && x.LevelId == levelId);
        }
        /*
         * lấy course của user theo courseId
         * 
         * thuphuong21072004
         */
        public async Task<UserCourse?> GetUserCourseByCourseId(int userId, int courseId)
        {
            return await _context.UserCourse
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId);
        }
        /*
         * lấy danh sách Unit (progress) của user theo course
         * 
         * thuphuong21072004
         */
        public async Task<List<UserProgress>> GetUserUnits(int userId, int courseId)
        {
            return await _context.UserProgress
                .Where(x => x.UserId == userId &&
                            _context.Units
                                .Where(l => l.CourseId == courseId)
                                .Select(l => l.UnitId)
                                .Contains(x.UnitId))
                .ToListAsync();
        }
        /*
         * lấy level hiện tại của user (chưa hoàn thành)
         * 
         * thuphuong21072004
         */
        public async Task<UserLevel?> GetCurrentLevel(int userId)
        {
            return await _context.UserLevel
                .Where(x => x.UserId == userId && x.Status == false)
                .OrderBy(x => x.AssignedDate)
                .FirstOrDefaultAsync();
        }
        /*
         * lấy course hiện tại của user (chưa hoàn thành)
         * 
         * thuphuong21072004
         */
        public async Task<UserCourse?> GetCurrentCourse(int userId)
        {
            return await _context.UserCourse
                .Where(x => x.UserId == userId && x.Status == false)
                .OrderBy(x => x.AssignedDate)
                .FirstOrDefaultAsync();
        }
        /*
         * thêm level cho user
         * 
         * thuphuong21072004
         */
        public async Task AddUserLevel(UserLevel userLevel)
        {
            await _context.UserLevel.AddAsync(userLevel);
        }
        /*
         * thêm course cho user
         * 
         * thuphuong21072004
         */
        public async Task AddUserCourse(UserCourse userCourse)
        {
            await _context.UserCourse.AddAsync(userCourse);
        }
        /*
         * thêm tiến trình học (Unit) cho user
         * 
         * thuphuong21072004
         */
        public async Task AddUserProgress(UserProgress userProgress)
        {
            await _context.UserProgress.AddAsync(userProgress);
        }
        /*
         * lưu thay đổi vào database
         * 
         * thuphuong21072004
         */
        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
        /*
         * lấy danh sách course của user theo level
         * 
         * thuphuong21072004
         */
        public async Task<List<UserCourse>> GetUserCourses(int userId, int levelId)
        {
            var courseIds = await _context.Courses
                .Where(c => c.LevelId == levelId)
                .Select(c => c.CourseId)
                .ToListAsync();

            return await _context.UserCourse
                .Where(x => x.UserId == userId && courseIds.Contains(x.CourseId))
                .ToListAsync();
        }
        /*
         * lấy tiến trình Unit của user theo UnitId
         * 
         * thuphuong21072004
         */
        public async Task<UserProgress?> GetUserUnitByUnitId(int userId, int UnitId)
        {
            return await _context.UserProgress
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UnitId == UnitId);
        }

        /*
         * kiểm tra user đã có level hay chưa
         * 
         * thuphuong21072004
         */
        public async Task<bool> HasUserLevel(int userId)
        {
            return await _context.UserLevel.AnyAsync(x => x.UserId == userId);
        }
        /*
         * kiểm tra user đã có course hay chưa
         * 
         * thuphuong21072004
         */
        public async Task<bool> HasUserCourse(int userId)
        {
            return await _context.UserCourse.AnyAsync(x => x.UserId == userId);
        }
        /*
         * kiểm tra user đã có Unit hay chưa
         * 
         * thuphuong21072004
         */
        public async Task<bool> HasUserUnit(int userId)
        {
            return await _context.UserProgress.AnyAsync(x => x.UserId == userId);
        }
        

    }
}
