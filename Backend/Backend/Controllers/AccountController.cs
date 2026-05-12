using Backend.Common;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly UserContextUtil _userContext;

        public AccountController(UserService userservice, UserContextUtil userContext)
        {
            _userService = userservice;
            _userContext = userContext;
        }
        /*
         * xác thực đăng nhập và trả về token     
         * 06/03/2026 
         * thuphuong21072004
         */
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            return Ok(await _userService.Login(dto));
        }
        /*
         * đăng ký tài khoản
         * 16/03/2026
         * thuphuong21072004
         */
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            return Ok(await _userService.Register(dto));
        }
        /*
         * lấy thông tin user hiện tại từ token
         * 16/03/2026
         * thuphuong21072004
         */
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var id = _userContext.GetUserId();
            if (id <= 0)
            {
                return Unauthorized();
            }

            var me = await _userService.GetMeProfile(id);
            if (me == null)
            {
                return Unauthorized();
            }

            return Ok(me);
        }
        /*
         * đổi mật khẩu tài khoản
         * 16/03/2026
         * thuphuong21072004
         */
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            await _userService.ChangePassword(_userContext.GetUserId(), dto);
            return Ok("Changed password");
        }
        /*
         * cập nhật thông tin cá nhân của user
         * 
         * thuphuong21072004
         */
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDTO dto)
        {
            await _userService.UpdateProfile(_userContext.GetUserId(), dto);
            return Ok();
        }
        /*
         * lấy danh sách người dùng theo điều kiện tìm kiếm và phân trang
         * 
         * thuphuong21072004
         */
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
    [FromQuery] string? email,
    [FromQuery] int? status,
    [FromQuery] int? roleId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            return Ok(await _userService.GetUsers(email, status, roleId, page, pageSize));
        }
        /*
         * cập nhật trạng thái người dùng
         * 
         * thuphuong21072004
         */
        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] int status)
        {
            await _userService.UpdateUserStatus(id, status);
            return Ok(new { message = "Updated status" });
        }
        /*
         * cập nhật vai trò người dùng
         * 
         * thuphuong21072004
         */
        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UserDTO dto)
        {
            await _userService.UpdateUserRole(id, dto);
            return Ok(new { message = "Updated status" });
        }

    }
}