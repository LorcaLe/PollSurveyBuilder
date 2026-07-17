using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.Persistence;

namespace PollSurveyBuilder.Infrastructure.Services
{
    public class VoteService : IVoteService
    {
        private readonly AppDbContext _db;
        private readonly IPollService _pollService;

        public VoteService(AppDbContext db, IPollService pollService)
        {
            _db = db;
            _pollService = pollService;
        }

        public async Task<CastVoteResult> CastAsync(string code, CastVoteDTO dto, string voterToken)
        {
            var poll = await _db.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Code == code);

            if (poll is null)
                return new CastVoteResult(CastVoteOutcome.PollNotFound, null);

            if (!poll.IsAcceptingVotes())
                return new CastVoteResult(CastVoteOutcome.PollClosed, null);

            var alreadyVoted = await _db.Votes.AnyAsync(v => v.PollId == poll.Id && v.VoterToken == voterToken);
            if (alreadyVoted)
                return new CastVoteResult(CastVoteOutcome.AlreadyVoted, null);

            var vote = new Vote
            {
                PollId = poll.Id,
                VoterToken = voterToken,
            };

            if (poll.Type == PollType.OpenText)
            {
                if (string.IsNullOrWhiteSpace(dto.TextAnswer))
                    return new CastVoteResult(CastVoteOutcome.InvalidOption, null);
                vote.TextAnswer = dto.TextAnswer.Trim();
            }
            else
            {
                var option = poll.Options.FirstOrDefault(o => o.Id == dto.OptionId);
                if (option is null)
                    return new CastVoteResult(CastVoteOutcome.InvalidOption, null);
                vote.OptionId = option.Id;
            }

            _db.Votes.Add(vote);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // The unique (PollId, VoterToken) index caught a race (e.g. a double-click
                // firing two requests) that the earlier AnyAsync check missed.
                return new CastVoteResult(CastVoteOutcome.AlreadyVoted, null);
            }

            // Bypass the cache to get the just-written vote, then refresh the cache
            // so the next GET /results (and every other viewer) sees the new count.
            var freshResults = await _pollService.ComputeResultsAsync(code);
            if (freshResults != null)
            {
                await _pollService.CacheResultsAsync(freshResults);
            }

            return new CastVoteResult(CastVoteOutcome.Success, freshResults);
        }
    }
}
