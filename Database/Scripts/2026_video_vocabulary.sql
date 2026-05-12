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
