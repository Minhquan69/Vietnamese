/*
  Auth + đăng nhập: chạy script này trên đúng database trong "DefaultConnection" của Backend
  (SQL Server, ví dụ VietnameseLearningPlatform).

  Giải quyết các lỗi thường gặp:
  - Chưa có bảng RefreshTokens → API lỗi khi đăng nhập (không tạo được refresh token).
  - Chưa có role "User" / "Admin" / "Moderator" → đăng ký hoặc seed user thất bại.
  - Email trong DB viết HOA → code cũ so khớp chữ thường thì không đăng nhập được (đã sửa code dùng ToLower;
    script này vẫn chuẩn hóa email trong DB).
  - PasswordHash không phải BCrypt → đăng nhập sai; user demo được ghi đè hash đúng.

  Tài khoản sau khi chạy (nếu MERGE thành công):
    Email:    demo@vietnamese.local
    Mật khẩu: DemoViet2026!

  Gợi ý kiểm tra trong SSMS:
    SELECT UserId, Email, Status, RoleId, LEN(PasswordHash) AS HashLen FROM dbo.Users WHERE Email = N'demo@vietnamese.local';
    SELECT OBJECT_ID(N'dbo.RefreshTokens', N'U') AS RefreshTokensExists;
*/

SET NOCOUNT ON;

/* ---- 1) Cột Users + bảng RefreshTokens (giống 2026_auth_extensions.sql) ---- */
IF COL_LENGTH(N'dbo.Users', N'AvatarUrl') IS NULL
  ALTER TABLE dbo.Users ADD AvatarUrl NVARCHAR(512) NULL;

IF COL_LENGTH(N'dbo.Users', N'PasswordResetTokenHash') IS NULL
  ALTER TABLE dbo.Users ADD PasswordResetTokenHash NVARCHAR(128) NULL;

IF COL_LENGTH(N'dbo.Users', N'PasswordResetTokenExpiresUtc') IS NULL
  ALTER TABLE dbo.Users ADD PasswordResetTokenExpiresUtc DATETIME2 NULL;

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.RefreshTokens (
    RefreshTokenId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    TokenHash NVARCHAR(128) NOT NULL,
    ExpiresUtc DATETIME2 NOT NULL,
    RevokedUtc DATETIME2 NULL,
    ReplacedByTokenHash NVARCHAR(128) NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
  );
  CREATE UNIQUE INDEX IX_RefreshTokens_TokenHash ON dbo.RefreshTokens(TokenHash);
  CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
END

/* ---- 2) Roles tối thiểu cho app ---- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'User')
  INSERT INTO dbo.Roles (RoleName) VALUES (N'User');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Admin')
  INSERT INTO dbo.Roles (RoleName) VALUES (N'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Moderator')
  INSERT INTO dbo.Roles (RoleName) VALUES (N'Moderator');

/* ---- 3) Chuẩn hóa email (chữ thường, trim) ---- */
UPDATE dbo.Users
SET Email = LOWER(LTRIM(RTRIM(Email)))
WHERE Email IS NOT NULL
  AND Email <> LOWER(LTRIM(RTRIM(Email)));

/* ---- 4) User demo: insert hoặc cập nhật hash + Status + Role ---- */
DECLARE @UserRoleId INT = (SELECT TOP (1) RoleId FROM dbo.Roles WHERE RoleName = N'User');

IF @UserRoleId IS NULL
BEGIN
  RAISERROR(N'Không tìm thấy role User trong dbo.Roles.', 16, 1);
  RETURN;
END

;MERGE dbo.Users AS T
USING (
  SELECT
    N'Demo learner' AS Name,
    N'demo@vietnamese.local' AS Email,
    N'$2a$11$z8zhEsRYYZ9ZIapoLUkgRO9/mc9GLlOH65bVCOiqfB6fJ0OFF0foW' AS PasswordHash,
    @UserRoleId AS RoleId,
    1 AS Status
) AS S
ON LOWER(LTRIM(RTRIM(T.Email))) = S.Email
WHEN MATCHED THEN
  UPDATE SET
    T.Name = S.Name,
    T.Email = S.Email,
    T.PasswordHash = S.PasswordHash,
    T.RoleId = S.RoleId,
    T.Status = S.Status
WHEN NOT MATCHED BY TARGET THEN
  INSERT (Name, Email, PasswordHash, RoleId, Status)
  VALUES (S.Name, S.Email, S.PasswordHash, S.RoleId, S.Status);

PRINT N'Auth bootstrap xong. Thử đăng nhập: demo@vietnamese.local / DemoViet2026!';

/*
  Kiểm tra user ↔ role (RoleId mồ côi thường gây 401 dù đúng mật khẩu):

  SELECT u.UserId, u.Email, u.RoleId, r.RoleName
  FROM dbo.Users u
  LEFT JOIN dbo.Roles r ON r.RoleId = u.RoleId
  WHERE u.Email = N'demo@vietnamese.local';

  Sửa RoleId mồ côi (chạy nếu RoleName ở truy vấn trên là NULL):

  UPDATE u
  SET u.RoleId = (SELECT TOP (1) RoleId FROM dbo.Roles WHERE RoleName = N'User')
  FROM dbo.Users u
  WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.RoleId = u.RoleId);
*/
GO
