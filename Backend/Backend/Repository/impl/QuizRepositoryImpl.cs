using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;


namespace Backend.Repository.impl
{
    public class QuizRepositoryImpl : QuizRepository
    {
        private readonly AppDbContext _context;

        public QuizRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        // UserQuiz
        /*
         * Lấy quiz theo UnitId
         * 
         * thuphuong21072004
         */
        public async Task<Quiz> GetQuizByUnit(int UnitId)
        {
            return await _context.Quizzes
    .FirstOrDefaultAsync(q => q.UnitId == UnitId);
        }
        /*
         * lấy đáp án đúng của câu hỏi
         * 
         * thuphuong21072004
         */
        public async Task<Answer> GetCorrectAnswer(int questionId)
        {
            return await _context.Answers
                .Where(a => a.QuestionId == questionId && a.IsCorrect == true)
                .FirstOrDefaultAsync();
        }
        /*
         * lấy Unit tương ứng với quiz
         * 
         * thuphuong21072004
         */
        public async Task<Unit> GetUnitByQuizId(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return null;

            return await _context.Units.FindAsync(quiz.UnitId);
        }

        /*
         * lưu thay đổi vào database
         * 
         * thuphuong21072004
         */
        public async Task SaveUserQuiz(UserQuiz userQuiz)
        {
            var exist = await _context.UserQuiz
                .FirstOrDefaultAsync(x => x.UserId == userQuiz.UserId && x.QuizId == userQuiz.QuizId);

            if (exist == null)
            {
                await _context.UserQuiz.AddAsync(userQuiz);
            }
            else
            {
                exist.Score = userQuiz.Score;
                exist.CompletedDate = DateTime.Now;
                exist.IsPassed = userQuiz.IsPassed;

                _context.UserQuiz.Update(exist);
            }
        }

        /*
         * lấy điểm bài kiểm tra
         * 
         * thuphuong21072004
         */
        public async Task<UserQuiz?> GetUserQuiz(int userId, int quizId)
        {
            return await _context.UserQuiz
                .FirstOrDefaultAsync(x => x.UserId == userId && x.QuizId == quizId);
        }
        // Quiz
        /*
         * Thêm quiz mới
         * 
         * thuphuong21072004
         */
        public async Task AddQuiz(Quiz quiz)
        {
            await _context.Quizzes.AddAsync(quiz);
        }
        /*
         * Cập nhật thông tin quiz
         * 
         * thuphuong21072004
         */
        public async Task UpdateQuiz(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
        }
        /*
         * xóa quiz và toàn bộ question, answer liên quan
         * 
         * thuphuong21072004
         */
        public async Task DeleteQuiz(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }
        /*
        * Lấy quiz theo quizId
        * 
        * thuphuong21072004
        */
        public async Task<Quiz> GetQuizById(int quizId)
        {
            return await _context.Quizzes
                .FirstOrDefaultAsync(q => q.QuizId == quizId );
        }

        // QUESTION
        public async Task<Question> GetQuestionById(int questionId)
        {
            return await _context.Questions
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);
        }
        /*
        * Lấy danh sách câu hỏi theo quizId
        * 
        * thuphuong21072004
        */
        public async Task<List<Question>> GetAllQuestions(int quizId)
        {
            return await _context.Questions
                .Where(x => x.QuizId == quizId)
                .ToListAsync();
        }
        /*
         * thêm câu hỏi
         * 
         * thuphuong21072004
         */
        public async Task AddQuestion(Question question)
        {
            await _context.Questions.AddAsync(question);
        }
        /*
         * cập nhật thông tin câu hỏi
         * 
         * thuphuong21072004
         */
        public async Task UpdateQuestion(Question question)
        {
            _context.Questions.Update(question);
        }
        /*
         * xóa danh sách câu hỏi và các câu trả lời liên quan
         * 
         * thuphuong21072004
         */
        public async Task DeleteQuestions(List<int> ids)
        {
            
            var answers = await _context.Answers
                .Where(a => ids.Contains(a.QuestionId))
                .ToListAsync();

            _context.Answers.RemoveRange(answers);

            var questions = await _context.Questions
                .Where(q => ids.Contains(q.QuestionId))
                .ToListAsync();

            _context.Questions.RemoveRange(questions);
        }

        public async Task<Question> GetQuestionByIdAndQuiz(int questionId, int quizId)
        {
            return await _context.Questions
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.QuizId == quizId);
        }
        // ANSWER
        /*
         * Lấy danh sách câu trả lời theo questionId
         * 
         * thuphuong21072004
         */
        public async Task<List<Answer>> GetAllAnswers(int questionId)
        {
            return await _context.Answers
                .Where(x => x.QuestionId == questionId)
                .ToListAsync();
        }
        /*
        * Lấy thông tin câu trả lời theo questionId
        * 
        * thuphuong21072004
        */
        public async Task<Answer> GetAnswerById(int answerId)
        {
            return await _context.Answers
                .FirstOrDefaultAsync(a => a.AnswerId == answerId );

        }
        /*
         * thêm câu trả lời
         * 
         * thuphuong21072004
         */
        public async Task AddAnswer(Answer answer)
        {
            await _context.Answers.AddAsync(answer);
        }
        /*
         * cập nhật thông tin câu trả lời
         * 
         * thuphuong21072004
         */
        public async Task UpdateAnswer(Answer answer)
        {
            _context.Answers.Update(answer);
        }
        /*
         * xóa danh sách câu trả lời
         * 
         * thuphuong21072004
         */
        public async Task DeleteAnswers(List<int> ids)
        {
            var answers = await _context.Answers
                .Where(a => ids.Contains(a.AnswerId))
                .ToListAsync();

            _context.Answers.RemoveRange(answers);
        }
        public async Task<Answer> GetAnswerByIdAndQuestion(int answerId, int questionId)
        {
            return await _context.Answers
                .FirstOrDefaultAsync(x => x.AnswerId == answerId && x.QuestionId == questionId);
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
    }
}
