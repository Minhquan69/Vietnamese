using Backend.Common;
using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/gamification")]
    [Authorize]
    public class GamificationController : ControllerBase
    {
        private readonly GamificationService _gamification;
        private readonly UserContextUtil _user;

        public GamificationController(GamificationService gamification, UserContextUtil user)
        {
            _gamification = gamification;
            _user = user;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<GamificationStateDto>>> Me()
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            try
            {
                var data = await _gamification.GetMyStateAsync();
                return Ok(ApiResponse<GamificationStateDto>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpGet("leaderboard")]
        public async Task<ActionResult<ApiResponse<List<GamificationLeaderboardRowDto>>>> Leaderboard(
            [FromQuery] int take = 50)
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var data = await _gamification.GetLeaderboardAsync(take);
            return Ok(ApiResponse<List<GamificationLeaderboardRowDto>>.Ok(data.ToList()));
        }
    }
}
