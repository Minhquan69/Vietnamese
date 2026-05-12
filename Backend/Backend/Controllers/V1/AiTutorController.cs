using Backend.Common;
using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/ai-tutor")]
    [Authorize]
    public class AiTutorController : ControllerBase
    {
        private readonly AiTutorService _tutor;
        private readonly UserContextUtil _user;

        public AiTutorController(AiTutorService tutor, UserContextUtil user)
        {
            _tutor = tutor;
            _user = user;
        }

        [HttpPost("chat")]
        public async Task<ActionResult<ApiResponse<AiTutorChatResponseDto>>> Chat([FromBody] AiTutorChatRequestDto body)
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            try
            {
                var data = await _tutor.ChatAsync(userId, body);
                return Ok(ApiResponse<AiTutorChatResponseDto>.Ok(data));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<AiTutorChatResponseDto>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<AiTutorChatResponseDto>.Fail(ex.Message));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ApiResponse<AiTutorChatResponseDto>.Fail(ex.Message));
            }
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<ApiResponse<List<TutorConversationSummaryDto>>>> Conversations()
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var data = await _tutor.ListConversationsAsync(userId);
            return Ok(ApiResponse<List<TutorConversationSummaryDto>>.Ok(data));
        }

        [HttpGet("conversations/{id:int}/messages")]
        public async Task<ActionResult<ApiResponse<List<TutorMessageDto>>>> Messages([FromRoute] int id)
        {
            var userId = _user.GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var data = await _tutor.GetMessagesAsync(userId, id);
            return Ok(ApiResponse<List<TutorMessageDto>>.Ok(data));
        }
    }
}
