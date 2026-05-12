using Backend.Contracts;
using Backend.dto;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.V1
{
    [ApiController]
    [Route("api/v1/vocabulary")]
    public class VocabularyController : ControllerBase
    {
        private readonly VocabularyLearningService _vocabularyLearningService;

        public VocabularyController(VocabularyLearningService vocabularyLearningService)
        {
            _vocabularyLearningService = vocabularyLearningService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<VocabularyListResultDto>>> Search(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 24)
        {
            var data = await _vocabularyLearningService.SearchAsync(q, page, pageSize);
            return Ok(ApiResponse<VocabularyListResultDto>.Ok(data));
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<VocabularyCardDto>>> GetById([FromRoute] int id)
        {
            var data = await _vocabularyLearningService.GetVocabularyAsync(id);
            if (data == null)
            {
                return NotFound(ApiResponse<VocabularyCardDto>.Fail("Vocabulary not found"));
            }

            return Ok(ApiResponse<VocabularyCardDto>.Ok(data));
        }

        [Authorize]
        [HttpGet("{id:int}/me")]
        public async Task<ActionResult<ApiResponse<UserVocabularyCardDto>>> GetMine([FromRoute] int id)
        {
            var data = await _vocabularyLearningService.GetUserCardAsync(id);
            if (data == null)
            {
                return NotFound(ApiResponse<UserVocabularyCardDto>.Fail("Not found"));
            }

            return Ok(ApiResponse<UserVocabularyCardDto>.Ok(data));
        }

        [Authorize]
        [HttpGet("me/deck")]
        public async Task<ActionResult<ApiResponse<List<UserVocabularyCardDto>>>> GetDeck(
            [FromQuery] int limit = 24)
        {
            try
            {
                var data = await _vocabularyLearningService.GetDeckAsync(limit);
                return Ok(ApiResponse<List<UserVocabularyCardDto>>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPost("{id:int}/review")]
        public async Task<ActionResult<ApiResponse<ReviewResultDto>>> Review(
            [FromRoute] int id,
            [FromBody] ReviewGradeDto body)
        {
            try
            {
                var data = await _vocabularyLearningService.SubmitReviewAsync(id, body.Grade);
                return Ok(ApiResponse<ReviewResultDto>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<ReviewResultDto>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpPut("{id:int}/saved")]
        public async Task<ActionResult<ApiResponse<SavedToggleResultDto>>> SetSaved(
            [FromRoute] int id,
            [FromBody] SavedRequestDto body)
        {
            try
            {
                var data = await _vocabularyLearningService.SetSavedAsync(id, body.Saved);
                return Ok(ApiResponse<SavedToggleResultDto>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<SavedToggleResultDto>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpGet("me/stats")]
        public async Task<ActionResult<ApiResponse<VocabularyStatsDto>>> Stats()
        {
            try
            {
                var data = await _vocabularyLearningService.GetStatsAsync();
                return Ok(ApiResponse<VocabularyStatsDto>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("me/saved")]
        public async Task<ActionResult<ApiResponse<List<UserVocabularyCardDto>>>> SavedList()
        {
            try
            {
                var data = await _vocabularyLearningService.GetSavedListAsync();
                return Ok(ApiResponse<List<UserVocabularyCardDto>>.Ok(data));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}
