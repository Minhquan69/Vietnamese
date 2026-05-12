-- Tài khoản demo (chỉ user + hash). Nên chạy đầy đủ: 2026_auth_bootstrap.sql (Roles + RefreshTokens + chuẩn hóa email).
-- Email: demo@vietnamese.local  |  Mật khẩu: DemoViet2026!

DECLARE @UserRoleId INT = (SELECT TOP (1) RoleId FROM dbo.Roles WHERE RoleName = N'User');
IF @UserRoleId IS NULL
BEGIN
  RAISERROR(N'Chạy 2026_auth_bootstrap.sql trước (thiếu role User).', 16, 1);
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
GO
