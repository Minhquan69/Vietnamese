-- Run against VietnameseLearningPlatform (or your DB name)
-- Auth: refresh tokens, avatar, password reset columns

IF COL_LENGTH('Users', 'AvatarUrl') IS NULL
  ALTER TABLE Users ADD AvatarUrl NVARCHAR(512) NULL;

IF COL_LENGTH('Users', 'PasswordResetTokenHash') IS NULL
  ALTER TABLE Users ADD PasswordResetTokenHash NVARCHAR(128) NULL;

IF COL_LENGTH('Users', 'PasswordResetTokenExpiresUtc') IS NULL
  ALTER TABLE Users ADD PasswordResetTokenExpiresUtc DATETIME2 NULL;

IF OBJECT_ID(N'RefreshTokens', N'U') IS NULL
BEGIN
  CREATE TABLE RefreshTokens (
    RefreshTokenId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    TokenHash NVARCHAR(128) NOT NULL,
    ExpiresUtc DATETIME2 NOT NULL,
    RevokedUtc DATETIME2 NULL,
    ReplacedByTokenHash NVARCHAR(128) NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
  );
  CREATE UNIQUE INDEX IX_RefreshTokens_TokenHash ON RefreshTokens(TokenHash);
  CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
END
