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
