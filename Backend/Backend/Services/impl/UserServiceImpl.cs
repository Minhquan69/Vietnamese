using Backend.Common;
using Backend.dto;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.impl
{
    public class UserServiceImpl : UserService
    {
        private readonly UserRepository _userrepository;

        private readonly JwtService _jwtService;

        public UserServiceImpl(UserRepository userrepository, JwtService jwtService)
        {
            _userrepository = userrepository;
            _jwtService = jwtService;
        }
        /*
         * đăng nhập thông tin 
         * 08/03/2026
         * thuphuong21072004
         */
        public async Task<object> Login(LoginDTO request)
        {
            var user = await _userrepository.Login(request.Email, request.Password);

            if (user == null)
                throw new Exception("Invalid email or password");

            if (user.Status != 1)
                throw new Exception("Tài khoản bị khóa");

            var userDto = new UserDTO
            {
                Id = user.UserId,
                Email = user.Email,
                RoleId = user.RoleId,
                Status = user.Status,
                Name = user.Name
            };

            var token = _jwtService.GenerateToken(userDto);

            return new
            {
                token = token,
            };
        }
        /*
         * đăng ký tài khoản
         * 08/03/2026
         * thuphuong21072004
         */
        public async Task<object> Register(RegisterDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Name))
            {
                throw new Exception("Dữ liệu không hợp lệ");
            }

            var email = request.Email.Trim().ToLower();

            var isExist = await _userrepository.IsEmailExist(email);
            if (isExist)
                throw new Exception("Email đã tồn tại");

            var user = await _userrepository.Register(
                request.Name,
                email,
                request.Password
            );
            var userDto = new UserDTO
            {
                Id = user.UserId,
                Email = user.Email,
                Name = user.Name,
                RoleId = 2,
                Status = user.Status
            };
            var token = _jwtService.GenerateToken(userDto);
            return new
            {
                token = token
            };
        }
        /*
         * đổi mật khẩu tài khoản
         * 08/03/2026
         * thuphuong21072004
         */
        public async Task ChangePassword(int userId, ChangePasswordDTO dto)
        {

            if (string.IsNullOrEmpty(dto.OldPassword) || string.IsNullOrEmpty(dto.NewPassword))
                throw new Exception("Password cannot be empty");

            var user = await _userrepository.GetUserById(userId);
            if (user == null)
                throw new Exception("User not found");

            if (user.Password != dto.OldPassword)
                throw new Exception("Old password is incorrect");

            if (dto.OldPassword == dto.NewPassword)
                throw new Exception("New password must be different");

            user.Password = dto.NewPassword;

            await _userrepository.Update(user);
            await _userrepository.Save();
        }
        /*
         * cập nhập thông tin các nhân
         * 
         * thuphuong21072004
         */
        public async Task UpdateProfile(int userId, UserDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Email))
                throw new Exception("Name and Email cannot be empty");

            var user = await _userrepository.GetUserById(userId);
            if (user == null)
                throw new Exception("User not found");

            var isExist = await _userrepository.IsEmailExist(dto.Email);
            if (isExist && user.Email != dto.Email)
                throw new Exception("Email already exists");

            user.Name = dto.Name;
            user.Email = dto.Email;

            await _userrepository.Update(user);
            await _userrepository.Save();
        }
        /*
         * lấy danh sách người dùng theo trạng thái, email, quyền
         * 
         * thuphuong221072004
         */
        public async Task<object> GetUsers(string? email, int? status, int? roleId, int page, int pageSize)
        {
            var users = await _userrepository.GetUsers(email, status, roleId, page, pageSize);
            var total = await _userrepository.CountUsers(email, status, roleId);

            return new
            {
                total,
                page,
                pageSize,
                data = users
            };
        }
        /*
         * cập nhật trạng thái người dùng
         * 
         * thuphuong21072004
         */
        public async Task UpdateUserStatus(int userId, int status)
        {
            var user = await _userrepository.GetUserById(userId);

            if (user == null)
                throw new Exception("User not found");

            if (status != 1 && status != 0 && status != -1)
                throw new Exception("Invalid status");

            user.Status = status;

            await _userrepository.Update(user);
            await _userrepository.Save();
        }
        /*
         * cập nhật quyền người dùng
         * 
         * thuphuong21072004
         */
        public async Task UpdateUserRole(int userId, UserDTO dto)
        {
            var user = await _userrepository.GetUserById(userId);

            if (user == null)
                throw new Exception("User not found");
            user.RoleId = dto.RoleId;

            await _userrepository.Update(user);
            await _userrepository.Save();
        }
    }
}