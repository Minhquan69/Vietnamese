using Backend.dto;

namespace Backend.Services
{
    public interface LearningService
    {
        Task<List<LevelDTO>> GetLevels();
        Task SaveLevels(List<LevelDTO> list);

        Task<List<CourseDTO>> GetCourses(int levelId);
        Task SaveCourses(List<CourseDTO> list);

        Task<List<LessonDTO>> GetLessons(int courseId);
        Task SaveLessons(List<LessonDTO> list);
        
        Task<List<LevelDTO>> GetMyProgress();
        Task<List<LevelDTO>> GetAllLearningPath();

        Task<QuizDTO> GetQuizByLesson(int lessonId);
        Task UnlockNextLevel(int lessonId);
        Task UnlockNextCourse(int lessonId);
        Task UnlockNextLesson(int lessonId);
        Task SubmitQuiz(int quizId, List<int> answerIds);
        Task SaveQuiz(QuizDTO dto);
        Task DeleteQuiz(int quizId);

    }
}
