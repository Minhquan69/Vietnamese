viecreate database vietnamese
use vietnamese

CREATE TABLE Roles(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100)
);
CREATE TABLE Users(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    Password NVARCHAR(255),
    RoleId INT,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
CREATE TABLE Videos(
    VideoId INT IDENTITY(1,1) PRIMARY KEY,
    YoutubeId NVARCHAR(50),
    Title NVARCHAR(200),
    CreatedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
CREATE TABLE Transcripts(
    TranscriptId INT IDENTITY(1,1) PRIMARY KEY,
    VideoId INT,
    Sentence NVARCHAR(MAX),
    StartTime FLOAT,
    FOREIGN KEY (VideoId) REFERENCES Videos(VideoId)
);

INSERT INTO Roles(RoleName)
VALUES
( N'Admin'),
( N'User'),
( N'Moderator');
INSERT INTO Users(Name, Email, Password, RoleId)
VALUES
( N'Phạm Thu Phương', 'admin@gmail.com', '123456', 1),
( N'Tran Thi B', 'user1@gmail.com', '123456', 2),
( N'Le Van C', 'user2@gmail.com', '123456', 2),
( N'Pham Thi D', 'moderator@gmail.com', '123456', 3);

select*
from Users
select*
from Roles
select*
from Videos
select*
from Transcripts

ALTER TABLE Videos
ADD Status INT DEFAULT 1;
alter table Users
add Status int default 1;




CREATE TABLE Levels(
    LevelId INT IDENTITY(1,1) PRIMARY KEY,
    LevelName NVARCHAR(50),
    Description NVARCHAR(255),
    OrderIndex INT,
    IsActive BIT DEFAULT 1
);
CREATE TABLE Courses(
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    LevelId INT,
    CourseName NVARCHAR(255),
    Description NVARCHAR(255),
    OrderIndex INT,
    CreatedBy INT,
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (LevelId) REFERENCES Levels(LevelId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
CREATE TABLE Lessons(
    LessonId INT IDENTITY(1,1) PRIMARY KEY,
    CourseId INT,
    LessonName NVARCHAR(255),
    VideoUrl NVARCHAR(255),
    Duration INT,
    OrderIndex INT,

    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);
CREATE TABLE Quizzes(
    QuizId INT IDENTITY(1,1) PRIMARY KEY,
    LessonId INT,
    QuizName NVARCHAR(255),
    PassScore FLOAT,

    FOREIGN KEY (LessonId) REFERENCES Lessons(LessonId)
);
CREATE TABLE Questions(
    QuestionId INT IDENTITY(1,1) PRIMARY KEY,
    QuizId INT,
    QuestionText NVARCHAR(500),

    FOREIGN KEY (QuizId) REFERENCES Quizzes(QuizId)
);
CREATE TABLE Answers(
    AnswerId INT IDENTITY(1,1) PRIMARY KEY,
    QuestionId INT,
    AnswerText NVARCHAR(255),
    IsCorrect BIT,

    FOREIGN KEY (QuestionId) REFERENCES Questions(QuestionId)
);
CREATE TABLE UserQuiz(
    UserQuizId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    QuizId INT,
    Score FLOAT,
    CompletedDate DATETIME,
    IsPassed BIT,

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (QuizId) REFERENCES Quizzes(QuizId),

    CONSTRAINT UQ_User_Quiz UNIQUE(UserId, QuizId)
);
CREATE TABLE UserProgress(
    UserProgressId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    LessonId INT,
    IsUnlocked BIT DEFAULT 0,
    IsCompleted BIT DEFAULT 0,
    CompletedDate DATETIME,

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (LessonId) REFERENCES Lessons(LessonId),

    CONSTRAINT UQ_User_Lesson UNIQUE(UserId, LessonId)
);
CREATE TABLE UserCourse(
    UserCourseId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    CourseId INT,
    IsUnlocked BIT DEFAULT 0,
    IsCompleted BIT DEFAULT 0,
    AssignedDate DATETIME,
    CompletedDate DATETIME,

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId),

    CONSTRAINT UQ_User_Course UNIQUE(UserId, CourseId)
);
CREATE TABLE UserLevel(
    UserLevelId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    LevelId INT,
    IsCompleted BIT DEFAULT 0,
    AssignedDate DATETIME,
    CompletedDate DATETIME,

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (LevelId) REFERENCES Levels(LevelId),

    CONSTRAINT UQ_User_Level UNIQUE(UserId, LevelId)
);
CREATE TABLE UserFavorite(
    UserFavoriteId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    LessonId INT,
    SavedDate DATETIME,

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (LessonId) REFERENCES Lessons(LessonId)
);

INSERT INTO Levels(LevelName, Description, OrderIndex, IsActive)
VALUES
('A1', N'Cơ bản', 1, 1),
('A2', N'Sơ cấp', 2, 1),
('B1', N'Trung cấp', 3, 1),
('B2', N'Trung cấp cao', 4, 1);

INSERT INTO Courses(LevelId, CourseName, Description, OrderIndex, CreatedBy, IsActive)
VALUES
(1, N'Giao tiếp cơ bản', N'Học chào hỏi', 1, 1, 1),
(1, N'Gia đình', N'Từ vựng gia đình', 2, 1, 1),
(2, N'Giao tiếp nâng cao', N'Hội thoại', 1, 1, 1);

INSERT INTO Lessons(CourseId, LessonName, VideoUrl, Duration, OrderIndex)
VALUES
(1, N'Xin chào', 'lesson1.mp4', 300, 1),
(1, N'Tôi là...', 'lesson2.mp4', 320, 2),
(1, N'Bạn khỏe không?', 'lesson3.mp4', 280, 3),

(2, N'Gia đình tôi', 'lesson4.mp4', 300, 1),
(2, N'Nghề nghiệp', 'lesson5.mp4', 350, 2);

INSERT INTO Quizzes(LessonId, QuizName, PassScore)
VALUES
(1, N'Quiz Xin chào', 70),
(2, N'Quiz Tôi là', 70),
(3, N'Quiz Bạn khỏe không', 70),
(4, N'Quiz Gia đình', 70),
(5, N'Quiz Nghề nghiệp', 70);

INSERT INTO Questions(QuizId, QuestionText)
VALUES
(1, N'Xin chào nghĩa là gì?'),
(1, N'Khi gặp người lớn tuổi ta nói gì?'),

(2, N'Câu "Tôi là..." dùng để làm gì?'),
(2, N'Chọn câu giới thiệu bản thân đúng'),

(3, N'Bạn khỏe không nghĩa là gì?'),

(4, N'Từ "Gia đình" nghĩa là gì?'),

(5, N'Nghề nghiệp nghĩa là gì?');
 
 INSERT INTO Answers(QuestionId, AnswerText, IsCorrect)
VALUES
-- Quiz 1
(1, N'Hello', 1),
(1, N'Goodbye', 0),
(1, N'Thank you', 0),

(2, N'Xin chào ạ', 1),
(2, N'Ê mày', 0),
(2, N'Hello bro', 0),

-- Quiz 2
(3, N'Giới thiệu bản thân', 1),
(3, N'Hỏi tuổi', 0),

(4, N'Tôi là Minh', 1),
(4, N'Tôi ăn cơm', 0),

-- Quiz 3
(5, N'How are you?', 1),
(5, N'Where are you?', 0),

-- Quiz 4
(6, N'Family', 1),
(6, N'Friend', 0),

-- Quiz 5
(7, N'Job', 1),
(7, N'Food', 0);

INSERT INTO UserLevel(UserId, LevelId, IsCompleted, AssignedDate)
VALUES
(1, 1, 0, GETDATE());
 INSERT INTO UserCourse(UserId, CourseId, IsUnlocked, IsCompleted, AssignedDate)
VALUES
(1, 1, 1, 0, GETDATE());
INSERT INTO UserProgress(UserId, LessonId, IsUnlocked, IsCompleted)
VALUES
(1, 1, 1, 0);

select*
from Courses
select*
from Quizzes where LessonId=1
SELECT UserId, LevelId, IsCompleted FROM UserLevel WHERE UserId = 1
SELECT * FROM UserLevel WHERE UserId = 18
SELECT * FROM UserCourse WHERE UserId = 18
SELECT * FROM UserProgress WHERE UserId = 18

-- Level: không trùng thứ tự
CREATE UNIQUE INDEX IX_Level_Order
ON Levels(OrderIndex);

-- Course: không trùng trong cùng Level
CREATE UNIQUE INDEX IX_Course_Level_Order
ON Courses(LevelId, OrderIndex);

-- Lesson: không trùng trong cùng Course
CREATE UNIQUE INDEX IX_Lesson_Course_Order
ON Lessons(CourseId, OrderIndex);

SELECT OrderIndex, COUNT(*) 
FROM Levels
GROUP BY OrderIndex
HAVING COUNT(*) > 1;

select*
from Courses
where LevelId=2
SELECT AnswerId, AnswerText, IsCorrect
FROM Answers
------- 06/04
ALTER TABLE UserProgress DROP COLUMN IsUnlocked;
ALTER TABLE UserProgress DROP COLUMN IsCompleted;

ALTER TABLE UserCourse DROP COLUMN IsUnlocked;
ALTER TABLE UserCourse DROP COLUMN IsCompleted;

ALTER TABLE UserLevel DROP COLUMN IsCompleted;
SELECT name
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('UserProgress')
AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('UserProgress'), 'IsUnlocked', 'ColumnId');
ALTER TABLE UserProgress DROP CONSTRAINT DF__UserProgr__IsUnl__41EDCAC5;
ALTER TABLE UserProgress DROP COLUMN IsUnlocked;
ALTER TABLE UserProgress DROP COLUMN IsCompleted;
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserProgress';
EXEC sp_rename 'UserProgress.IsCompleted', 'Status', 'COLUMN';
ALTER TABLE UserProgress
ADD AssignedDate DATETIME DEFAULT GETDATE();
select*
from UserProgress
select*
from UserCourse
EXEC sp_rename 'UserCourse.IsCompleted', 'Status', 'COLUMN';
SELECT name
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('UserCourse')
AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('UserCourse'), 'IsUnlocked', 'ColumnId');
ALTER TABLE UserCourse DROP CONSTRAINT DF__UserCours__IsUnl__489AC854;
ALTER TABLE UserCourse DROP COLUMN IsUnlocked;
select*
from UserLevel
EXEC sp_rename 'UserLevel.IsCompleted', 'Status', 'COLUMN';
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserLevel';

SELECT * FROM UserLevel WHERE UserId = 7
SELECT * FROM UserCourse WHERE UserId = 7
SELECT * FROM UserProgress WHERE UserId = 7

ALTER TABLE Lessons
ADD IsActive BIT DEFAULT 1;

ALTER TABLE Quizzes
ADD IsActive BIT DEFAULT 1;

ALTER TABLE Questions
ADD IsActive BIT DEFAULT 1;

ALTER TABLE Answers
ADD IsActive BIT DEFAULT 1;


------------ 07/04
-- sửa lại level, course , lesson là lấy email vào không lấy userId


select*
from Quizzes
ALTER TABLE Answers 
DROP COLUMN IsActive;
ALTER TABLE Questions
DROP COLUMN IsActive;
ALTER TABLE Quizzes
DROP COLUMN IsActive;
select*
from Quizzes
delete from Quizzes
where QuizId=8

DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql += 'ALTER TABLE Answers DROP CONSTRAINT ' + dc.name + ';'
FROM sys.default_constraints dc
JOIN sys.columns c 
    ON dc.parent_object_id = c.object_id 
    AND dc.parent_column_id = c.column_id
WHERE OBJECT_NAME(dc.parent_object_id) = 'Answers'
AND c.name = 'IsActive';

EXEC(@sql);

ALTER TABLE Answers DROP COLUMN IsActive;


DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql += 'ALTER TABLE Questions DROP CONSTRAINT ' + dc.name + ';'
FROM sys.default_constraints dc
JOIN sys.columns c 
    ON dc.parent_object_id = c.object_id 
    AND dc.parent_column_id = c.column_id
WHERE OBJECT_NAME(dc.parent_object_id) = 'Questions'
AND c.name = 'IsActive';

EXEC(@sql);

ALTER TABLE Questions DROP COLUMN IsActive;

DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql += 'ALTER TABLE Quizzes DROP CONSTRAINT ' + dc.name + ';'
FROM sys.default_constraints dc
JOIN sys.columns c 
    ON dc.parent_object_id = c.object_id 
    AND dc.parent_column_id = c.column_id
WHERE OBJECT_NAME(dc.parent_object_id) = 'Quizzes'
AND c.name = 'IsActive';

EXEC(@sql);

ALTER TABLE Quizzes DROP COLUMN IsActive;
---------- 08/04
select*
from UserLevel
select*
from UserCourse
select*
from UserProgress
delete from UserLevel
where UserLevelId=5