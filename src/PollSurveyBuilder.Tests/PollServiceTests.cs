using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.Persistence;
using PollSurveyBuilder.Infrastructure.Services;
using Xunit;

namespace PollSurveyBuilder.Tests
{
    public class PollServiceTests
    {
        private static (AppDbContext db, IDistributedCache cache) NewContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            return (db, cache);
        }

        [Fact]
        public async Task CreateAsync_SingleChoice_PersistsAllSuppliedOptions()
        {
            var (db, cache) = NewContext();
            var service = new PollService(db, cache, NullLogger<PollService>.Instance);

            var dto = new CreatePollDTO
            {
                Question = "Best pizza topping?",
                Type = PollType.SingleChoice,
                Options = new List<string> { "Pepperoni", "Mushroom", "Pineapple" },
            };

            var result = await service.CreateAsync(dto, userId: "user-1", baseUrl: "https://example.com");

            Assert.Equal(6, result.Code.Length);
            var saved = await db.Polls.Include(p => p.Options).SingleAsync(p => p.Code == result.Code);
            Assert.Equal(3, saved.Options.Count);
        }

        [Fact]
        public async Task CreateAsync_YesNo_GeneratesExactlyTwoFixedOptions()
        {
            var (db, cache) = NewContext();
            var service = new PollService(db, cache, NullLogger<PollService>.Instance);

            var dto = new CreatePollDTO { Question = "Do you like tests?", Type = PollType.YesNo };
            var result = await service.CreateAsync(dto, userId: null, baseUrl: "https://example.com");

            var saved = await db.Polls.Include(p => p.Options).SingleAsync(p => p.Code == result.Code);
            Assert.Equal(new[] { "Yes", "No" }, saved.Options.OrderBy(o => o.OrderIndex).Select(o => o.Text));
        }

        [Fact]
        public async Task CreateAsync_Rating_GeneratesFiveStarOptions()
        {
            var (db, cache) = NewContext();
            var service = new PollService(db, cache, NullLogger<PollService>.Instance);

            var dto = new CreatePollDTO { Question = "Rate the workshop", Type = PollType.Rating };
            var result = await service.CreateAsync(dto, userId: null, baseUrl: "https://example.com");

            var saved = await db.Polls.Include(p => p.Options).SingleAsync(p => p.Code == result.Code);
            Assert.Equal(5, saved.Options.Count);
        }

        [Fact]
        public async Task GetForVotingAsync_UnknownCode_ReturnsNull()
        {
            var (db, cache) = NewContext();
            var service = new PollService(db, cache, NullLogger<PollService>.Instance);

            var poll = await service.GetForVotingAsync("doesnotexist", "token");

            Assert.Null(poll);
        }

        [Fact]
        public async Task CloseAsync_WrongUser_DoesNotClosePoll()
        {
            var (db, cache) = NewContext();
            var service = new PollService(db, cache, NullLogger<PollService>.Instance);

            var dto = new CreatePollDTO { Question = "Q", Type = PollType.YesNo };
            var created = await service.CreateAsync(dto, userId: "owner", baseUrl: "https://example.com");

            var closed = await service.CloseAsync(created.Code, userId: "someone-else");

            Assert.False(closed);
        }
    }
}