using Backend.Common;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/learning")]
    [ApiController]
    public class LevelController : ControllerBase
    {
        private readonly LearningService _learningService;

        public LevelController(LearningService learningService)
        {
            _learningService = learningService;

        }

        // level
        /*
         * lấy danh sách level
         * 
         * thuphuong21072004
         */
        [HttpGet("listLevels")]
        public async Task<IActionResult> GetLevels()
        {
            return Ok(await _learningService.GetLevels());
        }
        /*
         * lưu level mới
         * 
         * thuphuong21072004
         */
        [HttpPost("saveLevel")]
        public async Task<IActionResult> SaveLevel(List<LevelDTO> list)
        {
            await _learningService.SaveLevels(list);
            return Ok("Save success");
        }
        // course
        /*
         * lấy danh sách course theo level
         * 
         * thuphuong21072004
         */
        [HttpGet("listCourses")]
        public async Task<IActionResult> GetCourses(int levelId)
        {
            var result = await _learningService.GetCourses(levelId);
            return Ok(result);
        }
        /*
         * lưu course mới
         * 
         * thuphuong21072004
         */
        [HttpPost("saveCourse")]
        public async Task<IActionResult> SaveCourse([FromBody] List<CourseDTO> list)
        {
            await _learningService.SaveCourses(list);
            return Ok("Save success");
        }

        //lesson
        /*
         * lấy danh sách lesson theo course
         * 
         * thuphuong21072004
         */
        [HttpGet("listLessons")]
        public async Task<IActionResult> GetLessons(int courseId)
        {
            var result = await _learningService.GetLessons(courseId);
            return Ok(result);
        }
        /*
         * lưu lesson mới
         * 
         * thuphuong21072004
         */
        [HttpPost("saveLesson")]
        public async Task<IActionResult> SaveLesson([FromBody] List<LessonDTO> list)
        {
            await _learningService.SaveLessons(list);
            return Ok("Save lesson success");
        }

        // User Quiz//
        /*
         * hiển thị bài kiểm tra quiz theo lesson
         * 
         * thuphuong21072004
         */
        [HttpGet("allQuiz")]
        public async Task<IActionResult> GetQuiz(int lessonId)
        {
            return Ok(await _learningService.GetQuizByLesson(lessonId));
        }
        /*
         * save toàn bộ bài kiểm tra
         * 
         * thuphuong21072004
         */
        [HttpPost("quiz/full")]
        public async Task<IActionResult> AddFullQuiz(QuizDTO dto)
        {
            await _learningService.SaveQuiz(dto);
            return Ok("Add full quiz success");
        }
        /*
         * xóa quiz
         * 
         * thuphuong21072004
         */
        [HttpDelete("deleteQuiz")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            await _learningService.DeleteQuiz(id);
            return Ok("Delete success");
        }
        /*
         * nộp bài quiz và chấm điểm
         * 
         * thuphuong21072004
         */
        [HttpPost("submitQuiz")]
        public async Task<IActionResult> SubmitQuiz(SubmitQuizDTO dto)
        {
            await _learningService.SubmitQuiz(dto.QuizId, dto.AnswerIds);
            return Ok("Submit quiz success");
        }

        // user progress
        /*
         * lấy danh sách tất cả khóa học, bài học đã mở khóa
         * 
         * thuphuong21072004
         */
        [HttpGet("learning-path")]
        public async Task<IActionResult> GetLearningPath()
        {
            return Ok(await _learningService.GetAllLearningPath());
        }
        /*
         * lấy tiến độ học tập của user
         * 
         * thuphuong21072004
         */
        [HttpGet("my-progress")]
        public async Task<IActionResult> GetMyProgress()
        {
            return Ok(await _learningService.GetMyProgress());
        }
        
    }
}