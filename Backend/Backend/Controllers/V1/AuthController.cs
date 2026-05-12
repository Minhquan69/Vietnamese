using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Backend.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly UserContextUtil _userContext;

        public AuthController(UserService userService, UserContextUtil userContext)
        {
            _userService = userService;
            _userContext = userContext;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginDTO dto)
        {
            var result = await _userService.Login(dto);
            if (result == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid email or password"));
            }

            return Ok(ApiResponse<object>.Ok(result, "Login success"));
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterDTO dto)
        {
            var result = await _userService.Register(dto);
            return Ok(ApiResponse<object>.Ok(result, "Register success"));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<object>>> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _userService.RefreshTokenPair(dto);
            if (result == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid or expired refresh token"));
            }

            return Ok(ApiResponse<object>.Ok(result, "Token refreshed"));
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var result = await _userService.ForgotPassword(dto);
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            await _userService.ResetPassword(dto);
            return Ok(ApiResponse<object>.Ok(new { message = "Password has been reset." }));
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<object>>> Profile()
        {
            var me = await _userService.GetMeProfile(_userContext.GetUserId());
            if (me == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Not authenticated"));
            }

            return Ok(ApiResponse<object>.Ok(me));
        }

        [Authorize]
        [HttpPost("me/avatar")]
        [RequestSizeLimit(5_242_880)]
        public async Task<ActionResult<ApiResponse<object>>> UploadAvatar(IFormFile file)
        {
            var path = await _userService.UploadAvatar(_userContext.GetUserId(), file);
            return Ok(ApiResponse<object>.Ok(new { avatarUrl = path }));
        }
    }
}
