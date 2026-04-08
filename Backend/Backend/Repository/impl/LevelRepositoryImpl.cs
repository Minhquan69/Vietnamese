using Backend.Data;
using Backend.dto;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class LevelRepositoryImpl : LevelRepository
    {
        private readonly AppDbContext _context;
        public LevelRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }
        /*
         * lấy tất cả các cấp độ
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task<List<Level>> GetLevels()
        {
            return await _context.Levels
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();
        }
        /*
         * lấy cấp độ theo id
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task<Level> GetLevelById(int id)
        {
            return await _context.Levels.FindAsync(id);
        }
        /*
         * thêm cấp độ mới
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task AddLevel(Level level)
        {
            await _context.Levels.AddAsync(level);
        }
        /*
         * sửa thông tin cấp độ
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task UpdateLevel(Level level)
        {
            _context.Levels.Update(level);
        }
        /*
         * xóa tin cấp độ
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task DeleteLevels(List<int> ids)
        {

            var courses = await _context.Courses
                .Where(c => ids.Contains(c.LevelId))
                .ToListAsync();

            var courseIds = courses.Select(c => c.CourseId).ToList();

            var lessons = await _context.Lessons
                .Where(l => courseIds.Contains(l.CourseId))
                .ToListAsync();

            var lessonIds = lessons.Select(l => l.LessonId).ToList();

            var quizzes = await _context.Quizzes
                .Where(q => lessonIds.Contains(q.LessonId))
                .ToListAsync();

            var quizIds = quizzes.Select(q => q.QuizId).ToList();

            var questions = await _context.Questions
                .Where(q => quizIds.Contains(q.QuizId))
                .ToListAsync();

            var questionIds = questions.Select(q => q.QuestionId).ToList();

            var answers = await _context.Answers
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();
            _context.Answers.RemoveRange(answers);
            _context.Questions.RemoveRange(questions);
            _context.Quizzes.RemoveRange(quizzes);
            _context.Lessons.RemoveRange(lessons);
            _context.Courses.RemoveRange(courses);

            var levels = await _context.Levels
                .Where(l => ids.Contains(l.LevelId))
                .ToListAsync();

            _context.Levels.RemoveRange(levels);
        }
        /*
         * lưu thay đổi vào database
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
        /*
         * lấy order index lớn nhất của cấp độ
         * 
         * thuphuong21072004
         */
        public async Task<int> GetMaxOrderIndex()
        {
            var max = await _context.Levels
                .Select(x => (int?)x.OrderIndex)
                .MaxAsync();

            return max ?? 0;
        }
    }
}
