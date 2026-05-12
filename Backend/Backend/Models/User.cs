namespace Backend.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public int Status { get; set; }

        public string? AvatarUrl { get; set; }
        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiresUtc { get; set; }

        public Role? Role { get; set; }

        public ICollection<UserQuiz>? UserQuizzes { get; set; }
        public ICollection<UserProgress>? UserProgresses { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }

        public ICollection<UserVocabulary>? UserVocabularies { get; set; }

        public ICollection<TutorConversation>? TutorConversations { get; set; }

        public ICollection<SpeakingAttempt>? SpeakingAttempts { get; set; }

        public UserGamificationProfile? GamificationProfile { get; set; }
    }
}

