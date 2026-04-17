using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Common
{
    public class UserContextUtil
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextUtil(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        /* * Lấy UserId từ Token
         * thuphuong21072004 
         */
        public int GetUserId()
        {
            // Kiểm tra cả hai loại claim phổ biến để tránh lỗi null
            var id = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return id == null ? 0 : int.Parse(id);
        }

        /*
         * Lấy Email của người dùng
         */
        public string GetEmail()
        {
            return User?.FindFirst(ClaimTypes.Email)?.Value
                   ?? User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? "";
        }

        /*
         * Lấy Tên hiển thị
         */
        public string GetName()
        {
            return User?.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }

        /* * Lấy RoleName (Admin hoặc User)
         * Hàm này sẽ trả về giá trị bạn đã gán ở JwtService: userData.RoleName
         */
        public string GetRole()
        {
            return User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        /*
         * Kiểm tra nhanh xem có phải Admin không
         * 
         */
        public bool IsAdmin()
        {
            return GetRole().Equals(common.Constant.Role.Admin, StringComparison.OrdinalIgnoreCase);
        }
        /*
         * kiểm tra có phải User không
         */
        public bool IsUsser()
        {
            return GetRole().Equals(common.Constant.Role.User, StringComparison.OrdinalIgnoreCase);
        }
        /*
         * kiểm tra có phải moderator không
         */
        public bool IsModerator()
        {
            return GetRole().Equals(common.Constant.Role.Moderator, StringComparison.OrdinalIgnoreCase);
        }
    }
}