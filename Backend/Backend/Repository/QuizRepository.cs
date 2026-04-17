using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository
{
    public interface QuizRepository
    {
        // UserQuiz
        Task<Quiz> GetQuizByUnit(int UnitId);
        Task<Answer> GetCorrectAnswer(int questionId);
        Task<Unit> GetUnitByQuizId(int quizId);
        
        Task SaveUserQuiz(UserQuiz userQuiz);

        Task<UserQuiz?> GetUserQuiz(int userId, int quizId);
        // Quiz

        Task AddQuiz(Quiz quiz);
        Task UpdateQuiz(Quiz quiz);
        Task DeleteQuiz(int quizId);
        Task<Quiz> GetQuizById(int quizId);
        // Question
        Task<Question> GetQuestionById(int questionId);
        Task<List<Question>> GetAllQuestions(int quizId);
        Task AddQuestion(Question question);
        Task UpdateQuestion(Question question);
        Task DeleteQuestions(List<int> ids);
        Task<Question> GetQuestionByIdAndQuiz(int questionId, int quizId);
        // Answer
        Task<List<Answer>> GetAllAnswers(int questionId);
        Task<Answer> GetAnswerById(int answerId);
        Task AddAnswer(Answer answer);
        Task UpdateAnswer(Answer answer);
        Task DeleteAnswers(List<int> ids);
        Task<Answer> GetAnswerByIdAndQuestion(int answerId, int questionId);

        Task Save();
    }
}
