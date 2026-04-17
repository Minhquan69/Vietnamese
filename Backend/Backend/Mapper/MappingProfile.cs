using AutoMapper;
using Backend.Models;
using Backend.dto;

namespace Backend.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Video, VideoDTO>().ReverseMap();
            CreateMap<Transcript, TranscriptDTO>()
                .ForMember(dest => dest.YoutubeId, opt => opt.MapFrom(src => src.Video.YoutubeId))
                .ReverseMap();

            CreateMap<Level, LevelDTO>().ReverseMap();
            CreateMap<Course, CourseDTO>().ReverseMap();
            CreateMap<Unit, UnitDTO>().ReverseMap();

            CreateMap<Quiz, QuizDTO>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
                .ReverseMap(); 

            CreateMap<Question, QuestionDTO>()
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers))
                .ReverseMap();

            
            CreateMap<Answer, AnswerDTO>().ReverseMap();

        }
    }
}