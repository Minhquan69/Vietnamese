/*
================================================================================
  2026_database_full_setup.sql — Vietnamese learning (SQL Server)
================================================================================
  Mục đích: MỘT file chạy tuần tự các extension + seed (idempotent):
    - Auth (cột Users, RefreshTokens, Roles, chuẩn hóa email, user demo)
    - Vocabulary + UserVocabularies + mẫu từ
    - Lessons (bảng + seed theo Units)
    - Gamification (profile, XpLedger, achievements, badges, daily challenges + seed)
    - Interactive quiz (cột Questions/UserAnswer, QuizAttempts)
    - SpeakingAttempts
    - AI Tutor (TutorConversations, TutorMessages)
    - VideoVocabulary + cột Transcripts.EndTime

  Điều kiện: Database đã có schema CỐT LÕI từ app/EF (ít nhất dbo.Users, dbo.Roles,
  dbo.Levels, dbo.Courses, dbo.Units, dbo.Quizzes, dbo.Questions, dbo.UserAnswer,
  dbo.Videos, dbo.Transcripts, ...). Script này KHÔNG tạo toàn bộ LMS từ đầu.

  Cách chạy: SSMS → chọn đúng database → mở file → Execute (F5).

  User demo sau auth section:
    demo@vietnamese.local / DemoViet2026!
================================================================================
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
  RAISERROR(N'Thiếu dbo.Users. Cần database gốc (schema app) trước khi chạy file này.', 16, 1);
  RETURN;
END
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
  RAISERROR(N'Thiếu dbo.Roles. Cần database gốc (bảng Roles) trước khi chạy file này.', 16, 1);
  RETURN;
END
GO


/* ========== AUTH BOOTSTRAP ========== */
GO


SET NOCOUNT ON;

/* ---- 1) Cột Users + bảng RefreshTokens (giống 2026_auth_extensions.sql) ---- */
IF COL_LENGTH(N'dbo.Users', N'AvatarUrl') IS NULL
  ALTER TABLE dbo.Users ADD AvatarUrl NVARCHAR(512) NULL;

IF COL_LENGTH(N'dbo.Users', N'PasswordResetTokenHash') IS NULL
  ALTER TABLE dbo.Users ADD PasswordResetTokenHash NVARCHAR(128) NULL;

IF COL_LENGTH(N'dbo.Users', N'PasswordResetTokenExpiresUtc') IS NULL
  ALTER TABLE dbo.Users ADD PasswordResetTokenExpiresUtc DATETIME2 NULL;

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.RefreshTokens (
    RefreshTokenId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    TokenHash NVARCHAR(128) NOT NULL,
    ExpiresUtc DATETIME2 NOT NULL,
    RevokedUtc DATETIME2 NULL,
    ReplacedByTokenHash NVARCHAR(128) NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
  );
  CREATE UNIQUE INDEX IX_RefreshTokens_TokenHash ON dbo.RefreshTokens(TokenHash);
  CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
END

/* ---- 2) Roles tối thiểu cho app ---- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'User')
  INSERT INTO dbo.Roles (RoleName) VALUES (N'User');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Admin')
  INSERT INTO dbo.Roles (RoleName) VALUES (N'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Moderator')
  INSERT INTO dbo.Roles (RoleName) VALUES (N'Moderator');

/* ---- 3) Chuẩn hóa email (chữ thường, trim) ---- */
UPDATE dbo.Users
SET Email = LOWER(LTRIM(RTRIM(Email)))
WHERE Email IS NOT NULL
  AND Email <> LOWER(LTRIM(RTRIM(Email)));

/* ---- 4) User demo: insert hoặc cập nhật hash + Status + Role ---- */
DECLARE @UserRoleId INT = (SELECT TOP (1) RoleId FROM dbo.Roles WHERE RoleName = N'User');

IF @UserRoleId IS NULL
BEGIN
  RAISERROR(N'Không tìm thấy role User trong dbo.Roles.', 16, 1);
  RETURN;
END

;MERGE dbo.Users AS T
USING (
  SELECT
    N'Demo learner' AS Name,
    N'demo@vietnamese.local' AS Email,
    N'$2a$11$z8zhEsRYYZ9ZIapoLUkgRO9/mc9GLlOH65bVCOiqfB6fJ0OFF0foW' AS PasswordHash,
    @UserRoleId AS RoleId,
    1 AS Status
) AS S
ON LOWER(LTRIM(RTRIM(T.Email))) = S.Email
WHEN MATCHED THEN
  UPDATE SET
    T.Name = S.Name,
    T.Email = S.Email,
    T.PasswordHash = S.PasswordHash,
    T.RoleId = S.RoleId,
    T.Status = S.Status
WHEN NOT MATCHED BY TARGET THEN
  INSERT (Name, Email, PasswordHash, RoleId, Status)
  VALUES (S.Name, S.Email, S.PasswordHash, S.RoleId, S.Status);

PRINT N'Auth bootstrap xong. Thử đăng nhập: demo@vietnamese.local / DemoViet2026!';

/*
  Kiểm tra user ↔ role (RoleId mồ côi thường gây 401 dù đúng mật khẩu):

  SELECT u.UserId, u.Email, u.RoleId, r.RoleName
  FROM dbo.Users u
  LEFT JOIN dbo.Roles r ON r.RoleId = u.RoleId
  WHERE u.Email = N'demo@vietnamese.local';

  Sửa RoleId mồ côi (chạy nếu RoleName ở truy vấn trên là NULL):

  UPDATE u
  SET u.RoleId = (SELECT TOP (1) RoleId FROM dbo.Roles WHERE RoleName = N'User')
  FROM dbo.Users u
  WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.RoleId = u.RoleId);
*/
GO

/* ========== VOCABULARY ========== */
GO

/*
 * Canonical vocabulary + per-user SRS / saved state (UserVocabulary).
 * Run on VietnameseLearningPlatform (or your API database).
 */

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vocabularies')
BEGIN
    CREATE TABLE dbo.Vocabularies (
        VocabularyId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        Word NVARCHAR(128) NOT NULL,
        Ipa NVARCHAR(256) NULL,
        AudioUrl NVARCHAR(1024) NULL,
        MeaningEn NVARCHAR(500) NULL,
        PartOfSpeech NVARCHAR(64) NULL,
        ExampleSentence NVARCHAR(500) NULL,
        ExampleTranslation NVARCHAR(500) NULL,
        ContextNote NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Vocabularies_Active DEFAULT (1),
        CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_Vocabularies_Created DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_Vocabularies_Word ON dbo.Vocabularies (Word);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserVocabularies')
BEGIN
    CREATE TABLE dbo.UserVocabularies (
        UserVocabularyId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        VocabularyId INT NOT NULL,
        Saved BIT NOT NULL CONSTRAINT DF_UserVocab_Saved DEFAULT (0),
        EaseFactor DECIMAL(5, 2) NOT NULL CONSTRAINT DF_UserVocab_Ease DEFAULT (2.50),
        IntervalDays INT NOT NULL CONSTRAINT DF_UserVocab_Interval DEFAULT (0),
        Repetitions INT NOT NULL CONSTRAINT DF_UserVocab_Rep DEFAULT (0),
        NextReviewUtc DATETIME2 NOT NULL CONSTRAINT DF_UserVocab_Next DEFAULT ('1970-01-01'),
        LastReviewedUtc DATETIME2 NULL,
        MasteryScore DECIMAL(5, 2) NOT NULL CONSTRAINT DF_UserVocab_Mastery DEFAULT (0),
        Familiarity TINYINT NOT NULL CONSTRAINT DF_UserVocab_Fam DEFAULT (0),
        CONSTRAINT FK_UserVocab_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_UserVocab_Vocab FOREIGN KEY (VocabularyId) REFERENCES dbo.Vocabularies(VocabularyId) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_UserVocab_User_Vocab ON dbo.UserVocabularies (UserId, VocabularyId);
    CREATE INDEX IX_UserVocab_NextReview ON dbo.UserVocabularies (UserId, NextReviewUtc);
END
GO

/* Seed sample entries if table was just created / empty */
IF NOT EXISTS (SELECT 1 FROM dbo.Vocabularies)
BEGIN
    INSERT INTO dbo.Vocabularies (Word, Ipa, MeaningEn, PartOfSpeech, ExampleSentence, ExampleTranslation, ContextNote)
    VALUES
    (N'xin chào', N'/sin tɕàː/', N'hello', N'phrase', N'Xin chào, bạn khỏe không?', N'Hello, how are you?', N'Greeting'),
    (N'cảm ơn', N'/kam əːn/', N'thank you', N'phrase', N'Cảm ơn rất nhiều.', N'Thank you very much.', N'Politeness'),
    (N'học', N'/hə̆k/', N'to study', N'verb', N'Tôi học tiếng Việt.', N'I study Vietnamese.', N'Education'),
    (N'đẹp', N'/ɗɛ̆p/', N'beautiful', N'adj', N'Cảnh rất đẹp.', N'The scenery is very beautiful.', N'Description'),
    (N'ăn', N'/ăːn/', N'to eat', N'verb', N'Bạn đã ăn cơm chưa?', N'Have you eaten yet?', N'Daily life');
END
GO

/* ========== LESSONS ========== */
GO

/*
 * Lessons table + seed rows (six lesson types per unit).
 * Run against VietnameseLearningPlatform (or your API database) after deploying the backend that includes the Lesson entity.
 *
 * Nếu bảng Lessons đã tồn tại từ DB cũ (thiếu Title, Summary, ...), script sẽ ALTER thêm cột trước khi seed.
 */

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Lessons')
BEGIN
    CREATE TABLE dbo.Lessons (
        LessonId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UnitId INT NOT NULL,
        LessonType NVARCHAR(32) NOT NULL,
        Title NVARCHAR(256) NOT NULL,
        Summary NVARCHAR(500) NULL,
        OrderIndex INT NOT NULL,
        DurationMinutes INT NOT NULL CONSTRAINT DF_Lessons_Duration DEFAULT (8),
        ContentJson NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Lessons_Active DEFAULT (1),
        CONSTRAINT FK_Lessons_Units FOREIGN KEY (UnitId) REFERENCES dbo.Units(UnitId) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_Lessons_Unit_Order ON dbo.Lessons (UnitId, OrderIndex);
END
GO

/* --- Đồng bộ bảng Lessons cũ với model app (chỉ thêm cột nếu thiếu) --- */
IF OBJECT_ID(N'dbo.Lessons', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Lessons', N'Title') IS NULL
    BEGIN
        ALTER TABLE dbo.Lessons ADD Title NVARCHAR(256) NOT NULL
            CONSTRAINT DF_Lessons_TitleMigr DEFAULT (N'');
        ALTER TABLE dbo.Lessons DROP CONSTRAINT DF_Lessons_TitleMigr;
    END;

    IF COL_LENGTH(N'dbo.Lessons', N'Summary') IS NULL
        ALTER TABLE dbo.Lessons ADD Summary NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Lessons', N'ContentJson') IS NULL
        ALTER TABLE dbo.Lessons ADD ContentJson NVARCHAR(MAX) NULL;

    IF COL_LENGTH(N'dbo.Lessons', N'IsActive') IS NULL
    BEGIN
        ALTER TABLE dbo.Lessons ADD IsActive BIT NOT NULL
            CONSTRAINT DF_Lessons_IsActiveMigr DEFAULT (1);
        ALTER TABLE dbo.Lessons DROP CONSTRAINT DF_Lessons_IsActiveMigr;
    END;

    IF COL_LENGTH(N'dbo.Lessons', N'DurationMinutes') IS NULL
    BEGIN
        ALTER TABLE dbo.Lessons ADD DurationMinutes INT NOT NULL
            CONSTRAINT DF_Lessons_DurMigr DEFAULT (8);
        ALTER TABLE dbo.Lessons DROP CONSTRAINT DF_Lessons_DurMigr;
    END;

    IF COL_LENGTH(N'dbo.Lessons', N'LessonType') IS NULL
    BEGIN
        ALTER TABLE dbo.Lessons ADD LessonType NVARCHAR(32) NOT NULL
            CONSTRAINT DF_Lessons_TypeMigr DEFAULT (N'vocabulary');
        ALTER TABLE dbo.Lessons DROP CONSTRAINT DF_Lessons_TypeMigr;
    END;

    IF COL_LENGTH(N'dbo.Lessons', N'OrderIndex') IS NULL
    BEGIN
        ALTER TABLE dbo.Lessons ADD OrderIndex INT NOT NULL
            CONSTRAINT DF_Lessons_OrdMigr DEFAULT (0);
        ALTER TABLE dbo.Lessons DROP CONSTRAINT DF_Lessons_OrdMigr;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes i
        WHERE i.object_id = OBJECT_ID(N'dbo.Lessons')
          AND i.name = N'UX_Lessons_Unit_Order')
    BEGIN
        BEGIN TRY
            CREATE UNIQUE INDEX UX_Lessons_Unit_Order ON dbo.Lessons (UnitId, OrderIndex);
        END TRY
        BEGIN CATCH
            PRINT N'[Lessons] Bỏ qua tạo UX_Lessons_Unit_Order (có thể trùng UnitId+OrderIndex trong dữ liệu cũ).';
        END CATCH
    END
END
GO

/* Cột legacy LessonTitle (NOT NULL): INSERT phải gán — trùng giá trị Title */
IF OBJECT_ID(N'dbo.Lessons', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Lessons', N'LessonTitle') IS NOT NULL
   AND COL_LENGTH(N'dbo.Lessons', N'Title') IS NOT NULL
BEGIN
    UPDATE dbo.Lessons
    SET Title = LessonTitle
    WHERE (Title IS NULL OR Title = N'') AND NULLIF(LessonTitle, N'') IS NOT NULL;

    UPDATE dbo.Lessons
    SET LessonTitle = Title
    WHERE NULLIF(LessonTitle, N'') IS NULL AND NULLIF(Title, N'') IS NOT NULL;
END
GO

IF COL_LENGTH(N'dbo.Lessons', N'LessonTitle') IS NOT NULL
BEGIN
    ;WITH slot (Ord, LType, TitleSuffix) AS (
        SELECT * FROM (VALUES
            (1, N'vocabulary', N'Vocabulary'),
            (2, N'grammar', N'Grammar'),
            (3, N'listening', N'Listening'),
            (4, N'speaking', N'Speaking'),
            (5, N'reading', N'Reading'),
            (6, N'video', N'Video')
        ) AS x (Ord, LType, TitleSuffix)
    )
    INSERT INTO dbo.Lessons (UnitId, LessonType, Title, LessonTitle, Summary, OrderIndex, DurationMinutes, ContentJson, IsActive)
    SELECT
        u.UnitId,
        slot.LType,
        u.UnitName + N' — ' + slot.TitleSuffix,
        u.UnitName + N' — ' + slot.TitleSuffix,
        NULL,
        slot.Ord,
        CASE WHEN slot.LType = N'video' THEN 12 ELSE 10 END,
        NULL,
        1
    FROM dbo.Units u
    CROSS JOIN slot
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Lessons l WHERE l.UnitId = u.UnitId AND l.OrderIndex = slot.Ord
    );
END
ELSE
BEGIN
    ;WITH slot (Ord, LType, TitleSuffix) AS (
        SELECT * FROM (VALUES
            (1, N'vocabulary', N'Vocabulary'),
            (2, N'grammar', N'Grammar'),
            (3, N'listening', N'Listening'),
            (4, N'speaking', N'Speaking'),
            (5, N'reading', N'Reading'),
            (6, N'video', N'Video')
        ) AS x (Ord, LType, TitleSuffix)
    )
    INSERT INTO dbo.Lessons (UnitId, LessonType, Title, Summary, OrderIndex, DurationMinutes, ContentJson, IsActive)
    SELECT
        u.UnitId,
        slot.LType,
        u.UnitName + N' — ' + slot.TitleSuffix,
        NULL,
        slot.Ord,
        CASE WHEN slot.LType = N'video' THEN 12 ELSE 10 END,
        NULL,
        1
    FROM dbo.Units u
    CROSS JOIN slot
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Lessons l WHERE l.UnitId = u.UnitId AND l.OrderIndex = slot.Ord
    );
END
GO

/* ========== GAMIFICATION ========== */
GO

/* Gamification: XP ledger, profile, streak, achievements, badges, daily challenges, leaderboard data */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.UserGamificationProfiles', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.UserGamificationProfiles (
    UserId INT NOT NULL PRIMARY KEY,
    TotalXp INT NOT NULL CONSTRAINT DF_GmProf_Total DEFAULT (0),
    DisplayLevel INT NOT NULL CONSTRAINT DF_GmProf_Lvl DEFAULT (1),
    CurrentStreak INT NOT NULL CONSTRAINT DF_GmProf_Streak DEFAULT (0),
    LongestStreak INT NOT NULL CONSTRAINT DF_GmProf_LongStreak DEFAULT (0),
    LastActivityDate DATE NULL,
    LegacyXpImported BIT NOT NULL CONSTRAINT DF_GmProf_Legacy DEFAULT (0),
    CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_GmProf_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedUtc DATETIME2 NOT NULL CONSTRAINT DF_GmProf_Updated DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_GmProf_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
  );
END
GO

IF OBJECT_ID(N'dbo.XpLedger', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.XpLedger (
    XpLedgerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    Amount INT NOT NULL,
    Source NVARCHAR(40) NOT NULL,
    RefKey NVARCHAR(80) NULL,
    CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_XpLedger_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_XpLedger_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
  );
  CREATE INDEX IX_XpLedger_User_Created ON dbo.XpLedger(UserId, CreatedUtc DESC);
END
GO

IF OBJECT_ID(N'dbo.AchievementDefinitions', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.AchievementDefinitions (
    AchievementDefinitionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Code NVARCHAR(64) NOT NULL,
    Title NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    IconKey NVARCHAR(32) NOT NULL CONSTRAINT DF_AchDef_Icon DEFAULT (N'star'),
    RuleType NVARCHAR(40) NOT NULL,
    RuleThreshold INT NOT NULL,
    XpReward INT NOT NULL CONSTRAINT DF_AchDef_Xp DEFAULT (0),
    CONSTRAINT UQ_AchievementDefinitions_Code UNIQUE (Code)
  );
END
GO

IF OBJECT_ID(N'dbo.UserAchievements', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.UserAchievements (
    UserId INT NOT NULL,
    AchievementDefinitionId INT NOT NULL,
    UnlockedUtc DATETIME2 NOT NULL CONSTRAINT DF_UserAch_Unl DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_UserAchievements PRIMARY KEY (UserId, AchievementDefinitionId),
    CONSTRAINT FK_UserAch_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserAch_Def FOREIGN KEY (AchievementDefinitionId) REFERENCES dbo.AchievementDefinitions(AchievementDefinitionId) ON DELETE CASCADE
  );
END
GO

IF OBJECT_ID(N'dbo.BadgeDefinitions', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.BadgeDefinitions (
    BadgeDefinitionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Code NVARCHAR(64) NOT NULL,
    Title NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Tier TINYINT NOT NULL CONSTRAINT DF_BadgeDef_Tier DEFAULT (1),
    RuleType NVARCHAR(40) NOT NULL,
    RuleThreshold INT NOT NULL,
    CONSTRAINT UQ_BadgeDefinitions_Code UNIQUE (Code)
  );
END
GO

IF OBJECT_ID(N'dbo.UserBadges', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.UserBadges (
    UserId INT NOT NULL,
    BadgeDefinitionId INT NOT NULL,
    EarnedUtc DATETIME2 NOT NULL CONSTRAINT DF_UserBadge_Earned DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_UserBadges PRIMARY KEY (UserId, BadgeDefinitionId),
    CONSTRAINT FK_UserBadges_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserBadges_Def FOREIGN KEY (BadgeDefinitionId) REFERENCES dbo.BadgeDefinitions(BadgeDefinitionId) ON DELETE CASCADE
  );
END
GO

IF OBJECT_ID(N'dbo.DailyChallengeDefinitions', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.DailyChallengeDefinitions (
    DailyChallengeDefinitionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Code NVARCHAR(64) NOT NULL,
    Title NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    TargetKind NVARCHAR(40) NOT NULL,
    TargetValue INT NOT NULL,
    XpReward INT NOT NULL CONSTRAINT DF_DailyDef_Xp DEFAULT (15),
    SortOrder INT NOT NULL CONSTRAINT DF_DailyDef_Sort DEFAULT (0),
    CONSTRAINT UQ_DailyChallengeDefinitions_Code UNIQUE (Code)
  );
END
GO

IF OBJECT_ID(N'dbo.UserDailyChallenges', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.UserDailyChallenges (
    UserDailyChallengeId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    ChallengeDate DATE NOT NULL,
    DailyChallengeDefinitionId INT NOT NULL,
    Progress INT NOT NULL CONSTRAINT DF_UserDaily_Prog DEFAULT (0),
    CompletedUtc DATETIME2 NULL,
    CONSTRAINT FK_UserDaily_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserDaily_Def FOREIGN KEY (DailyChallengeDefinitionId) REFERENCES dbo.DailyChallengeDefinitions(DailyChallengeDefinitionId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserDaily_User_Date_Def UNIQUE (UserId, ChallengeDate, DailyChallengeDefinitionId)
  );
  CREATE INDEX IX_UserDaily_User_Date ON dbo.UserDailyChallenges(UserId, ChallengeDate);
END
GO

/* Seed definitions (idempotent) */
IF NOT EXISTS (SELECT 1 FROM dbo.AchievementDefinitions WHERE Code = N'first_quiz')
INSERT INTO dbo.AchievementDefinitions (Code, Title, Description, IconKey, RuleType, RuleThreshold, XpReward)
VALUES
(N'first_quiz', N'First steps', N'Pass your first quiz.', N'target', N'quizzes_passed', 1, 20),
(N'xp_500', N'Rising star', N'Reach 500 lifetime XP.', N'star', N'total_xp', 500, 40),
(N'streak_7', N'Week warrior', N'Maintain a 7-day learning streak.', N'flame', N'streak_days', 7, 50),
(N'units_5', N'Path builder', N'Complete 5 units.', N'brick', N'units_completed', 5, 45),
(N'quizzes_25', N'Quiz fan', N'Pass 25 quizzes.', N'book', N'quizzes_passed', 25, 60),
(N'speaking_10', N'Voice explorer', N'Complete 10 speaking evaluations.', N'mic', N'speaking_attempts', 10, 55),
(N'lessons_20', N'Lesson sprinter', N'Complete 20 lessons.', N'bolt', N'lessons_completed', 20, 35);

IF NOT EXISTS (SELECT 1 FROM dbo.BadgeDefinitions WHERE Code = N'badge_bronze_xp')
INSERT INTO dbo.BadgeDefinitions (Code, Title, Description, Tier, RuleType, RuleThreshold)
VALUES
(N'badge_bronze_xp', N'Bronze scholar', N'Earn 300 lifetime XP.', 1, N'total_xp', 300),
(N'badge_silver_xp', N'Silver scholar', N'Earn 1,500 lifetime XP.', 2, N'total_xp', 1500),
(N'badge_gold_xp', N'Gold scholar', N'Earn 5,000 lifetime XP.', 3, N'total_xp', 5000),
(N'badge_streak', N'Streak keeper', N'Reach a 14-day streak.', 2, N'streak_days', 14),
(N'badge_polyglot', N'Polyglot spark', N'Unlock 4 achievements.', 2, N'achievements_unlocked', 4);

IF NOT EXISTS (SELECT 1 FROM dbo.DailyChallengeDefinitions WHERE Code = N'daily_quiz')
INSERT INTO dbo.DailyChallengeDefinitions (Code, Title, Description, TargetKind, TargetValue, XpReward, SortOrder)
VALUES
(N'daily_quiz', N'Quiz champion', N'Pass at least one quiz today.', N'quizzes_today', 1, 25, 1),
(N'daily_xp', N'XP sprint', N'Earn 50 XP from rewarded activities today.', N'xp_today', 50, 30, 2),
(N'daily_lesson', N'Lesson focus', N'Complete 2 lessons today.', N'lessons_today', 2, 20, 3);
GO

/* ========== INTERACTIVE QUIZ ========== */
GO

/* Interactive quiz engine: question types, explanations, payloads, attempt history */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.Questions', N'QuestionType') IS NULL
BEGIN
  ALTER TABLE dbo.Questions ADD QuestionType NVARCHAR(32) NOT NULL
    CONSTRAINT DF_Questions_QuestionType DEFAULT (N'MultipleChoice');
END
GO

IF COL_LENGTH(N'dbo.Questions', N'Explanation') IS NULL
BEGIN
  ALTER TABLE dbo.Questions ADD Explanation NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH(N'dbo.Questions', N'InteractivePayload') IS NULL
BEGIN
  ALTER TABLE dbo.Questions ADD InteractivePayload NVARCHAR(MAX) NULL;
END
GO

/* Allow text / structured responses without a selected Answer row */
IF OBJECT_ID(N'dbo.UserAnswer', N'U') IS NOT NULL
BEGIN
  IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.UserAnswer')
      AND name = N'AnswerId'
      AND is_nullable = 0)
  BEGIN
    ALTER TABLE dbo.UserAnswer ALTER COLUMN AnswerId INT NULL;
  END
END
GO

IF COL_LENGTH(N'dbo.UserAnswer', N'ResponsePayload') IS NULL
BEGIN
  ALTER TABLE dbo.UserAnswer ADD ResponsePayload NVARCHAR(MAX) NULL;
END
GO

IF OBJECT_ID(N'dbo.QuizAttempts', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.QuizAttempts (
    QuizAttemptId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    QuizId INT NOT NULL,
    ScorePercent DECIMAL(6,2) NOT NULL,
    DurationSeconds INT NOT NULL CONSTRAINT DF_QuizAttempts_Dur DEFAULT (0),
    CorrectCount INT NOT NULL CONSTRAINT DF_QuizAttempts_Corr DEFAULT (0),
    WrongCount INT NOT NULL CONSTRAINT DF_QuizAttempts_Wrong DEFAULT (0),
    SkippedCount INT NOT NULL CONSTRAINT DF_QuizAttempts_Skip DEFAULT (0),
    SubmittedUtc DATETIME2 NOT NULL CONSTRAINT DF_QuizAttempts_Sub DEFAULT (SYSUTCDATETIME()),
    DetailJson NVARCHAR(MAX) NULL,
    CONSTRAINT FK_QuizAttempts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_QuizAttempts_Quizzes FOREIGN KEY (QuizId) REFERENCES dbo.Quizzes(QuizId) ON DELETE CASCADE
  );
  CREATE INDEX IX_QuizAttempts_User_Quiz ON dbo.QuizAttempts(UserId, QuizId);
END
GO

/* ========== SPEAKING AI ========== */
GO

/* Speaking AI: uploads + transcript + scored evaluation history */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.SpeakingAttempts', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.SpeakingAttempts (
    SpeakingAttemptId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    ReferenceText NVARCHAR(500) NULL,
    Transcript NVARCHAR(MAX) NOT NULL CONSTRAINT DF_Speaking_Trans DEFAULT (N''),
    AudioRelativePath NVARCHAR(512) NULL,
    DurationMs INT NOT NULL CONSTRAINT DF_Speaking_Dur DEFAULT (0),
    PronunciationScore DECIMAL(5,2) NOT NULL CONSTRAINT DF_Speaking_Pron DEFAULT (0),
    FluencyScore DECIMAL(5,2) NOT NULL CONSTRAINT DF_Speaking_Flu DEFAULT (0),
    ToneScore DECIMAL(5,2) NOT NULL CONSTRAINT DF_Speaking_Tone DEFAULT (0),
    OverallScore DECIMAL(5,2) NOT NULL CONSTRAINT DF_Speaking_Ovr DEFAULT (0),
    Feedback NVARCHAR(MAX) NULL,
    AnalyticsJson NVARCHAR(MAX) NULL,
    CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_Speaking_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Speaking_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
  );
  CREATE INDEX IX_Speaking_User_Created ON dbo.SpeakingAttempts(UserId, CreatedUtc DESC);
END
GO

/* ========== AI TUTOR ========== */
GO

/* AI tutor: per-user conversations and message history */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.TutorConversations', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.TutorConversations (
    TutorConversationId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL CONSTRAINT DF_TutorConv_Title DEFAULT (N'Chat'),
    ScenarioKey NVARCHAR(64) NULL,
    CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_TutorConv_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedUtc DATETIME2 NOT NULL CONSTRAINT DF_TutorConv_Updated DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TutorConv_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
  );
  CREATE INDEX IX_TutorConv_User_Updated ON dbo.TutorConversations(UserId, UpdatedUtc DESC);
END
GO

IF OBJECT_ID(N'dbo.TutorMessages', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.TutorMessages (
    TutorMessageId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TutorConversationId INT NOT NULL,
    Role NVARCHAR(16) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_TutorMsg_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TutorMsg_Conv FOREIGN KEY (TutorConversationId) REFERENCES dbo.TutorConversations(TutorConversationId) ON DELETE CASCADE
  );
  CREATE INDEX IX_TutorMsg_Conv_Created ON dbo.TutorMessages(TutorConversationId, CreatedUtc);
END
GO

/* ========== VIDEO VOCABULARY ========== */
GO

/* Links dictionary entries to videos (and optional transcript cues) for contextual learning */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.Transcripts', N'EndTime') IS NULL
BEGIN
  ALTER TABLE dbo.Transcripts ADD EndTime FLOAT NULL;
END
GO

IF OBJECT_ID(N'dbo.VideoVocabularies', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.VideoVocabularies (
    VideoVocabularyId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VideoId INT NOT NULL,
    VocabularyId INT NOT NULL,
    TranscriptId INT NULL,
    ContextSnippet NVARCHAR(500) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_VideoVocab_Sort DEFAULT (0),
    CONSTRAINT FK_VideoVocab_Videos FOREIGN KEY (VideoId) REFERENCES dbo.Videos(VideoId) ON DELETE CASCADE,
    CONSTRAINT FK_VideoVocab_Vocab FOREIGN KEY (VocabularyId) REFERENCES dbo.Vocabularies(VocabularyId) ON DELETE CASCADE,
    CONSTRAINT FK_VideoVocab_Transcripts FOREIGN KEY (TranscriptId) REFERENCES dbo.Transcripts(TranscriptId) ON DELETE SET NULL,
    CONSTRAINT UX_VideoVocab_Video_Vocab UNIQUE (VideoId, VocabularyId)
  );
  CREATE INDEX IX_VideoVocab_Video ON dbo.VideoVocabularies(VideoId);
END
GO

/* ========== DONE ========== */
PRINT N'2026_database_full_setup: hoàn tất.';
GO

