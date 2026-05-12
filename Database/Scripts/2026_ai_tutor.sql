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
