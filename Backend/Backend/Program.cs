using Backend.Data;
using Backend.Repository;
using Backend.Repository.impl;
using Backend.Services;
using Backend.Services.impl;
using Backend.Options;
using Backend.Services.AiTutor;
using Backend.Services.Speaking;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Common;
using Backend.Mapper;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

var secretKey = builder.Configuration["AppSettings:SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException("Missing AppSettings:SecretKey configuration.");
}

var issuer = builder.Configuration["Jwt:Issuer"] ?? "VietnameseLearningApp";
var audience = builder.Configuration["Jwt:Audience"] ?? "VietnameseLearningClient";
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromSeconds(30),
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("AdminOrModerator", p => p.RequireRole("Admin", "Moderator"));
});


// ADD CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddScoped<JwtService>();

builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5_242_880;
});

builder.Services.AddScoped<UserService, UserServiceImpl>();
builder.Services.AddScoped<VideoService, VideoServiceImpl>();
builder.Services.AddScoped<LearningService, LearningServiceImpl>();
builder.Services.AddScoped<TestService, TestServiceImpl>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserContextUtil>();
builder.Services.AddScoped<UserRepository, UserRepositoryImpl>();
builder.Services.AddScoped<RefreshTokenRepository, RefreshTokenRepositoryImpl>();
builder.Services.AddHttpClient<VideoRepositoryImpl>();
builder.Services.AddScoped<VideoRepository, VideoRepositoryImpl>();
builder.Services.AddScoped<LevelRepository, LevelRepositoryImpl>();
builder.Services.AddScoped<CourseRepository, CourseRepositoryImpl>();
builder.Services.AddScoped<UnitRepository, UnitRepositoryImpl>();
builder.Services.AddScoped<ProgressRepository, ProgressRepositoryImpl>();

var automapperLicense = builder.Configuration["AutoMapper:LicenseKey"] ?? string.Empty;
builder.Services.AddAutoMapper(cfg => { cfg.LicenseKey = automapperLicense; }, typeof(MappingProfile));
builder.Services.AddScoped<QuizRepository, QuizRepositoryImpl>();
builder.Services.AddScoped<PlacementRepository, PlacementRepositoryImpl>();

builder.Services.AddScoped<PartRepository, PartRepositoryImpl>();

builder.Services.AddScoped<PassageRepository, PassageRepositoryImpl>();

builder.Services.AddScoped<QuestionRepository, QuestionRepositoryImpl>();

builder.Services.AddScoped<AnswerRepository, AnswerRepositoryImpl>();

builder.Services.AddScoped<UserAnswerRepository, UserAnswerRepositoryImpl>();

builder.Services.AddScoped<UserQuizRepository, UserQuizRepositoryImpl>();
builder.Services.AddScoped<LearningDashboardRepository, LearningDashboardRepositoryImpl>();
builder.Services.AddScoped<GamificationRepository, GamificationRepositoryImpl>();
builder.Services.AddScoped<AdminCmsRepository, AdminCmsRepositoryImpl>();
builder.Services.AddScoped<AdminCmsService, AdminCmsServiceImpl>();
builder.Services.AddScoped<GamificationService, GamificationServiceImpl>();
builder.Services.AddScoped<LearningDashboardService, LearningDashboardServiceImpl>();
builder.Services.AddScoped<LessonRepository, LessonRepositoryImpl>();
builder.Services.AddScoped<LessonLearningService, LessonLearningServiceImpl>();
builder.Services.AddScoped<VocabularyRepository, VocabularyRepositoryImpl>();
builder.Services.AddScoped<UserVocabularyRepository, UserVocabularyRepositoryImpl>();
builder.Services.AddScoped<VocabularyLearningService, VocabularyLearningServiceImpl>();
builder.Services.AddScoped<QuizAttemptRepository, QuizAttemptRepositoryImpl>();
builder.Services.AddScoped<InteractiveQuizService, InteractiveQuizServiceImpl>();
builder.Services.AddScoped<VideoVocabularyRepository, VideoVocabularyRepositoryImpl>();
builder.Services.AddScoped<VideoLearningService, VideoLearningServiceImpl>();

builder.Services.Configure<SpeakingOptions>(
    builder.Configuration.GetSection(SpeakingOptions.SectionName));
builder.Services.AddHttpClient<OpenAiSpeakingClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});
builder.Services.AddScoped<SpeakingAttemptRepository, SpeakingAttemptRepositoryImpl>();
builder.Services.AddScoped<SpeakingEvaluationService, SpeakingEvaluationServiceImpl>();

builder.Services.Configure<AiTutorOptions>(
    builder.Configuration.GetSection(AiTutorOptions.SectionName));
builder.Services.AddHttpClient<OpenAiTutorCompletionClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});
builder.Services.AddSingleton<StubAiTutorCompletionClient>();
builder.Services.AddScoped<IAiTutorCompletionClient>(sp =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiTutorOptions>>().Value;
    if (string.IsNullOrWhiteSpace(opt.ApiKey))
    {
        return sp.GetRequiredService<StubAiTutorCompletionClient>();
    }

    return sp.GetRequiredService<OpenAiTutorCompletionClient>();
});
builder.Services.AddScoped<TutorConversationRepository, TutorConversationRepositoryImpl>();
builder.Services.AddScoped<AiTutorService, AiTutorServiceImpl>();
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAngular");
app.UseStaticFiles();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();