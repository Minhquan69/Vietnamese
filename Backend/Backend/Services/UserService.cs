using Backend.dto;
using Microsoft.AspNetCore.Http;

namespace Backend.Services
{
    public interface UserService
    {
        Task<object?> Login(LoginDTO request);
        Task<object> Register(RegisterDTO request);
        Task<object?> RefreshTokenPair(RefreshTokenRequestDto request);
        Task<object> ForgotPassword(ForgotPasswordRequestDto request);
        Task ResetPassword(ResetPasswordRequestDto request);
        Task<string> UploadAvatar(int userId, IFormFile file);
        Task<object?> GetMeProfile(int userId);

        Task ChangePassword(int userId, ChangePasswordDTO dto);
        Task UpdateProfile(int userId, UserDTO dto);
        Task<object> GetUsers(string? email, int? status, int? roleId, int page, int pageSize);
        Task UpdateUserStatus(int userId, int status);
        Task UpdateUserRole(int id, UserDTO dto);
    }
}
