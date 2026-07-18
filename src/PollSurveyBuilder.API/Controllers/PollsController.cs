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
        private readonly IConfiguration _configuration;

        public PollsController(
            IPollService pollService,
            IQRCodeService qrCodeService,
            IValidator<CreatePollDTO> createValidator,
            IConfiguration configuration)
        {
            _pollService = pollService;
            _qrCodeService = qrCodeService;
            _createValidator = createValidator;
            _configuration = configuration;
        }

        /// <summary>
        /// The frontend's public URL (e.g. https://ballote.vercel.app) - poll share links and
        /// the QR code must point here, NOT at the API's own host, since /poll/{code} is a
        /// React Router route that only exists on the frontend. Configure via
        /// "Frontend:BaseUrl" in appsettings, or the Frontend__BaseUrl env var in production.
        /// Falls back to the API's own host only so local dev keeps working if it's unset.
        /// </summary>
        private string FrontendBaseUrl =>
            _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}";

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
            var apiBaseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = await _pollService.CreateAsync(dto, userId, FrontendBaseUrl);
            // QrCodeUrl is an API endpoint the frontend fetches from - that one genuinely
            // does live on the API's own host, unlike ShareUrl (see FrontendBaseUrl above).
            result.QrCodeUrl = $"{apiBaseUrl}/api/polls/{result.Code}/qrcode";

            return CreatedAtAction(nameof(GetForVoting), new { code = result.Code }, result);
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<PollVoteViewDTO>> GetForVoting(string code)
        {
            var voterToken = Request.Headers["X-Voter-Token"].FirstOrDefault() ?? string.Empty;
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

            var voteUrl = $"{FrontendBaseUrl}/poll/{code}";
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