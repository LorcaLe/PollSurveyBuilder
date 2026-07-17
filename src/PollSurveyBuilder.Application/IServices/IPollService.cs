using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices
{
    public interface IPollService
    {
        Task<CreatePollResultDTO> CreateAsync(CreatePollDTO dto, string? userId, string baseUrl);

        Task<PollVoteViewDTO?> GetForVotingAsync(string code, string voterToken);

        /// <summary>Cached (Redis) live tally, invalidated by VoteService on every new vote.</summary>
        Task<PollResultsDTO?> GetResultsAsync(string code);

        Task<bool> CloseAsync(string code, string userId);

        Task<IReadOnlyList<PollSummaryDTO>> GetMineAsync(string userId);

        /// <summary>Used by the background job to auto-close polls whose ExpiresAt has passed.</summary>
        Task<int> CloseExpiredPollsAsync();

        /// <summary>Recomputes results straight from the database, bypassing the cache. Used by VoteService right after a write.</summary>
        Task<PollResultsDTO?> ComputeResultsAsync(string code);

        Task CacheResultsAsync(PollResultsDTO results);

        Task InvalidateResultsCacheAsync(string code);
    }
}
