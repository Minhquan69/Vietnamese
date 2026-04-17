

-- ===================== ROLES =====================
CREATE TABLE Roles (
    RoleId INT IDENTITY PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL
);

-- ===================== USERS =====================
CREATE TABLE Users (
    UserId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    Password NVARCHAR(100),
    RoleId INT,
    Status INT DEFAULT 1,

    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- ===================== VIDEOS =====================
CREATE TABLE Videos (
    VideoId INT IDENTITY PRIMARY KEY,
    Youtube NVARCHAR(50),
    Title NVARCHAR(200),
    CreatedBy NVARCHAR(255), -- FIX ở đây
    Status INT DEFAULT 0
);

-- ===================== TRANSCRIPTS =====================
CREATE TABLE Transcripts (
    TranscriptId INT IDENTITY PRIMARY KEY,
    VideoId INT,
    Sentence NVARCHAR(MAX),
    StartTime FLOAT,

    FOREIGN KEY (VideoId) REFERENCES Videos(VideoId)
);

-- ===================== LEVELS =====================
CREATE TABLE Levels (
    LevelId INT IDENTITY PRIMARY KEY,
    LevelName NVARCHAR(50),
    Description NVARCHAR(255),
    OrderIndex INT,
    IsActive BIT DEFAULT 1
);

-- ===================== COURSES =====================
CREATE TABLE Courses (
    CourseId INT IDENTITY PRIMARY KEY,
    LevelId INT,
    CourseName NVARCHAR(255),
    Description NVARCHAR(255),
    OrderIndex INT,
    CreatedBy NVARCHAR(255), -- FIX
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (LevelId) REFERENCES Levels(LevelId)
);

-- ===================== UNITS =====================
CREATE TABLE Units (
    UnitId INT IDENTITY PRIMARY KEY,
    CourseId INT,
    UnitName NVARCHAR(255),
    Description NVARCHAR(255),
    Objective NVARCHAR(MAX),
    VideoUrl NVARCHAR(500),
    Duration INT,
    OrderIndex INT,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(255), -- FIX
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);

-- ===================== QUIZZES =====================
CREATE TABLE Quizzes (
    QuizId INT IDENTITY PRIMARY KEY,
    UnitId INT,
    QuizName NVARCHAR(255),
    PassScore FLOAT,

    FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
);

-- ===================== QUESTIONS =====================
CREATE TABLE Questions (
    QuestionId INT IDENTITY PRIMARY KEY,
    QuizId INT,
    QuestionText NVARCHAR(500),
    ImageUrl NVARCHAR(500),
    AudioUrl NVARCHAR(500),

    FOREIGN KEY (QuizId) REFERENCES Quizzes(QuizId)
);

-- ===================== ANSWERS =====================
CREATE TABLE Answers (
    AnswerId INT IDENTITY PRIMARY KEY,
    QuestionId INT,
    AnswerText NVARCHAR(255),
    IsCorrect BIT,
    ImageUrl NVARCHAR(500),
    AudioUrl NVARCHAR(500),

    FOREIGN KEY (QuestionId) REFERENCES Questions(QuestionId)
);

-- ===================== USER QUIZ =====================
CREATE TABLE UserQuiz (
    UserQuizId INT IDENTITY PRIMARY KEY,
    UserId INT,
    QuizId INT,
    Score FLOAT,
    CompletedDate DATETIME,
    IsPassed BIT,

    UNIQUE (UserId, QuizId),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (QuizId) REFERENCES Quizzes(QuizId)
);

-- ===================== USER PROGRESS =====================
CREATE TABLE UserProgress (
    UserProgressId INT IDENTITY PRIMARY KEY,
    UserId INT,
    UnitId INT,
    AssignedDate DATETIME,
    CompletedDate DATETIME,
    Status BIT,

    UNIQUE (UserId, UnitId),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
);

-- ===================== USER LEVEL =====================
CREATE TABLE UserLevel (
    UserLevelId INT IDENTITY PRIMARY KEY,
    UserId INT,
    LevelId INT,
    AssignedDate DATETIME,
    CompletedDate DATETIME,
    Status BIT,

    UNIQUE (UserId, LevelId),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (LevelId) REFERENCES Levels(LevelId)
);

-- ===================== USER COURSE =====================
CREATE TABLE UserCourse (
    UserCourseId INT IDENTITY PRIMARY KEY,
    UserId INT,
    CourseId INT,
    AssignedDate DATETIME,
    CompletedDate DATETIME,
    Status BIT,

    UNIQUE (UserId, CourseId),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);

-- ===================== USER FAVORITE =====================
CREATE TABLE UserFavorite (
    UserFavoriteId INT IDENTITY PRIMARY KEY,
    UserId INT,
    UnitId INT,
    SavedDate DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
);
ALTER TABLE Videos
ALTER COLUMN CreatedBy INT;
select*
from dbo.Users
INSERT INTO Roles (RoleName)
VALUES 
(N'User'),
(N'Admin'),
(N'Quản trị viên');
EXEC sp_rename 'Videos.Youtube', 'YoutubeId', 'COLUMN';
select*
from Videos

INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(2, N'1: Các thành viên trong nhà', N'Từ vựng về ông, bà, bố, mẹ, anh, chị, em.', N'Nhận diện và gọi tên đúng các thành viên cơ bản.', N'c2_u1.mp4', 500, 1, N'Admin', 1),
(2, N'2: Đại từ xưng hô mở rộng', N'Cách dùng Chú, Bác, Cô, Dì và tôn ti trật tự.', N'Sử dụng đúng hệ thống xưng hô gia đình Việt Nam.', N'c2_u2.mp4', 480, 2, N'Admin', 1),
(2, N'3: Hỏi thăm sức khỏe', N'Cấu trúc hỏi thăm tình hình người thân.', N'Thực hiện được cuộc đối thoại ngắn về gia đình.', N'c2_u3.mp4', 420, 3, N'Admin', 1),
(2, N'4: Nghề nghiệp người thân', N'Kết hợp từ vựng nghề nghiệp để nói về gia đình.', N'Mô tả được công việc của các thành viên trong nhà.', N'c2_u4.mp4', 550, 4, N'Admin', 1),
(2, N'5: Kể về gia đình mình', N'Bài tổng hợp miêu tả tổng quan về gia đình.', N'Viết được đoạn văn ngắn giới thiệu về gia đình.', N'c2_u5.mp4', 600, 5, N'Admin', 1);

-- =============================================================
-- COURSE 3: NGOẠI HÌNH & TÍNH CÁCH (Level A2)
-- =============================================================
INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(3, N'1: Miêu tả Ngoại hình', N'Từ vựng: cao, thấp, béo, gầy, cân đối.', N'Sử dụng tính từ cơ bản để miêu tả vóc dáng.', N'c3_u1.mp4', 520, 1, N'Admin', 1),
(3, N'2: Khuôn mặt và Kiểu tóc', N'Miêu tả mắt, mũi, miệng và màu sắc tóc.', N'Miêu tả chi tiết đặc điểm nhận dạng khuôn mặt.', N'c3_u2.mp4', 480, 2, N'Admin', 1),
(3, N'3: Tính cách con người', N'Từ vựng: hiền, thông minh, vui tính, khó tính.', N'Sử dụng tính từ chỉ đặc điểm tâm lý, tính cách.', N'c3_u3.mp4', 500, 3, N'Admin', 1),
(3, N'4: Cấu trúc So sánh', N'So sánh hơn, nhất (Cao hơn, thông minh nhất...).', N'Biết cách so sánh đặc điểm giữa các cá nhân.', N'c3_u4.mp4', 600, 4, N'Admin', 1),
(3, N'5: Người bạn thân nhất', N'Bài tập tổng hợp: Miêu tả người bạn yêu quý.', N'Viết đoạn văn hoàn chỉnh miêu tả một người.', N'c3_u5.mp4', 700, 5, N'Admin', 1);

-- =============================================================
-- COURSE 4: SỞ THÍCH & GIẢI TRÍ (Level A1/A2)
-- =============================================================
INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(4, N'1: Các hoạt động giải trí', N'Xem phim, nghe nhạc, đọc sách, chơi game.', N'Gọi tên được các sở thích phổ biến hàng ngày.', N'c4_u1.mp4', 450, 1, N'Admin', 1),
(4, N'2: Thể thao & Vận động', N'Bóng đá, cầu lông, bơi lội, chạy bộ.', N'Trao đổi về các môn thể thao yêu thích.', N'c4_u2.mp4', 480, 2, N'Admin', 1),
(4, N'3: Tần suất hoạt động', N'Cách dùng: Thường xuyên, thỉnh thoảng, hiếm khi.', N'Nói về mức độ thường xuyên của các sở thích.', N'c4_u3.mp4', 500, 3, N'Admin', 1),
(4, N'4: Tại sao bạn thích nó?', N'Cấu trúc diễn đạt lý do (Vì nó thú vị, thư giãn...).', N'Giải thích được lý do yêu thích một hoạt động.', N'c4_u4.mp4', 550, 4, N'Admin', 1),
(4, N'5: Kế hoạch cuối tuần', N'Rủ rê bạn bè tham gia các hoạt động giải trí.', N'Lên lịch và mời người khác cùng đi chơi.', N'c4_u5.mp4', 600, 5, N'Admin', 1);


INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(5, N'1: Món ăn Việt Nam', N'Phở, Bún chả, Bánh mì, Cơm tấm.', N'Nhận diện các món đặc sản cơ bản của Việt Nam.', N'c5_u1.mp4', 500, 1, N'Admin', 1),
(2, N'2: Các loại đồ uống', N'Cà phê sữa đá, trà đá, nước ép trái cây.', N'Gọi đồ uống tự tin tại các quán giải khát.', N'c5_u2.mp4', 420, 2, N'Admin', 1),
(5, N'3: Hương vị & Cảm nhận', N'Ngọt, cay, mặn, chua, đắng, ngon.', N'Mô tả được khẩu vị và cảm nhận về món ăn.', N'c5_u3.mp4', 480, 3, N'Admin', 1),
(5, N'4: Tại nhà hàng', N'Cách đặt bàn, gọi món và gọi phục vụ.', N'Thực hiện đúng quy trình giao tiếp tại quán ăn.', N'c5_u4.mp4', 550, 4, N'Admin', 1),
(5, N'5: Tính tiền & Thanh toán', N'Hỏi giá, xem hóa đơn và tiền thừa.', N'Thực hiện việc thanh toán hóa đơn ăn uống.', N'c5_u5.mp4', 400, 5, N'Admin', 1);


INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(6, N'1: Phương tiện giao thông', N'Các loại xe phổ biến: Xe máy, ô tô, xe buýt.', N'Gọi tên và phân biệt được các phương tiện đi lại.', N'c6_u1.mp4', 450, 1, N'Admin', 1),
(6, N'2: Hỏi đường cơ bản', N'Cấu trúc "Cho hỏi đường đến...", "Ở đâu?".', N'Biết cách đặt câu hỏi tìm địa điểm đơn giản.', N'c6_u2.mp4', 500, 2, N'Admin', 1),
(6, N'3: Chỉ đường chi tiết', N'Rẽ trái, rẽ phải, đi thẳng, quay đầu.', N'Hiểu và thực hiện theo chỉ dẫn của người bản xứ.', N'c6_u3.mp4', 480, 3, N'Admin', 1),
(6, N'4: Khoảng cách & Thời gian', N'Cấu trúc "Bao xa?", "Mất bao lâu?".', N'Ước lượng được quãng đường và thời gian di chuyển.', N'c6_u4.mp4', 420, 4, N'Admin', 1),
(6, N'5: Tại bến xe & Nhà ga', N'Mua vé, hỏi giờ xe chạy, tìm cửa ra.', N'Thực hiện giao tiếp cơ bản tại các đầu mối giao thông.', N'c6_u5.mp4', 550, 5, N'Admin', 1);

-- =============================================================
-- COURSE 7: THỜI GIAN & THỜI TIẾT (Level A1)
-- =============================================================
INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(7, N'1: Cách xem giờ', N'Giờ chẵn, giờ rưỡi, giờ kém và các buổi trong ngày.', N'Hỏi và trả lời chính xác về thời gian hiện tại.', N'c7_u1.mp4', 400, 1, N'Admin', 1),
(7, N'2: Thứ, Ngày và Tháng', N'Cách gọi 7 ngày trong tuần và 12 tháng trong năm.', N'Nói được ngày sinh nhật và các ngày lễ quan trọng.', N'c7_u2.mp4', 450, 2, N'Admin', 1),
(7, N'3: Thời tiết hàng ngày', N'Nắng, mưa, nóng, lạnh, gió, bão.', N'Mô tả được tình trạng thời tiết và nhiệt độ cơ bản.', N'c7_u3.mp4', 480, 3, N'Admin', 1),
(7, N'4: Bốn mùa ở Việt Nam', N'Đặc điểm khí hậu miền Bắc và miền Nam.', N'Trao đổi được về sự khác biệt khí hậu vùng miền.', N'c7_u4.mp4', 500, 4, N'Admin', 1),
(7, N'5: Lên lịch và Hẹn gặp', N'Sắp xếp thời gian gặp bạn bè, đồng nghiệp.', N'Thực hiện giao tiếp sắp đặt lịch trình cá nhân.', N'c7_u5.mp4', 550, 5, N'Admin', 1);

-- =============================================================
-- COURSE 8: SỨC KHỎE (Level A1)
-- =============================================================
INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(8, N'1: Bộ phận cơ thể', N'Đầu, tay, chân, mắt, mũi, miệng.', N'Gọi tên chính xác các bộ phận trên cơ thể người.', N'c8_u1.mp4', 420, 1, N'Admin', 1),
(8, N'2: Các bệnh thường gặp', N'Đau đầu, cảm cúm, đau bụng, ho, sốt.', N'Trình bày được tình trạng sức khỏe cá nhân đơn giản.', N'c8_u2.mp4', 500, 2, N'Admin', 1),
(8, N'3: Tại hiệu thuốc', N'Cách mua thuốc và hỏi liều lượng sử dụng.', N'Tự mua được các loại thuốc thông dụng không kê đơn.', N'c8_u3.mp4', 480, 3, N'Admin', 1),
(8, N'4: Lời khuyên sức khỏe', N'Nên nghỉ ngơi, uống nước, tập thể dục.', N'Đưa ra và hiểu các chỉ dẫn y tế cơ bản.', N'c8_u4.mp4', 450, 4, N'Admin', 1),
(8, N'5: Tại phòng khám', N'Giao tiếp với bác sĩ về triệu chứng bệnh.', N'Thực hiện quy trình khám bệnh cơ bản.', N'c8_u5.mp4', 600, 5, N'Admin', 1);

-- =============================================================
-- COURSE 9: NHÀ CỬA & ĐỒ VẬT (Level A1)
-- =============================================================
INSERT INTO Units (CourseId, UnitName, [Description], Objective, VideoUrl, Duration, OrderIndex, CreatedBy, IsActive)
VALUES 
(9, N'1: Các loại nhà phổ biến', N'Nhà phố, chung cư, nhà sàn, biệt thự.', N'Phân biệt các kiểu kiến trúc nhà ở tại Việt Nam.', N'c9_u1.mp4', 480, 1, N'Admin', 1),
(9, N'2: Không gian trong nhà', N'Phòng khách, phòng ngủ, bếp, vệ sinh.', N'Mô tả chức năng của các phòng trong ngôi nhà.', N'c9_u2.mp4', 450, 2, N'Admin', 1),
(9, N'3: Đồ đạc thiết yếu', N'Bàn ghế, giường tủ, tivi, tủ lạnh.', N'Gọi tên các món đồ dùng gia đình thường gặp.', N'c9_u3.mp4', 500, 3, N'Admin', 1),
(9, N'4: Vị trí của đồ vật', N'Trên, dưới, trong, ngoài, trái, phải.', N'Mô tả chính xác vị trí đồ vật trong không gian.', N'c9_u4.mp4', 520, 4, N'Admin', 1),
(9, N'5: Ngôi nhà trong mơ', N'Bài tập tổng hợp miêu tả nơi ở lý tưởng.', N'Sử dụng từ vựng không gian để thuyết trình về nhà ở.', N'c9_u5.mp4', 600, 5, N'Admin', 1);

select*
from Units
select*
from Courses

SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Units');
ALTER TABLE Units
DROP CONSTRAINT FK__Units__CourseId__5EBF139D;
ALTER TABLE Units
ADD CONSTRAINT FK_Units_Courses
FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
ON DELETE CASCADE;


SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Quizzes');

ALTER TABLE Quizzes DROP CONSTRAINT FK__Quizzes__UnitId__619B8048;

ALTER TABLE Quizzes
ADD CONSTRAINT FK_Quizzes_Units
FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
ON DELETE CASCADE;


SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Questions');
ALTER TABLE Questions DROP CONSTRAINT FK__Questions__QuizI__6477ECF3;

ALTER TABLE Questions
ADD CONSTRAINT FK_Questions_Quizzes
FOREIGN KEY (QuizId) REFERENCES Quizzes(QuizId)
ON DELETE CASCADE;


SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Answers');
ALTER TABLE Answers DROP CONSTRAINT FK__Answers__Questio__6754599E;

ALTER TABLE Answers
ADD CONSTRAINT FK_Answers_Questions
FOREIGN KEY (QuestionId) REFERENCES Questions(QuestionId)
ON DELETE CASCADE;



SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Courses');
ALTER TABLE Courses DROP CONSTRAINT FK__Courses__LevelId__59FA5E80;

ALTER TABLE Courses
ADD CONSTRAINT FK_Courses_Levels
FOREIGN KEY (LevelId) REFERENCES Levels(LevelId)
ON DELETE CASCADE;

SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('UserCourse');
ALTER TABLE UserCourse DROP CONSTRAINT FK__UserCours__Cours__7A672E12;

ALTER TABLE UserCourse
ADD CONSTRAINT FK_UserCourse_Courses
FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
ON DELETE CASCADE;


SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('UserProgress');
ALTER TABLE UserProgress DROP CONSTRAINT FK__UserProgr__UnitI__70DDC3D8;

ALTER TABLE UserProgress
ADD CONSTRAINT FK_UserProgress_Units
FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
ON DELETE CASCADE;


SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('UserQuiz');
ALTER TABLE UserQuiz DROP CONSTRAINT FK__UserQuiz__QuizId__6C190EBB;

ALTER TABLE UserQuiz
ADD CONSTRAINT FK_UserQuiz_Quizzes
FOREIGN KEY (QuizId) REFERENCES Quizzes(QuizId)
ON DELETE CASCADE;


SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('UserFavorite');
ALTER TABLE UserFavorite DROP CONSTRAINT FK__UserFavor__UnitI__7F2BE32F;

ALTER TABLE UserFavorite
ADD CONSTRAINT FK_UserFavorite_Units
FOREIGN KEY (UnitId) REFERENCES Units(UnitId)
ON DELETE CASCADE;


SELECT name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Transcripts');

ALTER TABLE Transcripts 
DROP CONSTRAINT FK__Transcrip__Video__534D60F1; 

ALTER TABLE Transcripts
ADD CONSTRAINT FK_Transcripts_Videos
FOREIGN KEY (VideoId) REFERENCES Videos(VideoId)
ON DELETE CASCADE;


----------////////////
--- 17/04
drop view View_UserProgressDetail
create VIEW View_UserProgressDetail AS
SELECT 
    l.LevelId, l.LevelName, l.OrderIndex AS LevelOrder, l.IsActive AS LevelActive,
    c.CourseId, c.CourseName, c.OrderIndex AS CourseOrder, c.IsActive AS CourseActive,
    u.UnitId, u.UnitName, u.VideoUrl, u.Duration, u.OrderIndex AS UnitOrder, u.IsActive AS UnitActive,
    
    COALESCE(ul.UserId, uc.UserId, up.UserId) AS UserId,
    
    ul.Status AS LevelStatus,
    uc.Status AS CourseStatus,
    up.Status AS UnitStatus
FROM Levels l
LEFT JOIN Courses c ON l.LevelId = c.LevelId AND c.IsActive = 1
LEFT JOIN Units u ON c.CourseId = u.CourseId AND u.IsActive = 1

LEFT JOIN UserLevel ul ON l.LevelId = ul.LevelId
LEFT JOIN UserCourse uc ON c.CourseId = uc.CourseId AND uc.UserId = ul.UserId
LEFT JOIN UserProgress up ON u.UnitId = up.UnitId AND up.UserId = ul.UserId
GO
select*
from View_UserProgressDetail
where UserId=2


