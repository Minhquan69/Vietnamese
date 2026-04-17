using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class CourseRepositoryImpl : CourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }
        /*
         * lay danh sach course theo level
         * 
         * thuphuong21072004
         */
        public async Task<List<Course>> GetAllCourses(int levelId, bool? isActive)
        {
            var query = _context.Courses
                .Where(c => c.LevelId == levelId)
                .AsQueryable();
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(c => c.OrderIndex)
                .ToListAsync();
        }
        /*
         * lay course theo id
         * 
         * thuphuong21072004
         */
        public async Task<Course> GetCourseById(int id)
        {
            return await _context.Courses.FindAsync(id);
        }
        /*
         * them course moi
         * 
         * thuphuong21072004
         */
        public async Task AddCourse(Course course)
        {
            await _context.Courses.AddAsync(course);
        }
        /*
         * sua thong tin course
         * 
         * thuphuong21072004
         */
        public async Task UpdateCourse(Course course)
        {
            _context.Courses.Update(course);
        }
        /*
         * xóa course và toàn bộ dữ liệu liên quan
         * 
         * thuphuong21072004
         */
        public async Task DeleteCourses(List<int> ids)
        {
            var courses = await _context.Courses
                .Where(c => ids.Contains(c.CourseId))
                .ToListAsync();

            _context.Courses.RemoveRange(courses);
        }
        /*
        * lưu thay đổi vào database
        * 
        * thuphuong21072004
        */
        public async Task SaveCourse()
        {
            await _context.SaveChangesAsync();
        }
        /*
         * lấy trạng thái khóa/mở của user theo course
         * 
         * thuphuong21072004
         */
        public async Task<List<UserCourse>> GetUserCourses(int userId, int levelId)
        {
            return await _context.UserCourse
                .Where(uc => uc.UserId == userId &&
                             _context.Courses
                                .Where(c => c.LevelId == levelId)
                                .Select(c => c.CourseId)
                                .Contains(uc.CourseId))
                .ToListAsync();
        }
        /*
         * lấy order max
         * 
         * thuphuong21072004
         */
        public async Task<int> GetMaxOrderIndex(int levelId)
        {
            var maxOrder = await _context.Courses
                .Where(x => x.LevelId == levelId)
                .MaxAsync(x => (int?)x.OrderIndex);

            return maxOrder ?? 0;
        }
    }
}