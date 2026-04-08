using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

public class LessonRepositoryImpl : LessonRepository
{
    private readonly AppDbContext _context;

    public LessonRepositoryImpl(AppDbContext context)
    {
        _context = context;
    }

    /*
     * Lấy danh sách lesson theo courseId
     * 
     * thuphuong21072004
     */
    public async Task<List<Lesson>> GetLessons(int courseId)
    {
        return await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();
    }

    public async Task<Lesson?> GetLessonById(int lessonId)
    {
        return await _context.Lessons
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);
    }
    /*
     * Lấy thông tin lesson theo lessonId
     * 
     * thuphuong21072004
     */
    public async Task<Lesson> GetById(int lessonId)
    {
        return await _context.Lessons.FindAsync(lessonId);
    }
    /*
     * Thêm lesson mới
     * 
     * thuphuong21072004
     */
    public async Task Add(Lesson lesson)
    {
        await _context.Lessons.AddAsync(lesson);
    }
    /*
     * Cập nhật thông tin lesson
     * 
     * thuphuong21072004
     */
    public async Task Update(Lesson lesson)
    {
        _context.Lessons.Update(lesson);
    }
    /*
     * Xóa lesson
     * 
     * thuphuong21072004
     */
    public async Task DeleteLessons(List<int> ids)
    {
        
        var quizzes = await _context.Quizzes
            .Where(q => ids.Contains(q.LessonId))
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

        var lessons = await _context.Lessons
            .Where(l => ids.Contains(l.LessonId))
            .ToListAsync();

        _context.Lessons.RemoveRange(lessons);
    }
    /*
     * Lưu thay đổi vào database
     * 
     * thuphuong21072004
     */
    public async Task Save()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<List<UserProgress>> GetUserProgress(int userId, int courseId)
    {
        if (courseId == 0)
        {
            // lấy tất cả progress của user
            return await _context.UserProgress
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        // lấy progress theo course
        return await _context.UserProgress
            .Include(x => x.Lesson)
            .Where(x => x.UserId == userId && x.Lesson.CourseId == courseId)
            .ToListAsync();
    }

    public async Task CompleteLesson(int userId, int lessonId)
    {
        var progress = await _context.UserProgress
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId);

        if (progress != null)
        {
            progress.Status = true;
            progress.CompletedDate = DateTime.Now;
            _context.UserProgress.Update(progress); 
        }
        else
        {
            await _context.UserProgress.AddAsync(new UserProgress
            {
                UserId = userId,
                LessonId = lessonId,
                Status = true,
                CompletedDate = DateTime.Now
            });
        }
    }

    public async Task<List<Lesson>> GetAllLessons()
    {
        return await _context.Lessons
            .OrderBy(x => x.OrderIndex)
            .ToListAsync();
    }
    public async Task<int> GetMaxOrderIndex(int courseId)
    {
        var maxOrder = await _context.Lessons
            .Where(x => x.CourseId == courseId)
            .MaxAsync(x => (int?)x.OrderIndex);

        return maxOrder ?? 0;
    }

}