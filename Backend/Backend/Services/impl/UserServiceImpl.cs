using Backend.Common;
using Backend.dto;
using Backend.Features.Auth.Application;
using Backend.Models;
using Backend.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Backend.Services.impl
{
    public class UserServiceImpl : UserService
    {
        private readonly UserRepository _userrepository;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly JwtService _jwtService;
        private readonly UserContextUtil _userContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public UserServiceImpl(
            UserRepository userrepository,
            RefreshTokenRepository refreshTokenRepository,
            JwtService jwtService,
            UserContextUtil userContext,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _userrepository = userrepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
            _userContext = userContext;
            _configuration = configuration;
            _env = env;
        }

        private bool ValidateAdmin()
        {
            return _userContext.GetRole() == common.Constant.Role.Admin;
        }

        private async Task<string> IssueRefreshTokenAsync(int userId)
        {
            var plain = CryptoUtil.CreateOpaqueToken();
            var hash = CryptoUtil.Sha256Hex(plain);
            var days = int.TryParse(_configuration["Jwt:RefreshTokenDays"], out var d) ? d : 14;
            var entity = new RefreshToken
            {
                UserId = userId,
                TokenHash = hash,
                ExpiresUtc = DateTime.UtcNow.AddDays(days),
            };
            await _refreshTokenRepository.AddAsync(entity);
            return plain;
        }

        private object BuildAuthPayload(string accessToken, string refreshTokenPlain)
        {
            var seconds = _jwtService.AccessTokenLifetimeMinutes * 60;
            return new
            {
                accessToken,
                refreshToken = refreshTokenPlain,
                token = accessToken,
                expiresIn = seconds,
            };
        }

        public async Task<object?> Login(LoginDTO request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var userData = await _userrepository.GetUserWithRole(email);

            if (userData == null)
            {
                return null;
            }

            bool isPasswordValid;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    (string)userData.PasswordHash);
            }
            catch
            {
                isPasswordValid = false;
            }

            if (!isPasswordValid)
            {
                return null;
            }

            if ((int)userData.Status != 1)
            {
                return null;
            }

            var userId = (int)userData.UserId;
            await _refreshTokenRepository.RevokeAllForUserAsync(userId);

            var userDto = new UserDTO
            {
                Id = userId,
                Name = (string)userData.Name,
                Email = (string)userData.Email,
            };

            var access = _jwtService.GenerateAccessToken(userDto, (string)userData.RoleName);
            var refresh = await IssueRefreshTokenAsync(userId);
            return BuildAuthPayload(access, refresh);
        }

        public async Task<object> Register(RegisterDTO request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var isExist = await _userrepository.IsEmailExist(email);
            if (isExist)
            {
                throw new Exception("Email already exists");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = await _userrepository.Register(
                request.Name.Trim(),
                email,
                hashedPassword
            );

            await _refreshTokenRepository.RevokeAllForUserAsync(user.UserId);

            var userDto = new UserDTO
            {
                Id = user.UserId,
                Email = user.Email,
                Name = user.Name,
                RoleId = user.RoleId,
            };

            var access = _jwtService.GenerateAccessToken(userDto, "User");
            var refresh = await IssueRefreshTokenAsync(user.UserId);
            return BuildAuthPayload(access, refresh);
        }

        public async Task<object?> RefreshTokenPair(RefreshTokenRequestDto request)
        {
            var hash = CryptoUtil.Sha256Hex(request.RefreshToken.Trim());
            var existing = await _refreshTokenRepository.GetActiveByHashAsync(hash);
            if (existing?.User == null)
            {
                return null;
            }

            var user = existing.User;
            if (user.Status != 1)
            {
                throw new UnauthorizedAccessException("Account is not active.");
            }

            await _refreshTokenRepository.RevokeAsync(existing.RefreshTokenId, null);

            var roleName = user.Role?.RoleName ?? "User";
            var dto = new UserDTO
            {
                Id = user.UserId,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
            };

            var access = _jwtService.GenerateAccessToken(dto, roleName);
            var refresh = await IssueRefreshTokenAsync(user.UserId);
            return BuildAuthPayload(access, refresh);
        }

        public async Task<object> ForgotPassword(ForgotPasswordRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userrepository.GetUserByEmailNormalized(email);
            if (user == null)
            {
                return new { message = "If the email exists, reset instructions were sent." };
            }

            var plain = CryptoUtil.CreateOpaqueToken(32);
            user.PasswordResetTokenHash = CryptoUtil.Sha256Hex(plain);
            user.PasswordResetTokenExpiresUtc = DateTime.UtcNow.AddHours(1);
            await _userrepository.Update(user);
            await _userrepository.Save();

            var expose = _configuration.GetValue("Auth:ExposePasswordResetToken", false);
            if (expose)
            {
                return new
                {
                    message = "Password reset token issued (dev only).",
                    devResetToken = plain,
                };
            }

            return new { message = "If the email exists, reset instructions were sent." };
        }

        public async Task ResetPassword(ResetPasswordRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userrepository.GetUserByEmailNormalized(email);
            if (user == null || string.IsNullOrEmpty(user.PasswordResetTokenHash))
            {
                throw new Exception("Invalid reset request.");
            }

            if (user.PasswordResetTokenExpiresUtc == null ||
                user.PasswordResetTokenExpiresUtc < DateTime.UtcNow)
            {
                throw new Exception("Reset token has expired.");
            }

            var hash = CryptoUtil.Sha256Hex(request.Token.Trim());
            if (!CryptographicEquals(user.PasswordResetTokenHash, hash))
            {
                throw new Exception("Invalid reset token.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresUtc = null;
            await _userrepository.Update(user);
            await _userrepository.Save();
            await _refreshTokenRepository.RevokeAllForUserAsync(user.UserId);
        }

        private static bool CryptographicEquals(string a, string b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var result = 0;
            for (var i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        public async Task<string> UploadAvatar(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("No file uploaded.");
            }

            var user = await _userrepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/webp",
            };

            if (!allowedTypes.Contains(file.ContentType))
            {
                throw new Exception("Only JPEG, PNG, or WebP images are allowed.");
            }

            var ext = file.ContentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg",
            };

            var dir = Path.Combine(_env.WebRootPath, "avatars");
            Directory.CreateDirectory(dir);

            var fileName = $"{userId}{ext}";
            var physical = Path.Combine(dir, fileName);
            await using (var fs = new FileStream(physical, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var relative = $"/avatars/{fileName}";
            user.AvatarUrl = relative;
            await _userrepository.Update(user);
            await _userrepository.Save();
            return relative;
        }

        public async Task<object?> GetMeProfile(int userId)
        {
            var user = await _userrepository.GetUserById(userId);
            if (user == null)
            {
                return null;
            }

            return new
            {
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                role = user.Role?.RoleName ?? "",
                status = user.Status,
                avatarUrl = user.AvatarUrl,
            };
        }

        public async Task ChangePassword(int userId, ChangePasswordDTO dto)
        {
            var user = await _userrepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            {
                throw new Exception("The old password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _userrepository.Update(user);
            await _userrepository.Save();
            await _refreshTokenRepository.RevokeAllForUserAsync(userId);
        }

        public async Task UpdateProfile(int userId, UserDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Email))
            {
                throw new Exception("Name and Email cannot be empty");
            }

            var user = await _userrepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var email = dto.Email.Trim().ToLowerInvariant();
            var isExist = await _userrepository.IsEmailExist(email);
            if (isExist && user.Email != email)
            {
                throw new Exception("Email already exists");
            }

            user.Name = dto.Name.Trim();
            user.Email = email;

            await _userrepository.Update(user);
            await _userrepository.Save();
        }

        public async Task<object> GetUsers(string? email, int? status, int? roleId, int page, int pageSize)
        {
            var users = await _userrepository.GetUsers(email, status, roleId, page, pageSize);
            var total = await _userrepository.CountUsers(email, status, roleId);

            return new
            {
                total,
                page,
                pageSize,
                data = users,
            };
        }

        public async Task UpdateUserStatus(int userId, int status)
        {
            if (!ValidateAdmin())
            {
                throw new UnauthorizedAccessException("You do not have permission to edit the status.");
            }

            var user = await _userrepository.GetUserById(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (status != 1 && status != 0 && status != -1)
            {
                throw new Exception("Invalid status");
            }

            user.Status = status;

            await _userrepository.Update(user);
            await _userrepository.Save();
        }

        public async Task UpdateUserRole(int userId, UserDTO dto)
        {
            if (!ValidateAdmin())
            {
                throw new UnauthorizedAccessException("You do not have the authority to change user permissions.");
            }

            var user = await _userrepository.GetUserById(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.RoleId = dto.RoleId;

            await _userrepository.Update(user);
            await _userrepository.Save();
        }
    }
}
