using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using PollSurveyBuilder.API.Hubs;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;

namespace PollSurveyBuilder.API.Controllers
{
    [ApiController]
    [Route("api/polls/{code}/vote")]
    [EnableRateLimiting("vote")]
    public class VotesController : ControllerBase
    {
        private readonly IVoteService _voteService;
        private readonly IValidator<CastVoteDTO> _validator;
        private readonly IHubContext<PollHub> _hub;

        public VotesController(IVoteService voteService, IValidator<CastVoteDTO> validator, IHubContext<PollHub> hub)
        {
            _voteService = voteService;
            _validator = validator;
            _hub = hub;
        }

        [HttpPost]
        public async Task<ActionResult> Cast(string code, CastVoteDTO dto)
        {
            var validation = await _validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

            var voterToken = Request.Headers["X-Voter-Token"].FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrEmpty(voterToken))
                return BadRequest(new { message = "Missing voter identity." });

            var result = await _voteService.CastAsync(code, dto, voterToken);

            switch (result.Outcome)
            {
                case CastVoteOutcome.PollNotFound:
                    return NotFound();
                case CastVoteOutcome.PollClosed:
                    return Conflict(new { message = "This poll is closed." });
                case CastVoteOutcome.AlreadyVoted:
                    return Conflict(new { message = "You've already voted on this poll." });
                case CastVoteOutcome.InvalidOption:
                    return BadRequest(new { message = "Invalid option selected." });
            }

            // Push the fresh tally to every browser currently watching this poll's
            // results page - no need for them to refresh or poll the API.
            await _hub.Clients.Group(PollHub.GroupName(code)).SendAsync("resultsUpdated", result.Results);

            return Ok(result.Results);
        }
    }
}