using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.IServices
{
    public enum CastVoteOutcome
    {
        Success,
        PollNotFound,
        PollClosed,
        AlreadyVoted,
        InvalidOption
    }

    public record CastVoteResult(CastVoteOutcome Outcome, PollResultsDTO? Results);

    public interface IVoteService
    {
        /// <summary>
        /// Casts one vote and returns the freshly recomputed live results so the
        /// caller (VotesController) can broadcast them over SignalR in the same request.
        /// </summary>
        Task<CastVoteResult> CastAsync(string code, CastVoteDTO dto, string voterToken);
    }
}
