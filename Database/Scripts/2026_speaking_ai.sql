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
