using Backend.Common;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.impl
{
    public class LearningServiceImpl : LearningService
    {
        private readonly LevelRepository _levelRepository;
        private readonly CourseRepository _courseRepository;
        private readonly LessonRepository _lessonRepository;
        private readonly UserContextUtil _userContext;
        private readonly ProgressRepository _progressRepository;
        public readonly QuizRepository _quizRepository;
        private readonly IMapper _mapper;
        public LearningServiceImpl(
            LevelRepository levelRepository,
            CourseRepository courseRepository,
            LessonRepository lessonRepository,
            UserContextUtil userContext, IMapper mapper, ProgressRepository progressRepository, QuizRepository quizRepository)
        {
            _levelRepository = levelRepository;
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _userContext = userContext;
            _mapper = mapper;
            _progressRepository = progressRepository;
            _quizRepository = quizRepository;
        }

        //level
        /*
         * lấy danh sách level
         * 
         * thuphuong21072004
         */
        public async Task<List<LevelDTO>> GetLevels()
        {
            var levels = await _levelRepository.GetLevels();

            return _mapper.Map<List<LevelDTO>>(levels);
        }
        /*
         * lưu level
         * 
         * thuphuong21072004
         */
        public async Task SaveLevels(List<LevelDTO> list)
        {
            if (list == null) return;

            var deleteIds = list
                .Where(x => x.IsDelete && x.LevelId != 0)
                .Select(x => x.LevelId)
                .ToList();

            if (deleteIds.Any())
            {
                await _levelRepository.DeleteLevels(deleteIds);
            }

            foreach (var dto in list.Where(x => !x.IsDelete))
            {
                if (dto.LevelId == 0)
                {
                    var level = _mapper.Map<Level>(dto);
                    level.IsActive = true;

                    var maxOrder = await _levelRepository.GetMaxOrderIndex();
                    level.OrderIndex = maxOrder + 1;

                    await _levelRepository.AddLevel(level);
                }
                else
                {
                    var level = await _levelRepository.GetLevelById(dto.LevelId);

                    if (level != null)
                    {
                        dto.OrderIndex = level.OrderIndex;
                        _mapper.Map(dto, level);

                        await _levelRepository.UpdateLevel(level);
                    }
                }
            }

            await _levelRepository.Save();
        }

        // course
        /*
         * lấy danh sách course theo level
         * 
         * thuphuong21072004
         */
        public async Task<List<CourseDTO>> GetCourses(int levelId)
        {
            int userId = _userContext.GetUserId();

            var courses = await _courseRepository.GetCourses(levelId);
            var userCourses = await _courseRepository.GetUserCourses(userId, levelId);

            return courses.Select(c =>
            {
                var dto = _mapper.Map<CourseDTO>(c);

                var uc = userCourses.FirstOrDefault(x => x.CourseId == c.CourseId);

                return dto;
            }).ToList();
        }
        /*
         * lưu course
         * 
         * thuphuong21072004
         */
        public async Task SaveCourses(List<CourseDTO> list)
        {
            if (list == null) return;

            int userId = _userContext.GetUserId();

            var deleteIds = list
                .Where(x => x.IsDelete && x.CourseId != 0)
                .Select(x => x.CourseId)
                .ToList();

            if (deleteIds.Any())
            {
                await _courseRepository.DeleteCourses(deleteIds);
            }

            foreach (var dto in list.Where(x => !x.IsDelete))
            {
                if (dto.CourseId == 0)
                {
                    var course = _mapper.Map<Course>(dto);
                    course.CreatedBy = userId;
                    course.IsActive = true;

                    var maxOrder = await _courseRepository.GetMaxOrderIndex(dto.LevelId);
                    course.OrderIndex = maxOrder + 1;

                    await _courseRepository.Add(course);
                }
                else
                {
                    var course = await _courseRepository.GetById(dto.CourseId);

                    if (course != null)
                    {
                        dto.OrderIndex = course.OrderIndex;
                        dto.LevelId = course.LevelId;

                        _mapper.Map(dto, course);

                        await _courseRepository.Update(course);
                    }
                }
            }

            await _courseRepository.Save();
        }

        // lesson
        /*
         * lấy danh sách lesson theo course
         * 
         * thuphuong21072004
         */
        public async Task<List<LessonDTO>> GetLessons(int courseId)
        {
            int userId = _userContext.GetUserId();

            var lessons = await _lessonRepository.GetLessons(courseId);
            var progress = await _lessonRepository.GetUserProgress(userId, courseId);

            return lessons.Select(l =>
            {
                var dto = _mapper.Map<LessonDTO>(l);

                var p = progress.FirstOrDefault(x => x.LessonId == l.LessonId);

                return dto;
            }).ToList();
        }
        /*
         * lưu lesson
         * 
         * thuphuong21072004
         */
        public async Task SaveLessons(List<LessonDTO> list)
        {
            if (list == null) return;

            var deleteIds = list
                .Where(x => x.IsDelete && x.LessonId != 0)
                .Select(x => x.LessonId)
                .ToList();

            if (deleteIds.Any())
            {
                await _lessonRepository.DeleteLessons(deleteIds);
            }

            foreach (var dto in list.Where(x => !x.IsDelete))
            {
                if (dto.LessonId == 0)
                {
                    var lesson = _mapper.Map<Lesson>(dto);
                    lesson.IsActive = true;

                    var maxOrder = await _lessonRepository.GetMaxOrderIndex(dto.CourseId);
                    lesson.OrderIndex = maxOrder + 1;

                    await _lessonRepository.Add(lesson);
                }
                else
                {
                    var lesson = await _lessonRepository.GetById(dto.LessonId);

                    if (lesson != null)
                    {
                        dto.OrderIndex = lesson.OrderIndex;
                        dto.CourseId = lesson.CourseId;

                        _mapper.Map(dto, lesson);

                        await _lessonRepository.Update(lesson);
                    }
                }
            }

            await _lessonRepository.Save();
        }

        // User Lesson//
        /*
         * mở khóa bài học đầu tiên cho user khi họ bắt đầu học (lần đầu tiên)
         * 
         * thuphuong21072004
         */
        private async Task UnlockFirstLesson(int userId)
        {
            var hasLevel = await _progressRepository.HasUserLevel(userId);
            var hasCourse = await _progressRepository.HasUserCourse(userId);
            var hasLesson = await _progressRepository.HasUserLesson(userId);

            var firstLevel = (await _levelRepository.GetLevels())
                .OrderBy(x => x.OrderIndex)
                .FirstOrDefault();

            if (firstLevel == null) return;

            var firstCourse = (await _courseRepository.GetCourses(firstLevel.LevelId))
                .FirstOrDefault();

            if (firstCourse == null) return;

            var firstLesson = (await _lessonRepository.GetLessons(firstCourse.CourseId))
                .FirstOrDefault();

            if (firstLesson == null) return;

            if (!hasLevel)
            {
                await _progressRepository.AddUserLevel(new UserLevel
                {
                    UserId = userId,
                    LevelId = firstLevel.LevelId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });
            }

            if (!hasCourse)
            {
                await _progressRepository.AddUserCourse(new UserCourse
                {
                    UserId = userId,
                    CourseId = firstCourse.CourseId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });
            }

            if (!hasLesson)
            {
                await _progressRepository.AddUserProgress(new UserProgress
                {
                    UserId = userId,
                    LessonId = firstLesson.LessonId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });
            }

            await _progressRepository.Save();
        }
        /*
         * lấy tiến độ học tập của user hiện tại
         * 
         * thuphuong21072004
         */
        public async Task<List<LevelDTO>> GetMyProgress()
        {
            var userId = _userContext.GetUserId();

            await UnlockFirstLesson(userId);

            var currentLevel = await _progressRepository.GetCurrentLevel(userId);
            var currentCourse = await _progressRepository.GetCurrentCourse(userId);

            var levels = await _levelRepository.GetLevels() ?? new List<Level>();
            var result = new List<LevelDTO>();

            foreach (var level in levels)
            {
                if (level == null) continue;

                var ul = await _progressRepository.GetUserLevel(userId, level.LevelId);

                var levelDTO = new LevelDTO
                {
                    LevelId = level.LevelId,
                    LevelName = level.LevelName,
                    Description = level.Description,
                    OrderIndex = level.OrderIndex,
                    IsActive = level.IsActive,
                    Status = ul?.Status,
                    Courses = new List<CourseDTO>()
                };

                if (currentLevel != null && level.LevelId == currentLevel.LevelId)
                {
                    var courses = await _courseRepository.GetCourses(level.LevelId)
                                  ?? new List<Course>();

                    foreach (var course in courses)
                    {
                        if (course == null) continue;

                        var uc = await _progressRepository.GetUserCourseByCourseId(userId, course.CourseId);

                        var courseDTO = new CourseDTO
                        {
                            CourseId = course.CourseId,
                            LevelId = course.LevelId,
                            CourseName = course.CourseName,
                            Description = course.Description,
                            OrderIndex = course.OrderIndex,
                            IsActive = course.IsActive,
                            Status = uc?.Status, 
                            Lessons = new List<LessonDTO>()
                        };

                        if (currentCourse != null && course.CourseId == currentCourse.CourseId)
                        {
                            var lessons = await _lessonRepository.GetLessons(course.CourseId)
                                          ?? new List<Lesson>();

                            var userLessons = await _progressRepository.GetUserLessons(userId, course.CourseId)
                                              ?? new List<UserProgress>();

                            foreach (var lesson in lessons)
                            {
                                if (lesson == null) continue;

                                var ulp = userLessons.FirstOrDefault(x => x.LessonId == lesson.LessonId);

                                var lessonDTO = new LessonDTO
                                {
                                    LessonId = lesson.LessonId,
                                    CourseId = lesson.CourseId,
                                    LessonName = lesson.LessonName,
                                    VideoUrl = lesson.VideoUrl,
                                    Duration = lesson.Duration,
                                    OrderIndex = lesson.OrderIndex,
                                    Status = ulp?.Status 
                                };

                                courseDTO.Lessons.Add(lessonDTO);
                            }
                        }

                        levelDTO.Courses.Add(courseDTO);
                    }
                }

                result.Add(levelDTO);
            }

            return result;
        }
        /*
         * lấy tất cả danh sách level, course, lesson
         * 
         * thuphuong21072004
         */
        public async Task<List<LevelDTO>> GetAllLearningPath()
        {
            var levels = await _levelRepository.GetLevels();
            var result = new List<LevelDTO>();

            foreach (var level in levels)
            {
                var levelDTO = new LevelDTO
                {
                    LevelId = level.LevelId,
                    LevelName = level.LevelName,
                    Description = level.Description,
                    OrderIndex = level.OrderIndex,
                    IsActive = level.IsActive,
                    Courses = new List<CourseDTO>()
                };

                var courses = await _courseRepository.GetCourses(level.LevelId);

                foreach (var course in courses)
                {
                    var courseDTO = new CourseDTO
                    {
                        CourseId = course.CourseId,
                        LevelId = course.LevelId,
                        CourseName = course.CourseName,
                        Description = course.Description,
                        OrderIndex = course.OrderIndex,
                        IsActive = course.IsActive,
                        Lessons = new List<LessonDTO>()
                    };

                    var lessons = await _lessonRepository.GetLessons(course.CourseId);

                    foreach (var lesson in lessons)
                    {
                        courseDTO.Lessons.Add(new LessonDTO
                        {
                            LessonId = lesson.LessonId,
                            CourseId = lesson.CourseId,
                            LessonName = lesson.LessonName,
                            VideoUrl = lesson.VideoUrl,
                            Duration = lesson.Duration,
                            OrderIndex = lesson.OrderIndex,
                        });
                    }

                    levelDTO.Courses.Add(courseDTO);
                }

                result.Add(levelDTO);
            }

            return result;
        }

        //User Question//
        /*
         * lấy quiz theo lesson đang học
         * 
         * thuphuong21072004
         */
        public async Task<QuizDTO> GetQuizByLesson(int lessonId)
        {
            var quiz = await _quizRepository.GetQuizByLesson(lessonId);
            if (quiz == null) return null;

            var quizDTO = _mapper.Map<QuizDTO>(quiz);

            var questions = await _quizRepository.GetAllQuestions(quiz.QuizId);

            var questionDTOs = new List<QuestionDTO>();

            foreach (var q in questions)
            {
                var answers = await _quizRepository.GetAllAnswers(q.QuestionId);
                var questionDTO = _mapper.Map<QuestionDTO>(q);
                questionDTO.Answers = _mapper.Map<List<AnswerDTO>>(answers);
                questionDTOs.Add(questionDTO);
            }
            quizDTO.Questions = questionDTOs;

            return quizDTO;
        }
        /*
         * mở level tiếp theo
         * 
         * thuphuong21072004
         */
        public async Task UnlockNextLevel(int lessonId)
        {
            int userId = _userContext.GetUserId();

            var lesson = await _lessonRepository.GetById(lessonId);
            var course = await _courseRepository.GetById(lesson.CourseId);
            var level = await _levelRepository.GetLevelById(course.LevelId);

            var courses = await _courseRepository.GetCourses(level.LevelId);
            var userCourses = await _progressRepository.GetUserCourses(userId, level.LevelId);

            bool allCompleted = courses.All(c =>
                userCourses.Any(uc => uc.CourseId == c.CourseId && uc.Status == true));

            if (!allCompleted) return;

            var userLevel = await _progressRepository.GetUserLevelByLevelId(userId, level.LevelId);
            userLevel.Status = true;
            userLevel.CompletedDate = DateTime.Now;

            var nextLevel = (await _levelRepository.GetLevels())
                .FirstOrDefault(x => x.OrderIndex == level.OrderIndex + 1);

            if (nextLevel != null)
            {
                await _progressRepository.AddUserLevel(new UserLevel
                {
                    UserId = userId,
                    LevelId = nextLevel.LevelId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });

                var firstCourse = (await _courseRepository.GetCourses(nextLevel.LevelId))
                    .OrderBy(x => x.OrderIndex)
                    .FirstOrDefault();

                await _progressRepository.AddUserCourse(new UserCourse
                {
                    UserId = userId,
                    CourseId = firstCourse.CourseId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });

                var firstLesson = (await _lessonRepository.GetLessons(firstCourse.CourseId))
                    .OrderBy(x => x.OrderIndex)
                    .FirstOrDefault();

                await _progressRepository.AddUserProgress(new UserProgress
                {
                    UserId = userId,
                    LessonId = firstLesson.LessonId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });
            }

            await _progressRepository.Save();
        }
        /*
         * mở course tiếp theo
         * 
         * thuphuong21072004
         */
        public async Task UnlockNextCourse(int lessonId)
        {
            int userId = _userContext.GetUserId();

            var lesson = await _lessonRepository.GetById(lessonId);
            var course = await _courseRepository.GetById(lesson.CourseId);

            var lessons = await _lessonRepository.GetLessons(course.CourseId);
            var progress = await _progressRepository.GetUserLessons(userId, course.CourseId);

            bool allCompleted = lessons.All(l =>
                progress.Any(p => p.LessonId == l.LessonId && p.Status == true));

            if (!allCompleted) return;

            var userCourse = await _progressRepository.GetUserCourseByCourseId(userId, course.CourseId);
            userCourse.Status = true;
            userCourse.CompletedDate = DateTime.Now;

            var nextCourse = (await _courseRepository.GetCourses(course.LevelId))
                .OrderBy(c => c.OrderIndex)
                .FirstOrDefault(c => c.OrderIndex > course.OrderIndex);

            if (nextCourse != null)
            {
                await _progressRepository.AddUserCourse(new UserCourse
                {
                    UserId = userId,
                    CourseId = nextCourse.CourseId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });

                var firstLesson = (await _lessonRepository.GetLessons(nextCourse.CourseId))
                    .OrderBy(x => x.OrderIndex)
                    .FirstOrDefault();

                await _progressRepository.AddUserProgress(new UserProgress
                {
                    UserId = userId,
                    LessonId = firstLesson.LessonId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });
            }
            else
            {
                await UnlockNextLevel(lessonId);
            }

            await _progressRepository.Save();
        }
        /*
         * mở khóa lesson tiếp theo 
         * 
         * thuphuong21072004
         */
        public async Task UnlockNextLesson(int lessonId)
        {
            int userId = _userContext.GetUserId();

            var lesson = await _lessonRepository.GetById(lessonId);

            var progress = await _progressRepository.GetUserLessonByLessonId(userId, lessonId);
            progress.Status = true;
            progress.CompletedDate = DateTime.Now;

            var lessons = await _lessonRepository.GetLessons(lesson.CourseId);

            var nextLesson = lessons
                .FirstOrDefault(x => x.OrderIndex == lesson.OrderIndex + 1);

            if (nextLesson != null)
            {
                var exist = await _progressRepository.GetUserLessonByLessonId(userId, nextLesson.LessonId);
                if (exist == null)
                {
                    await _progressRepository.AddUserProgress(new UserProgress
                    {
                        UserId = userId,
                        LessonId = nextLesson.LessonId,
                        AssignedDate = DateTime.Now,
                        Status = false
                    });
                }
            }
            else
            {
                await UnlockNextCourse(lessonId);
            }

            await _progressRepository.Save();
        }
        /*
         * chấm điểm -> đậu/rớt
         * 
         * thuphuong21072004
         */
        public async Task SubmitQuiz(int quizId, List<int> answerIds)
        {
            int userId = _userContext.GetUserId();

            var questions = await _quizRepository.GetAllQuestions(quizId);

            int total = questions.Count;
            int correct = 0;

            foreach (var q in questions)
            {
                var correctAnswer = await _quizRepository.GetCorrectAnswer(q.QuestionId);

                if (correctAnswer != null && answerIds.Contains(correctAnswer.AnswerId))
                    correct++;
            }

            double score = (double)correct * 100 / total;

            var quiz = await _quizRepository.GetQuizById(quizId);

            bool isPassed = score >= quiz.PassScore;

            await _quizRepository.SaveUserQuiz(new UserQuiz
            {
                UserId = userId,
                QuizId = quizId,
                Score = score,
                CompletedDate = DateTime.Now,
                IsPassed = isPassed
            });

            await _quizRepository.Save();

            if (isPassed)
            {
                var lesson = await _quizRepository.GetLessonByQuizId(quizId);
                await UnlockNextLesson(lesson.LessonId);
            }
        }

        /*
         * lưu câu trả lời của câu hỏi
         * 
         * thuphuong21072004
         */
        private async Task SaveAnswers(int questionId, List<AnswerDTO> list)
        {
            if (list == null) return;

           
            var deleteIds = list
                .Where(x => x.IsDelete && x.AnswerId != 0)
                .Select(x => x.AnswerId)
                .ToList();

            if (deleteIds.Any())
            {
                await _quizRepository.DeleteAnswers(deleteIds);
            }

            foreach (var aDto in list.Where(x => !x.IsDelete))
            {
                if (aDto.AnswerId == 0)
                {
                    await _quizRepository.AddAnswer(new Answer
                    {
                        QuestionId = questionId,
                        AnswerText = aDto.AnswerText,
                        IsCorrect = aDto.IsCorrect
                    });
                }
                else
                {
                    var answer = await _quizRepository
                        .GetAnswerByIdAndQuestion(aDto.AnswerId, questionId);

                    answer.AnswerText = aDto.AnswerText;
                    answer.IsCorrect = aDto.IsCorrect;

                    await _quizRepository.UpdateAnswer(answer);
                }
            }
        }
        /*
         * lưu câu hỏi và danh sách câu trả lời
         * 
         * thuphuong21072004
         */
        private async Task SaveQuestions(int quizId, List<QuestionDTO> list)
        {
            if (list == null) return;

            var deleteIds = list
                .Where(x => x.IsDelete && x.QuestionId != 0)
                .Select(x => x.QuestionId)
                .ToList();

            if (deleteIds.Any())
            {
                await _quizRepository.DeleteQuestions(deleteIds);
            }

            
            foreach (var qDto in list.Where(x => !x.IsDelete))
            {
                Question question;

                if (qDto.QuestionId == 0)
                {
                    question = new Question
                    {
                        QuizId = quizId,
                        QuestionText = qDto.QuestionText
                    };

                    await _quizRepository.AddQuestion(question);
                    await _quizRepository.Save(); 
                }
                else
                {
                    question = await _quizRepository
                        .GetQuestionByIdAndQuiz(qDto.QuestionId, quizId);

                    question.QuestionText = qDto.QuestionText;

                    await _quizRepository.UpdateQuestion(question);
                }

                await SaveAnswers(question.QuestionId, qDto.Answers);
            }
        }
        /*
         * lưu quiz và danh sách câu hỏi, câu trả lời
         * 
         * thuphuong21072004
         */
        public async Task SaveQuiz(QuizDTO dto)
        {
            Quiz quiz;

            if (dto.QuizId == 0)
            {
                
                quiz = new Quiz
                {
                    LessonId = dto.LessonId,
                    QuizName = dto.QuizName,
                    PassScore = dto.PassScore
                };

                await _quizRepository.AddQuiz(quiz);
                await _quizRepository.Save(); 
            }
            else
            {
                
                quiz = await _quizRepository.GetQuizById(dto.QuizId);

                quiz.QuizName = dto.QuizName;
                quiz.PassScore = dto.PassScore;

                await _quizRepository.UpdateQuiz(quiz);
            }

            await SaveQuestions(quiz.QuizId, dto.Questions);

            await _quizRepository.Save();
        }

        /*
         * xóa quiz
         * 
         * thuphuong21072004
         */
        public async Task DeleteQuiz(int quizId)
        {
            await _quizRepository.DeleteQuiz(quizId);
        }
    }
}