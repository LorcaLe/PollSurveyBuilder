using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;
using System.Security.Claims;

namespace PollSurveyBuilder.API.Controllers
{
    [ApiController]
    [Route("api/polls")]
    public class PollsController : ControllerBase
    {
        private readonly IPollService _pollService;
        private readonly IQRCodeService _qrCodeService;
        private readonly IValidator<CreatePollDTO> _createValidator;

        public PollsController(IPollService pollService, IQRCodeService qrCodeService, IValidator<CreatePollDTO> createValidator)
        {
            _pollService = pollService;
            _qrCodeService = qrCodeService;
            _createValidator = createValidator;
        }

        /// <summary>Creating a poll is the "Admin" action the coursework brief asks to protect with Identity.</summary>
        [HttpPost]
        [Authorize]
        [EnableRateLimiting("create-poll")]
        public async Task<ActionResult<CreatePollResultDTO>> Create(CreatePollDTO dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ValidationProblem(BuildModelState(validation));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var baseUrl = "http://localhost:5173";

            var result = await _pollService.CreateAsync(dto, userId, baseUrl);
            return CreatedAtAction(nameof(GetForVoting), new { code = result.Code }, result);
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<PollVoteViewDTO>> GetForVoting(string code)
        {
            var voterToken = (string)(HttpContext.Items["voter_token"] ?? string.Empty);
            var poll = await _pollService.GetForVotingAsync(code, voterToken);
            return poll is null ? NotFound() : Ok(poll);
        }

        [HttpGet("{code}/results")]
        public async Task<ActionResult<PollResultsDTO>> GetResults(string code)
        {
            var results = await _pollService.GetResultsAsync(code);
            return results is null ? NotFound() : Ok(results);
        }

        [HttpGet("{code}/qrcode")]
        public async Task<ActionResult> GetQrCode(string code)
        {
            var poll = await _pollService.GetForVotingAsync(code, "");
            if (poll is null) return NotFound();

            var voteUrl = $"http://localhost:5173/poll/{code}";
            return Ok(new { dataUrl = _qrCodeService.GenerateBase64(voteUrl) });
        }

        [HttpPost("{code}/close")]
        [Authorize]
        public async Task<ActionResult> Close(string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var ok = await _pollService.CloseAsync(code, userId);
            return ok ? NoContent() : NotFound();
        }

        [HttpGet("mine")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<PollSummaryDTO>>> Mine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var polls = await _pollService.GetMineAsync(userId);
            return Ok(polls);
        }

        private static Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary BuildModelState(
            FluentValidation.Results.ValidationResult validation)
        {
            var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
            foreach (var error in validation.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return modelState;
        }
    }
}
