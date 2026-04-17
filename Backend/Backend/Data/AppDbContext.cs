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
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        public DbSet<UserProgress> UserProgress { get; set; }
        public DbSet<UserQuiz> UserQuiz { get; set; }
        public DbSet<UserCourse> UserCourse { get; set; }
        public DbSet<UserLevel> UserLevel { get; set; }
        public DbSet<UserFavorite> UserFavorite { get; set; }



        /* * Cấu hình các ràng buộc Cascade Delete 
         * thuphuong21072004 
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Unit)
                .WithOne(u => u.Quiz)
                .HasForeignKey<Quiz>(q => q.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<UserLevel>()
                .HasOne(x => x.Level)
                .WithMany(l => l.UserLevels)
                .HasForeignKey(x => x.LevelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCourse>()
                .HasOne(x => x.Course)
                .WithMany(c => c.UserCourses)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProgress>()
                .HasOne(x => x.Unit)
                .WithMany(u => u.UserProgresses)
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuiz>()
                .HasOne(x => x.Quiz)
                .WithMany(q => q.UserQuizzes)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(x => x.Unit)
                .WithMany(u => u.UserFavorites)
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}