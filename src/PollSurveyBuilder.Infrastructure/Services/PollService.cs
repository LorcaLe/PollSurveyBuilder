using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PollSurveyBuilder.Application.Common;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Domain.Enums;
using PollSurveyBuilder.Infrastructure.Persistence;
using System.Text.Json;

namespace PollSurveyBuilder.Infrastructure.Services
{
    public class PollService : IPollService
    {
        private readonly AppDbContext _db;
        private readonly IDistributedCache _cache;

        public PollService(AppDbContext db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }

        private static string ResultsCacheKey(string code) => $"poll:results:{code}";

        public async Task<CreatePollResultDTO> CreateAsync(CreatePollDTO dto, string? userId, string baseUrl)
        {
            var poll = new Poll
            {
                Code = await GenerateUniqueCodeAsync(),
                Question = dto.Question.Trim(),
                Type = dto.Type,
                Status = PollStatus.Open,
                CreatedByUserId = userId,
                ExpiresAt = dto.ExpiresInMinutes.HasValue
                    ? DateTime.UtcNow.AddMinutes(dto.ExpiresInMinutes.Value)
                    : null,
            };

            poll.Options = BuildOptions(dto);

            _db.Polls.Add(poll);
            await _db.SaveChangesAsync();

            var shareUrl = $"{baseUrl.TrimEnd('/')}/poll/{poll.Code}";

            return new CreatePollResultDTO
            {
                Code = poll.Code,
                ShareUrl = shareUrl,
                QrCodeUrl = $"{baseUrl.TrimEnd('/')}/api/polls/{poll.Code}/qrcode",
            };
        }

        private static List<PollOption> BuildOptions(CreatePollDTO dto)
        {
            var options = new List<PollOption>();

            switch (dto.Type)
            {
                case PollType.SingleChoice:
                    for (int i = 0; i < dto.Options.Count; i++)
                    {
                        options.Add(new PollOption { Text = dto.Options[i].Trim(), OrderIndex = i });
                    }
                    break;

                case PollType.YesNo:
                    options.Add(new PollOption { Text = "Yes", OrderIndex = 0 });
                    options.Add(new PollOption { Text = "No", OrderIndex = 1 });
                    break;

                case PollType.Rating:
                    for (int star = 1; star <= 5; star++)
                    {
                        options.Add(new PollOption { Text = $"{star} star{(star > 1 ? "s" : "")}", OrderIndex = star - 1 });
                    }
                    break;

                case PollType.OpenText:
                    // no options - answers go straight into Vote.TextAnswer
                    break;
            }

            return options;
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            // Extremely unlikely to collide, but a poll link with a duplicate code
            // would be a broken app, so we check and retry.
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var code = ShortCodeGenerator.Generate();
                var exists = await _db.Polls.AnyAsync(p => p.Code == code);
                if (!exists) return code;
            }
            throw new InvalidOperationException("Could not generate a unique poll code after 5 attempts.");
        }

        public async Task<PollVoteViewDTO?> GetForVotingAsync(string code, string voterToken)
        {
            var poll = await _db.Polls
                .Include(p => p.Options.OrderBy(o => o.OrderIndex))
                .FirstOrDefaultAsync(p => p.Code == code);

            if (poll is null) return null;

            var alreadyVoted = await _db.Votes.AnyAsync(v => v.PollId == poll.Id && v.VoterToken == voterToken);

            return new PollVoteViewDTO
            {
                Code = poll.Code,
                Question = poll.Question,
                Type = poll.Type,
                IsOpen = poll.IsAcceptingVotes(),
                ExpiresAt = poll.ExpiresAt,
                AlreadyVoted = alreadyVoted,
                Options = poll.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select(o => new PollOptionDTO { Id = o.Id, Text = o.Text })
                    .ToList(),
            };
        }

        public async Task<PollResultsDTO?> GetResultsAsync(string code)
        {
            var cached = await _cache.GetStringAsync(ResultsCacheKey(code));
            if (cached != null)
            {
                return JsonSerializer.Deserialize<PollResultsDTO>(cached);
            }

            var results = await ComputeResultsAsync(code);
            if (results is null) return null;

            await CacheResultsAsync(results);
            return results;
        }

        /// <summary>Recomputes results straight from the database, bypassing the cache. Used by VoteService right after a write.</summary>
        public async Task<PollResultsDTO?> ComputeResultsAsync(string code)
        {
            var poll = await _db.Polls
                .Include(p => p.Options.OrderBy(o => o.OrderIndex))
                    .ThenInclude(o => o.Votes)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Code == code);

            if (poll is null) return null;

            var totalVotes = poll.Votes.Count;

            return new PollResultsDTO
            {
                Code = poll.Code,
                Question = poll.Question,
                Type = poll.Type,
                IsOpen = poll.IsAcceptingVotes(),
                TotalVotes = totalVotes,
                Options = poll.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select(o => new PollOptionResultDTO
                    {
                        OptionId = o.Id,
                        Text = o.Text,
                        Count = o.Votes.Count,
                        Percentage = totalVotes == 0 ? 0 : Math.Round(o.Votes.Count * 100.0 / totalVotes, 1),
                    })
                    .ToList(),
                OpenTextAnswers = poll.Type == PollType.OpenText
                    ? poll.Votes.Where(v => v.TextAnswer != null).Select(v => v.TextAnswer!).ToList()
                    : new List<string>(),
            };
        }

        public async Task CacheResultsAsync(PollResultsDTO results)
        {
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            await _cache.SetStringAsync(ResultsCacheKey(results.Code), JsonSerializer.Serialize(results), options);
        }

        public async Task InvalidateResultsCacheAsync(string code)
        {
            await _cache.RemoveAsync(ResultsCacheKey(code));
        }

        public async Task<bool> CloseAsync(string code, string userId)
        {
            var poll = await _db.Polls.FirstOrDefaultAsync(p => p.Code == code);
            if (poll is null || poll.CreatedByUserId != userId) return false;

            poll.Status = PollStatus.Closed;
            await _db.SaveChangesAsync();
            await InvalidateResultsCacheAsync(code);
            return true;
        }

        public async Task<IReadOnlyList<PollSummaryDTO>> GetMineAsync(string userId)
        {
            return await _db.Polls
                .Where(p => p.CreatedByUserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PollSummaryDTO
                {
                    Code = p.Code,
                    Question = p.Question,
                    IsOpen = p.Status == PollStatus.Open && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow),
                    TotalVotes = p.Votes.Count,
                    CreatedAt = p.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task<int> CloseExpiredPollsAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _db.Polls
                .Where(p => p.Status == PollStatus.Open && p.ExpiresAt != null && p.ExpiresAt <= now)
                .ToListAsync();

            foreach (var poll in expired)
            {
                poll.Status = PollStatus.Expired;
                await InvalidateResultsCacheAsync(poll.Code);
            }

            if (expired.Count > 0)
            {
                await _db.SaveChangesAsync();
            }

            return expired.Count;
        }
    }
}
