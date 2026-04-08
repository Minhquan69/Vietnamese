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
        public async Task<List<Course>> GetCourses(int levelId)
        {
            return await _context.Courses
                .Where(c => c.LevelId == levelId && c.IsActive == true)
                .OrderBy(c => c.OrderIndex)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseById(int courseId)
        {
            return await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }
        /*
         * lay course theo id
         * 
         * thuphuong21072004
         */
        public async Task<Course> GetById(int id)
        {
            return await _context.Courses.FindAsync(id);
        }
        /*
         * them course moi
         * 
         * thuphuong21072004
         */
        public async Task Add(Course course)
        {
            await _context.Courses.AddAsync(course);
        }
        /*
         * sua thong tin course
         * 
         * thuphuong21072004
         */
        public async Task Update(Course course)
        {
            _context.Courses.Update(course);
        }
        /*
         * xoa course
         * 
         * thuphuong21072004
         */
        public async Task DeleteCourses(List<int> ids)
        {
            
            var lessons = await _context.Lessons
                .Where(l => ids.Contains(l.CourseId))
                .ToListAsync();

            var lessonIds = lessons.Select(l => l.LessonId).ToList();

            if (lessonIds.Any())
            {
               
                var quizzes = await _context.Quizzes
                    .Where(q => lessonIds.Contains(q.LessonId))
                    .ToListAsync();

                var quizIds = quizzes.Select(q => q.QuizId).ToList();

                if (quizIds.Any())
                {
                    
                    var questions = await _context.Questions
                        .Where(q => quizIds.Contains(q.QuizId))
                        .ToListAsync();

                    var questionIds = questions.Select(q => q.QuestionId).ToList();

                    if (questionIds.Any())
                    {
                        var answers = await _context.Answers
                            .Where(a => questionIds.Contains(a.QuestionId))
                            .ToListAsync();

                        _context.Answers.RemoveRange(answers);
                    }
                    _context.Questions.RemoveRange(questions);
                    _context.Quizzes.RemoveRange(quizzes);
                }
                _context.Lessons.RemoveRange(lessons);
            }
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
        public async Task Save()
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
         * mở khóa khóa học đầu tiên cho user khi họ bắt đầu một cấp độ mới
         * 
         * thuphuong21072004
         */
        public async Task UnlockFirstCourse(int userId, int levelId)
        {
            var firstCourse = await _context.Courses
                .Where(c => c.LevelId == levelId)
                .OrderBy(c => c.OrderIndex)
                .FirstOrDefaultAsync();

            if (firstCourse != null)
            {
                var exist = await _context.UserCourse
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == firstCourse.CourseId);

                if (exist == null)
                {
                    _context.UserCourse.Add(new UserCourse
                    {
                        UserId = userId,
                        CourseId = firstCourse.CourseId,
                        Status = false,
                        AssignedDate = DateTime.Now
                    });
                }
            }
        }

        public async Task<List<Course>> GetAllCourses()
        {
            return await _context.Courses
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();
        }
        public async Task<int> GetMaxOrderIndex(int levelId)
        {
            var maxOrder = await _context.Courses
                .Where(x => x.LevelId == levelId)
                .MaxAsync(x => (int?)x.OrderIndex);

            return maxOrder ?? 0;
        }
    }
}