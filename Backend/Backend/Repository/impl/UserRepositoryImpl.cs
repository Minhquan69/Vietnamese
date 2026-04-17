using Backend.Data;
using Backend.dto;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class UserRepositoryImpl : UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }
        /*
         * đăng nhập người dùng
         * thuphuong21072004
         */
        public async Task<dynamic?> Login(string email, string password)
        {
            var query = from user in _context.Users
                        join role in _context.Roles on user.RoleId equals role.RoleId
                        where user.Email == email && user.Password == password
                        select new
                        {
                           
                            UserId = user.UserId,
                            Name = user.Name,
                            Email = user.Email,
                            RoleName = role.RoleName
                        };

            return await query.FirstOrDefaultAsync();
        }
        /*
         * kiểm tra email đã tồn tại
         * thuphuong21072004
         */
        public async Task<bool> IsEmailExist(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }
        /*
         * đăng ký người dùng mới
         * 
         * thuphuong21072004
         */
        
        public async Task<User> Register(string name, string email, string password)
        {
            
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "User");

            if (role == null)
            {
                throw new Exception("Role 'User' không tồn tại trong Database. Hãy kiểm tra lại bảng Roles!");
            }

            var user = new User
            {
                Name = name,
                Email = email,
                Password = password,
                RoleId = role.RoleId, 
                Status = 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
        /*
         * lấy thông tin người dùng theo id
         * thuphuong21072004
         */
        public async Task<User?> GetUserById(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
        /*
         * cập nhật thông tin người dùng
         * thuphuong21072004
         */
        public async Task Update(User user)
        {
            _context.Users.Update(user);
        }
        /*
         * lưu thay đổi vào database
         * thuphuong21072004
         */
        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
        /*
         * đếm số lượng người dùng theo điều kiện tìm kiếm
         * thuphuong21072004
         */
        public async Task<int> CountUsers(string? email, int? status, int? roleId)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(email))
                query = query.Where(u => u.Email.Contains(email));

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            return await query.CountAsync();
        }
        /*
         * lấy danh sách người dùng theo điều kiện tìm kiếm và phân trang
         * thuphuong21072004
         */
        public async Task<List<UserDTO>> GetUsers(string? email, int? status, int? roleId, int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(email))
                query = query.Where(u => u.Email.Contains(email));

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            return await query
                .OrderByDescending(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDTO
                {
                    Id = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Status = u.Status,
                    RoleId = u.RoleId 
                })
                .ToListAsync();
        }
        /*
         * lấy user theo email
         * 
         * thuphuong21072004
         */
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        /*
         * lấy quyền theo email
         * 
         * thuphuong21072004
         */
        public async Task<dynamic?> GetUserWithRole(string email)
        {
            var query = from user in _context.Users
                        join role in _context.Roles on user.RoleId equals role.RoleId
                        where user.Email == email 
                        select new
                        {
                            user.UserId,
                            user.Name,
                            user.Email,
                            user.Password, 
                            RoleName = role.RoleName
                        };

            return await query.FirstOrDefaultAsync();
        }
    }
}