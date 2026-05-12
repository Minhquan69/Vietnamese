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
