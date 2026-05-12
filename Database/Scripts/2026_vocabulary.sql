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
