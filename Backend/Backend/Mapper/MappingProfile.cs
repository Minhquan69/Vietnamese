using AutoMapper;
using Backend.Models;
using Backend.dto;
namespace Backend.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Video,VideoDTO>();
            CreateMap<Transcript,TranscriptDTO>();
            
            CreateMap<Level, LevelDTO>().ReverseMap();
            CreateMap<Course, CourseDTO>().ReverseMap();
            CreateMap<Lesson, LessonDTO>().ReverseMap();
            CreateMap<Quiz, QuizDTO>();
            CreateMap<Question, QuestionDTO>();
            CreateMap<Answer, AnswerDTO>();
            CreateMap<QuizDTO, Quiz>();
            CreateMap<QuestionDTO, Question>();
            CreateMap<AnswerDTO, Answer>();
        }
    }
}