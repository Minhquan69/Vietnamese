create database vietnamese
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