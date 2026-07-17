namespace PollSurveyBuilder.Domain.Entities
{
    /// <summary>
    /// A single respondent's answer. Voting is anonymous (no login required):
    /// each browser gets a random VoterToken cookie, and the (PollId, VoterToken)
    /// pair is unique so nobody can vote twice on the same poll.
    /// </summary>
    public class Vote
    {
        public int Id { get; set; }

        public int PollId { get; set; }
        public Poll Poll { get; set; } = default!;

        /// <summary>Null when the poll type is OpenText (answer stored in TextAnswer instead).</summary>
        public int? OptionId { get; set; }
        public PollOption? Option { get; set; }

        /// <summary>Free-text answer, only populated for PollType.OpenText.</summary>
        public string? TextAnswer { get; set; }

        /// <summary>Random per-browser token (cookie), used to enforce one vote per respondent.</summary>
        public string VoterToken { get; set; } = default!;

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
}
