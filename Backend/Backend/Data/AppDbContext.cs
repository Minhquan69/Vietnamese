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
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        public DbSet<UserProgress> UserProgress { get; set; }
        public DbSet<UserQuiz> UserQuiz { get; set; }
        public DbSet<UserCourse> UserCourse { get; set; }
        public DbSet<UserLevel> UserLevel { get; set; }
        public DbSet<UserFavorite> UserFavorite { get; set; }



    }
}