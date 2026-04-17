using Backend.dto;

namespace Backend.Services
{
    public interface UserService
    {
        Task<object> Login(LoginDTO request);
        Task<object> Register(RegisterDTO request);
        Task ChangePassword(int userId, ChangePasswordDTO dto);
        Task UpdateProfile(int userId, UserDTO dto);
        Task<object> GetUsers(string? email, int? status, int? roleId, int page, int pageSize);
        Task UpdateUserStatus(int userId, int status);
        Task UpdateUserRole(int id, UserDTO dto);
    }
}
