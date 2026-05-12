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
