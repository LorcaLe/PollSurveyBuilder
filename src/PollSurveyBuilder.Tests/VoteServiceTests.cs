using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.Persistence;
using PollSurveyBuilder.Infrastructure.Services;
using Xunit;

namespace PollSurveyBuilder.Tests
{
    public class VoteServiceTests
    {
        private static (AppDbContext db, PollService pollService, VoteService voteService) NewContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var pollService = new PollService(db, cache, NullLogger<PollService>.Instance);
            var voteService = new VoteService(db, pollService);
            return (db, pollService, voteService);
        }

        [Fact]
        public async Task CastAsync_FirstVote_Succeeds_AndIncrementsCount()
        {
            var (db, pollService, voteService) = NewContext();
            var poll = await pollService.CreateAsync(
                new CreatePollDTO { Question = "Q", Type = PollType.YesNo }, null, "https://example.com");
            var yesOptionId = db.PollOptions.First(o => o.Text == "Yes").Id;

            var result = await voteService.CastAsync(poll.Code, new CastVoteDTO { OptionId = yesOptionId }, "voter-a");

            Assert.Equal(CastVoteOutcome.Success, result.Outcome);
            Assert.Equal(1, result.Results!.TotalVotes);
        }

        [Fact]
        public async Task CastAsync_SameVoterTwice_SecondCallIsRejected()
        {
            var (db, pollService, voteService) = NewContext();
            var poll = await pollService.CreateAsync(
                new CreatePollDTO { Question = "Q", Type = PollType.YesNo }, null, "https://example.com");
            var yesOptionId = db.PollOptions.First(o => o.Text == "Yes").Id;

            await voteService.CastAsync(poll.Code, new CastVoteDTO { OptionId = yesOptionId }, "voter-a");
            var second = await voteService.CastAsync(poll.Code, new CastVoteDTO { OptionId = yesOptionId }, "voter-a");

            Assert.Equal(CastVoteOutcome.AlreadyVoted, second.Outcome);
        }

        [Fact]
        public async Task CastAsync_DifferentVoters_BothCounted()
        {
            var (db, pollService, voteService) = NewContext();
            var poll = await pollService.CreateAsync(
                new CreatePollDTO { Question = "Q", Type = PollType.YesNo }, null, "https://example.com");
            var yesOptionId = db.PollOptions.First(o => o.Text == "Yes").Id;

            await voteService.CastAsync(poll.Code, new CastVoteDTO { OptionId = yesOptionId }, "voter-a");
            var result = await voteService.CastAsync(poll.Code, new CastVoteDTO { OptionId = yesOptionId }, "voter-b");

            Assert.Equal(CastVoteOutcome.Success, result.Outcome);
            Assert.Equal(2, result.Results!.TotalVotes);
        }

        [Fact]
        public async Task CastAsync_ClosedPoll_IsRejected()
        {
            var (db, pollService, voteService) = NewContext();
            var poll = await pollService.CreateAsync(
                new CreatePollDTO { Question = "Q", Type = PollType.YesNo }, "owner", "https://example.com");
            await pollService.CloseAsync(poll.Code, "owner");
            var yesOptionId = db.PollOptions.First(o => o.Text == "Yes").Id;

            var result = await voteService.CastAsync(poll.Code, new CastVoteDTO { OptionId = yesOptionId }, "voter-a");

            Assert.Equal(CastVoteOutcome.PollClosed, result.Outcome);
        }

        [Fact]
        public async Task CastAsync_InvalidOptionId_IsRejected()
        {
            var (db, pollService, voteService) = NewContext();
            var poll = await pollService.CreateAsync(
                new CreatePollDTO { Question = "Q", Type = PollType.YesNo }, null, "https://example.com");

            var result = await voteService.CastAsync(poll.Code, new CastVoteDTO { OptionId = 999_999 }, "voter-a");

            Assert.Equal(CastVoteOutcome.InvalidOption, result.Outcome);
        }

        [Fact]
        public async Task CastAsync_OpenTextPoll_StoresAnswerWithoutOption()
        {
            var (db, pollService, voteService) = NewContext();
            var poll = await pollService.CreateAsync(
                new CreatePollDTO { Question = "Any feedback?", Type = PollType.OpenText }, null, "https://example.com");

            var result = await voteService.CastAsync(poll.Code, new CastVoteDTO { TextAnswer = "Great workshop!" }, "voter-a");

            Assert.Equal(CastVoteOutcome.Success, result.Outcome);
            Assert.Contains("Great workshop!", result.Results!.OpenTextAnswers);
        }
    }
}