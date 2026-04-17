using Backend.Common;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.impl
{
    public class LearningServiceImpl : LearningService
    {
        private readonly LevelRepository _levelRepository;
        private readonly CourseRepository _courseRepository;
        private readonly UnitRepository _unitRepository;
        private readonly UserContextUtil _userContext;
        private readonly ProgressRepository _progressRepository;
        public readonly QuizRepository _quizRepository;
        private readonly IMapper _mapper;

        public LearningServiceImpl(
            LevelRepository levelRepository,
            CourseRepository courseRepository,
            UnitRepository unitRepository,
            UserContextUtil userContext, IMapper mapper, ProgressRepository progressRepository, QuizRepository quizRepository)
        {
            _levelRepository = levelRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userContext = userContext;
            _mapper = mapper;
            _progressRepository = progressRepository;
            _quizRepository = quizRepository;
        }
        // validate
        /*
         * kiểm tra quyền là admin
         * 
         * thuphuong21072004
         */
        private bool ValidateAdmin()
        {
            string role = _userContext.GetRole();
            if (role == common.Constant.Role.Admin)
            {
                return true;
            }
            return false;
        }
        /*
         * kiem tra quyen user
         * 
         * thuphuong21072004
         */
        private bool ValidateUser()
        {
            string role = _userContext.GetRole();
            if (role == common.Constant.Role.User) { return true; }
            return false;
        }
        /*
         * kiêm tra cong tac vien
         * 
         * thuphuong21072004
         */
        private bool ValidateModerator()
        {
            string role = _userContext.GetRole();
            if (role == common.Constant.Role.Moderator) { return true; }
            return false;
        }

        //level
        /*
         * lấy danh sách level
         * 
         * thuphuong21072004
         */
        public async Task<List<LevelDTO>> GetLevels()
        {
            var levels = await _levelRepository.GetAllLevels(null);

            return _mapper.Map<List<LevelDTO>>(levels);
        }
        /*
         * lấy level theo id
         * 
         * thuphuong21072004
         */
        public async Task<LevelDTO?> GetLevelById(int levelId)
        {
            var level = await _levelRepository.GetLevelById(levelId);

            if (level == null) return null;

            return _mapper.Map<LevelDTO>(level);
        }
        /*
         * thêm, cập nhật và xóa danh sách level
         * 
         * thuphuong21072004
         */
        public async Task SaveLevels(List<LevelDTO> list)
        {
            if (!ValidateAdmin()&&!ValidateModerator())
            {
                throw new UnauthorizedAccessException("You do not have the right to manage Levels.");
            }
            if (list == null) return;

            var deleteIds = list
                .Where(x => x.IsDelete && x.LevelId != 0)
                .Select(x => x.LevelId)
                .ToList();

            if (deleteIds.Any())
            {
                if (!ValidateAdmin())
                {
                    throw new UnauthorizedAccessException("You do not have the right to manage Levels.");
                }
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

            await _levelRepository.SaveLevel();
        }

        // course
        /*
         * lấy danh sách course theo level kèm trạng thái của user
         * 
         * thuphuong21072004
         */
        public async Task<List<CourseDTO>> GetCourses(int levelId)
        {
            int userId = _userContext.GetUserId();

            var courses = await _courseRepository.GetAllCourses(levelId,null);
            var userCourses = await _courseRepository.GetUserCourses(userId, levelId);

            return courses.Select(c =>
            {
                var dto = _mapper.Map<CourseDTO>(c);

                var uc = userCourses.FirstOrDefault(x => x.CourseId == c.CourseId);

                return dto;
            }).ToList();
        }
        /*
         * lấy course theo id
         * 
         * thuphuong21072004
         */
        public async Task<CourseDTO?> GetCourseById(int courseId)
        {
            var course = await _courseRepository.GetCourseById(courseId);

            if (course == null) return null;

            return _mapper.Map<CourseDTO>(course);
        }
        /*
         * thêm, cập nhật và xóa danh sách course
         * 
         * thuphuong21072004
         */
        public async Task SaveCourses(List<CourseDTO> list)
        {
            if (!ValidateAdmin() && !ValidateModerator())
            {
                throw new UnauthorizedAccessException("You do not have course management privileges.");
            }
            if (list == null) return;

            string currentUserName = _userContext.GetName();

            var deleteIds = list
                .Where(x => x.IsDelete && x.CourseId != 0)
                .Select(x => x.CourseId)
                .ToList();

            if (deleteIds.Any())
            {
                if (!ValidateAdmin())
                {
                    throw new UnauthorizedAccessException("You do not have course management privileges.");
                }
                await _courseRepository.DeleteCourses(deleteIds);
            }

            foreach (var dto in list.Where(x => !x.IsDelete))
            {
                if (dto.CourseId == 0)
                {
                    var course = _mapper.Map<Course>(dto);
                    course.CreatedBy = currentUserName;
                    course.IsActive = true;

                    var maxOrder = await _courseRepository.GetMaxOrderIndex(dto.LevelId);
                    course.OrderIndex = maxOrder + 1;

                    await _courseRepository.AddCourse(course);
                }
                else
                {
                    var course = await _courseRepository.GetCourseById(dto.CourseId);

                    if (course != null)
                    {
                        dto.OrderIndex = course.OrderIndex;
                        dto.LevelId = course.LevelId;

                        _mapper.Map(dto, course);

                        await _courseRepository.UpdateCourse(course);
                    }
                }
            }

            await _courseRepository.SaveCourse();
        }

        // Unit
        /*
         * lấy danh sách Unit theo course kèm tiến trình học của user
         * 
         * thuphuong21072004
         */
        public async Task<List<UnitDTO>> GetUnits(int courseId)
        {
            int userId = _userContext.GetUserId();

            var Units = await _unitRepository.GetAllUnits(courseId, null);
            var progress = await _unitRepository.GetUserProgress(userId, courseId);

            return Units.Select(l =>
            {
                var dto = _mapper.Map<UnitDTO>(l);

                var p = progress.FirstOrDefault(x => x.UnitId == l.UnitId);

                return dto;
            }).ToList();
        }
        /*
         * lấy Unit theo id
         * 
         * thuphuong21072004
         */
        public async Task<UnitDTO?> GetUnitById(int UnitId)
        {
            var Unit = await _unitRepository.GetUnitById(UnitId);

            if (Unit == null) return null;

            return _mapper.Map<UnitDTO>(Unit);
        }
        /*
         * thêm hoặc cập nhật Unit và xử lý file video cũ
         * 
         * thuphuong21072004
         */
        public async Task SaveUnit(UnitDTO dto)
        {
            if (!ValidateAdmin() && !ValidateModerator())
            {
                throw new UnauthorizedAccessException("You do not have permission to manage the Unit.");
            }
            if (dto == null) return;
            string currentUserName = _userContext.GetName();
            if (dto.UnitId == 0)
            {
                var Unit = _mapper.Map<Unit>(dto);
                Unit.IsActive = true;
                Unit.CreatedBy = currentUserName;
                Unit.CreatedDate = DateTime.Now;

                var maxOrder = await _unitRepository.GetMaxOrderIndex(dto.CourseId);
                Unit.OrderIndex = maxOrder + 1;

                await _unitRepository.AddUnit(Unit);
            }
            else
            {
                var Unit = await _unitRepository.GetUnitById(dto.UnitId);

                if (Unit != null)
                {
                    
                    if (!string.IsNullOrEmpty(Unit.VideoUrl) && Unit.VideoUrl != dto.VideoUrl)
                    {
                        var oldFileName = Path.GetFileName(Unit.VideoUrl);
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", oldFileName);

                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }

                    dto.OrderIndex = Unit.OrderIndex;
                    dto.CourseId = Unit.CourseId;

                    _mapper.Map(dto, Unit);

                    await _unitRepository.UpdateUnit(Unit);
                }
            }

            await _unitRepository.SaveUnit();
        }
        /*
         * xóa danh sách Unit và file video liên quan
         * 
         * thuphuong21072004
         */
        public async Task DeleteUnits(List<int> UnitIds)
        {
            if (!ValidateAdmin() )
            {
                throw new UnauthorizedAccessException("You do not have the right to delete lessons.");
            }
            if (UnitIds == null || !UnitIds.Any()) return;

            var Units = await _unitRepository.GetUnitsByIds(UnitIds);

            foreach (var Unit in Units)
            {
                if (!string.IsNullOrEmpty(Unit.VideoUrl))
                {
                    var fileName = Path.GetFileName(Unit.VideoUrl);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", fileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            await _unitRepository.DeleteUnits(UnitIds);
            await _unitRepository.SaveUnit();
        }
        // User Unit//
        /*
         * mở khóa bài học đầu tiên cho user khi họ bắt đầu học (lần đầu tiên)
         * 
         * thuphuong21072004
         */
        private async Task UnlockFirstUnit(int userId)
        {
            var hasLevel = await _progressRepository.HasUserLevel(userId);
            var hasCourse = await _progressRepository.HasUserCourse(userId);
            var hasUnit = await _progressRepository.HasUserUnit(userId);

            var firstLevel = (await _levelRepository.GetAllLevels(true))
    .OrderBy(x => x.OrderIndex)
    .FirstOrDefault();

            if (firstLevel == null) return;

            var firstCourse = (await _courseRepository.GetAllCourses(firstLevel.LevelId, true))
    .FirstOrDefault();

            if (firstCourse == null) return;

            var firstUnit = (await _unitRepository.GetAllUnits(firstCourse.CourseId, true))
     .FirstOrDefault();

            if (firstUnit == null) return;

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

            if (!hasUnit)
            {
                await _progressRepository.AddUserProgress(new UserProgress
                {
                    UserId = userId,
                    UnitId = firstUnit.UnitId,
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

            await UnlockFirstUnit(userId);

            var currentLevel = await _progressRepository.GetCurrentLevel(userId);
            var currentCourse = await _progressRepository.GetCurrentCourse(userId);

            var levels = await _levelRepository.GetAllLevels(true) ?? new List<Level>();
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
                    var courses = await _courseRepository.GetAllCourses(level.LevelId,true)
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
                            Units = new List<UnitDTO>()
                        };

                        if (currentCourse != null && course.CourseId == currentCourse.CourseId)
                        {
                            var Units = await _unitRepository.GetAllUnits(course.CourseId, true) ?? new List<Unit>();

                            var userUnits = await _progressRepository.GetUserUnits(userId, course.CourseId)
                                              ?? new List<UserProgress>();

                            foreach (var Unit in Units)
                            {
                                if (Unit == null) continue;

                                var ulp = userUnits.FirstOrDefault(x => x.UnitId == Unit.UnitId);

                                var UnitDTO = new UnitDTO
                                {
                                    UnitId = Unit.UnitId,
                                    CourseId = Unit.CourseId,
                                    UnitName = Unit.UnitName,
                                    VideoUrl = Unit.VideoUrl,
                                    Duration = Unit.Duration,
                                    OrderIndex = Unit.OrderIndex,
                                    Status = ulp?.Status 
                                };

                                courseDTO.Units.Add(UnitDTO);
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
         * lấy tất cả danh sách level, course, Unit
         * O(1)
         * thuphuong21072004
         */
        public async Task<List<LevelDTO>> GetAllLearningPath()
        {
            
            var levels = await _levelRepository.GetQueryable() 
                .Include(l => l.Courses.Where(c => c.IsActive))
                .ThenInclude(c => c.Units.Where(u => u.IsActive))
                .Where(l => l.IsActive)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();

            return _mapper.Map<List<LevelDTO>>(levels);
        }

        //User Question//
        private void DeleteMediaFile(string url, string folder)
        {
            if (string.IsNullOrEmpty(url)) return;

            var fileName = Path.GetFileName(url);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        /*
         * lấy quiz theo Unit đang học
         * 
         * thuphuong21072004
         */
        public async Task<QuizDTO> GetQuizByUnit(int UnitId)
        {
            int userId = _userContext.GetUserId();
            
            var quiz = await _quizRepository.GetQuizByUnit(UnitId);
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
         * mở level tiếp theo khi user hoàn thành toàn bộ course của level hiện tại
         * 
         * thuphuong21072004
         */
        public async Task UnlockNextLevel(int UnitId)
        {
            int userId = _userContext.GetUserId();

            var Unit = await _unitRepository.GetUnitById(UnitId);
            var course = await _courseRepository.GetCourseById(Unit.CourseId);
            var level = await _levelRepository.GetLevelById(course.LevelId);

            var courses = await _courseRepository.GetAllCourses(level.LevelId,true);
            var userCourses = await _progressRepository.GetUserCourses(userId, level.LevelId);

            bool allCompleted = courses.All(c =>
                userCourses.Any(uc => uc.CourseId == c.CourseId && uc.Status == true));

            if (!allCompleted) return;

            var userLevel = await _progressRepository.GetUserLevel(userId, level.LevelId);
            userLevel.Status = true;
            userLevel.CompletedDate = DateTime.Now;

            var nextLevel = (await _levelRepository.GetAllLevels(true))
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

                var firstCourse = (await _courseRepository.GetAllCourses(nextLevel.LevelId, true))
    .OrderBy(x => x.OrderIndex)
    .FirstOrDefault();

                await _progressRepository.AddUserCourse(new UserCourse
                {
                    UserId = userId,
                    CourseId = firstCourse.CourseId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });

                var firstUnit = (await _unitRepository.GetAllUnits(firstCourse.CourseId,true))
                    .OrderBy(x => x.OrderIndex)
                    .FirstOrDefault();

                await _progressRepository.AddUserProgress(new UserProgress
                {
                    UserId = userId,
                    UnitId = firstUnit.UnitId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });
            }

            await _progressRepository.Save();
        }
        /*
         * mở course tiếp theo khi hoàn thành toàn bộ Unit của course hiện tại
         * 
         * thuphuong21072004
         */
        public async Task UnlockNextCourse(int UnitId)
        {
            int userId = _userContext.GetUserId();

            var Unit = await _unitRepository.GetUnitById(UnitId);
            var course = await _courseRepository.GetCourseById(Unit.CourseId);

            var Units = await _unitRepository.GetAllUnits(course.CourseId,true);
            var progress = await _progressRepository.GetUserUnits(userId, course.CourseId);

            bool allCompleted = Units.All(l =>
                progress.Any(p => p.UnitId == l.UnitId && p.Status == true));

            if (!allCompleted) return;

            var userCourse = await _progressRepository.GetUserCourseByCourseId(userId, course.CourseId);
            userCourse.Status = true;
            userCourse.CompletedDate = DateTime.Now;

            var nextCourse = (await _courseRepository.GetAllCourses(course.LevelId, true))
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

                var firstUnit = (await _unitRepository.GetAllUnits(nextCourse.CourseId,true))
                    .OrderBy(x => x.OrderIndex)
                    .FirstOrDefault();

                await _progressRepository.AddUserProgress(new UserProgress
                {
                    UserId = userId,
                    UnitId = firstUnit.UnitId,
                    AssignedDate = DateTime.Now,
                    Status = false
                });
            }
            else
            {
                await UnlockNextLevel(UnitId);
            }

            await _progressRepository.Save();
        }
        /*
         * mở khóa Unit tiếp theo sau khi hoàn thành Unit hiện tại
         * 
         * thuphuong21072004
         */
        public async Task UnlockNextUnit(int UnitId)
        {
            int userId = _userContext.GetUserId();

            var Unit = await _unitRepository.GetUnitById(UnitId);

            var progress = await _progressRepository.GetUserUnitByUnitId(userId, UnitId);
            progress.Status = true;
            progress.CompletedDate = DateTime.Now;

            var Units = await _unitRepository.GetAllUnits(Unit.CourseId,true);

            var nextUnit = Units
                .FirstOrDefault(x => x.OrderIndex == Unit.OrderIndex + 1);

            if (nextUnit != null)
            {
                var exist = await _progressRepository.GetUserUnitByUnitId(userId, nextUnit.UnitId);
                if (exist == null)
                {
                    await _progressRepository.AddUserProgress(new UserProgress
                    {
                        UserId = userId,
                        UnitId = nextUnit.UnitId,
                        AssignedDate = DateTime.Now,
                        Status = false
                    });
                }
            }
            else
            {
                await UnlockNextCourse(UnitId);
            }

            await _progressRepository.Save();
        }
        /*
         * chấm điểm quiz, lưu kết quả và mở khóa bài học tiếp theo nếu đạt
         * 
         * thuphuong21072004
         */
        public async Task SubmitQuiz(int quizId, List<int> answerIds)
        {
            if (!ValidateUser())
            {
                throw new UnauthorizedAccessException("Only students are allowed to submit assignments.");
            }
            int userId = _userContext.GetUserId();

            var questions = await _quizRepository.GetAllQuestions(quizId);

            int total = questions.Count;
            if (total == 0) throw new Exception("The test currently has no questions.");
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
                var Unit = await _quizRepository.GetUnitByQuizId(quizId);
                await UnlockNextUnit(Unit.UnitId);
            }
        }
        /*
         * xem điểm
         * 
         * thupuong21072004
         */
        public async Task<UserQuiz?> GetMyQuizResult(int quizId)
        {
            int userId = _userContext.GetUserId();
            return await _quizRepository.GetUserQuiz(userId, quizId);
        }
        /*
         * lưu câu trả lời của câu hỏi
         * 
         * thuphuong21072004
         */
        private async Task SaveAnswers(int questionId, List<AnswerDTO> list)
        {
            if (list == null) return;

            var deleteItems = list.Where(x => x.IsDelete && x.AnswerId != 0).ToList();
            foreach (var item in deleteItems)
            {
                var oldAnswer = await _quizRepository.GetAnswerByIdAndQuestion(item.AnswerId, questionId);
                if (oldAnswer != null)
                {
                    DeleteMediaFile(oldAnswer.ImageUrl, "images");
                    DeleteMediaFile(oldAnswer.AudioUrl, "audios");
                }
            }

            if (deleteItems.Any())
            {
                await _quizRepository.DeleteAnswers(deleteItems.Select(x => x.AnswerId).ToList());
            }

            foreach (var aDto in list.Where(x => !x.IsDelete))
            {
                if (aDto.AnswerId == 0)
                {
                    await _quizRepository.AddAnswer(new Answer
                    {
                        QuestionId = questionId,
                        AnswerText = aDto.AnswerText,
                        IsCorrect = aDto.IsCorrect,
                        ImageUrl = aDto.ImageUrl,
                        AudioUrl = aDto.AudioUrl
                    });
                }
                else
                {
                    var answer = await _quizRepository.GetAnswerByIdAndQuestion(aDto.AnswerId, questionId);
                    if (answer != null)
                    {
                        if (answer.ImageUrl != aDto.ImageUrl) DeleteMediaFile(answer.ImageUrl, "images");
                        if (answer.AudioUrl != aDto.AudioUrl) DeleteMediaFile(answer.AudioUrl, "audios");

                        answer.AnswerText = aDto.AnswerText;
                        answer.IsCorrect = aDto.IsCorrect;
                        answer.ImageUrl = aDto.ImageUrl;
                        answer.AudioUrl = aDto.AudioUrl;

                        await _quizRepository.UpdateAnswer(answer);
                    }
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

            var deleteItems = list.Where(x => x.IsDelete && x.QuestionId != 0).ToList();
            foreach (var item in deleteItems)
            {
                var oldQ = await _quizRepository.GetQuestionByIdAndQuiz(item.QuestionId, quizId);
                if (oldQ != null)
                {
                    DeleteMediaFile(oldQ.ImageUrl, "images");
                    DeleteMediaFile(oldQ.AudioUrl, "audios");

                    var answers = await _quizRepository.GetAllAnswers(oldQ.QuestionId);
                    foreach (var ans in answers)
                    {
                        DeleteMediaFile(ans.ImageUrl, "images");
                        DeleteMediaFile(ans.AudioUrl, "audios");
                    }
                }
            }

            if (deleteItems.Any())
            {
                await _quizRepository.DeleteQuestions(deleteItems.Select(x => x.QuestionId).ToList());
            }

            foreach (var qDto in list.Where(x => !x.IsDelete))
            {
                Question question;
                if (qDto.QuestionId == 0)
                {
                    question = new Question
                    {
                        QuizId = quizId,
                        QuestionText = qDto.QuestionText,
                        ImageUrl = qDto.ImageUrl,
                        AudioUrl = qDto.AudioUrl
                    };
                    await _quizRepository.AddQuestion(question);
                    await _quizRepository.Save();
                }
                else
                {
                    question = await _quizRepository.GetQuestionByIdAndQuiz(qDto.QuestionId, quizId);
                    if (question != null)
                    {
                        if (question.ImageUrl != qDto.ImageUrl) DeleteMediaFile(question.ImageUrl, "images");
                        if (question.AudioUrl != qDto.AudioUrl) DeleteMediaFile(question.AudioUrl, "audios");

                        question.QuestionText = qDto.QuestionText;
                        question.ImageUrl = qDto.ImageUrl;
                        question.AudioUrl = qDto.AudioUrl;
                        await _quizRepository.UpdateQuestion(question);
                    }
                }
                await SaveAnswers(question.QuestionId, qDto.Answers);
            }
        }
        /*
         * thêm hoặc cập nhật quiz và toàn bộ câu hỏi, câu trả lời
         * 
         * thuphuong21072004
         */
        public async Task SaveQuiz(QuizDTO dto)
        {
            if (!ValidateAdmin() && !ValidateModerator())
            {
                throw new UnauthorizedAccessException("You do not have permission to set up quizzes.");
            }
            Quiz quiz;

            if (dto.QuizId == 0)
            {
                
                quiz = new Quiz
                {
                    UnitId = dto.UnitId,
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
            if (!ValidateAdmin())
            {
                throw new UnauthorizedAccessException("You do not have the right to delete the quiz.");
            }

            var questions = await _quizRepository.GetAllQuestions(quizId);
            foreach (var q in questions)
            {
                DeleteMediaFile(q.ImageUrl, "images");
                DeleteMediaFile(q.AudioUrl, "audios");

                var answers = await _quizRepository.GetAllAnswers(q.QuestionId);
                foreach (var a in answers)
                {
                    DeleteMediaFile(a.ImageUrl, "images");
                    DeleteMediaFile(a.AudioUrl, "audios");
                }
            }

            await _quizRepository.DeleteQuiz(quizId);
            await _quizRepository.Save();
        }
    }
}