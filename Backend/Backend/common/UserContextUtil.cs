using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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

        public int GetUserId()
        {
            var id = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id == null ? 0 : int.Parse(id);
        }

        public string GetEmail()
        {
            return User?.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        public string GetRole()
        {
            var roleId = User?.FindFirst(ClaimTypes.Role)?.Value;
            return roleId == null ? "" : roleId;
        }

        public string GetName()
        {
            return User?.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }
    }
}