using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Video> Videos { get; set; }

        public DbSet<Transcript> Transcripts { get; set; }


        public DbSet<Level> Levels { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Passage> Passages { get;set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        public DbSet<UserProgress> UserProgress { get; set; }
        public DbSet<UserQuiz> UserQuiz { get; set; }
        
        public DbSet<UserAnswer> UserAnswer { get; set; }
        public DbSet<PlacementTest> PlacementTests { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Vocabulary> Vocabularies { get; set; }
        public DbSet<UserVocabulary> UserVocabularies { get; set; }

        public DbSet<QuizAttempt> QuizAttempts { get; set; }

        public DbSet<VideoVocabulary> VideoVocabularies { get; set; }

        public DbSet<TutorConversation> TutorConversations { get; set; }

        public DbSet<TutorMessage> TutorMessages { get; set; }

        public DbSet<SpeakingAttempt> SpeakingAttempts { get; set; }

        public DbSet<UserGamificationProfile> UserGamificationProfiles { get; set; }

        public DbSet<XpLedgerEntry> XpLedger { get; set; }

        public DbSet<AchievementDefinition> AchievementDefinitions { get; set; }

        public DbSet<UserAchievement> UserAchievements { get; set; }

        public DbSet<BadgeDefinition> BadgeDefinitions { get; set; }

        public DbSet<UserBadge> UserBadges { get; set; }

        public DbSet<DailyChallengeDefinition> DailyChallengeDefinitions { get; set; }

        public DbSet<UserDailyChallenge> UserDailyChallenges { get; set; }

        /* * Cấu hình các ràng buộc Cascade Delete 
         * thuphuong21072004 
         */
        protected override void OnModelCreating(
    ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Question>()
                .Property(x => x.Score)
                .HasPrecision(9, 4);

            modelBuilder.Entity<Quiz>()
                .Property(x => x.PassScore)
                .HasPrecision(9, 4);

            modelBuilder.Entity<UserQuiz>()
                .Property(x => x.Score)
                .HasPrecision(9, 4);

            modelBuilder.Entity<QuizAttempt>()
                .Property(x => x.ScorePercent)
                .HasPrecision(6, 2);

            modelBuilder.Entity<SpeakingAttempt>()
                .Property(x => x.PronunciationScore)
                .HasPrecision(5, 2);
            modelBuilder.Entity<SpeakingAttempt>()
                .Property(x => x.FluencyScore)
                .HasPrecision(5, 2);
            modelBuilder.Entity<SpeakingAttempt>()
                .Property(x => x.ToneScore)
                .HasPrecision(5, 2);
            modelBuilder.Entity<SpeakingAttempt>()
                .Property(x => x.OverallScore)
                .HasPrecision(5, 2);

            modelBuilder.Entity<UserVocabulary>()
                .Property(x => x.EaseFactor)
                .HasPrecision(5, 2);
            modelBuilder.Entity<UserVocabulary>()
                .Property(x => x.MasteryScore)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Level)
                .WithMany(l => l.Courses)
                .HasForeignKey(c => c.LevelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Unit>()
                .HasOne(u => u.Course)
                .WithMany(c => c.Units)
                .HasForeignKey(u => u.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Unit)
                .WithMany(u => u.Lessons)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lesson>()
                .HasIndex(x => new { x.UnitId, x.OrderIndex })
                .IsUnique();

            modelBuilder.Entity<Part>()
                .HasOne(p => p.Quiz)
                .WithMany(q => q.Parts)
                .HasForeignKey(p => p.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Passage>()
                .HasOne(p => p.Part)
                .WithMany(p => p.Passages)
                .HasForeignKey(p => p.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Part)
                .WithMany(p => p.Questions)
                .HasForeignKey(q => q.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Passage)
                .WithMany(p => p.Questions)
                .HasForeignKey(q => q.PassageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transcript>()
                .HasOne(t => t.Video)
                .WithMany(v => v.Transcripts)
                .HasForeignKey(t => t.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuiz>()
                .HasOne(x => x.User)
                .WithMany(u => u.UserQuizzes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuiz>()
                .HasOne(x => x.Quiz)
                .WithMany(q => q.UserQuizzes)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.UserQuiz)
                .WithMany(uq => uq.UserAnswers)
                .HasForeignKey(ua => ua.UserQuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Answer)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(ua => ua.AnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Video>()
                .HasIndex(v => v.YoutubeId)
                .IsUnique();

            modelBuilder.Entity<Level>()
                .HasIndex(l => l.OrderIndex)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(x => new { x.LevelId, x.OrderIndex })
                .IsUnique();

            modelBuilder.Entity<Unit>()
                .HasIndex(x => new { x.CourseId, x.OrderIndex })
                .IsUnique();

            modelBuilder.Entity<UserQuiz>()
                .HasIndex(x => new { x.UserId, x.QuizId })
                .IsUnique();

            modelBuilder.Entity<UserProgress>()
                .HasIndex(x => new { x.UserId, x.RefType, x.RefId })
                .IsUnique();

            modelBuilder.Entity<Quiz>()
                .HasIndex(x => new { x.RefType, x.RefId })
                .IsUnique();

            modelBuilder.Entity<UserAnswer>()
                .HasIndex(x => new { x.UserQuizId, x.QuestionId })
                .IsUnique();

            modelBuilder.Entity<Answer>()
                .HasIndex(x => new { x.QuestionId, x.OrderIndex })
                .IsUnique();

            modelBuilder.Entity<Part>()
                .HasIndex(x => new { x.QuizId, x.PartNumber })
                .IsUnique();

            modelBuilder.Entity<Passage>()
                .HasIndex(x => new { x.PartId, x.OrderIndex })
                .IsUnique();

            modelBuilder.Entity<Quiz>()
                .Property(x => x.CreatedDate)
                .HasDefaultValueSql(
                    "GETDATE()");

            modelBuilder.Entity<PlacementTest>()
                .HasKey(x => x.PlacementId);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.TokenHash)
                .IsUnique();

            modelBuilder.Entity<UserVocabulary>()
                .HasOne(uv => uv.User)
                .WithMany(u => u.UserVocabularies)
                .HasForeignKey(uv => uv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserVocabulary>()
                .HasOne(uv => uv.Vocabulary)
                .WithMany(v => v.UserVocabularies)
                .HasForeignKey(uv => uv.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserVocabulary>()
                .HasIndex(x => new { x.UserId, x.VocabularyId })
                .IsUnique();

            modelBuilder.Entity<Vocabulary>()
                .HasIndex(x => x.Word)
                .IsUnique();

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(x => x.Quiz)
                .WithMany()
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoVocabulary>()
                .HasOne(x => x.Video)
                .WithMany(v => v.VideoVocabularies)
                .HasForeignKey(x => x.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoVocabulary>()
                .HasOne(x => x.Vocabulary)
                .WithMany(v => v.VideoVocabularies)
                .HasForeignKey(x => x.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoVocabulary>()
                .HasOne(x => x.Transcript)
                .WithMany()
                .HasForeignKey(x => x.TranscriptId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VideoVocabulary>()
                .HasIndex(x => new { x.VideoId, x.VocabularyId })
                .IsUnique();

            modelBuilder.Entity<TutorConversation>()
                .HasOne(c => c.User)
                .WithMany(u => u.TutorConversations)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TutorMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.TutorConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SpeakingAttempt>()
                .HasOne(x => x.User)
                .WithMany(u => u.SpeakingAttempts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserGamificationProfile>()
                .HasKey(x => x.UserId);

            modelBuilder.Entity<UserGamificationProfile>()
                .HasOne(x => x.User)
                .WithOne(u => u.GamificationProfile)
                .HasForeignKey<UserGamificationProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<XpLedgerEntry>()
                .HasKey(x => x.XpLedgerId);

            modelBuilder.Entity<XpLedgerEntry>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAchievement>()
                .HasKey(x => new { x.UserId, x.AchievementDefinitionId });

            modelBuilder.Entity<UserAchievement>()
                .HasOne(x => x.Definition)
                .WithMany(d => d.UserAchievements)
                .HasForeignKey(x => x.AchievementDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAchievement>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBadge>()
                .HasKey(x => new { x.UserId, x.BadgeDefinitionId });

            modelBuilder.Entity<UserBadge>()
                .HasOne(x => x.Definition)
                .WithMany(d => d.UserBadges)
                .HasForeignKey(x => x.BadgeDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBadge>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDailyChallenge>()
                .HasIndex(x => new { x.UserId, x.ChallengeDate, x.DailyChallengeDefinitionId })
                .IsUnique();

            modelBuilder.Entity<UserDailyChallenge>()
                .Property(x => x.ChallengeDate)
                .HasColumnType("date");

            modelBuilder.Entity<UserGamificationProfile>()
                .Property(x => x.LastActivityDate)
                .HasColumnType("date");

            modelBuilder.Entity<UserDailyChallenge>()
                .HasOne(x => x.Definition)
                .WithMany(d => d.UserDailyChallenges)
                .HasForeignKey(x => x.DailyChallengeDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDailyChallenge>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
