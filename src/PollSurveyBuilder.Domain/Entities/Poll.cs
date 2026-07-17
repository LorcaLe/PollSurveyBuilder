using PollSurveyBuilder.Domain.Enums;

namespace PollSurveyBuilder.Domain.Entities
{
    /// <summary>
    /// A poll created by a user. Identified publicly by the short "Code"
    /// (e.g. /poll/7fGh2) rather than the numeric Id, so links never leak
    /// row counts and can't be enumerated.
    /// </summary>
    public class Poll
    {
        public int Id { get; set; }

        /// <summary>Short, URL-safe public identifier, e.g. "7fGh2".</summary>
        public string Code { get; set; } = default!;

        public string Question { get; set; } = default!;

        public PollType Type { get; set; } = PollType.SingleChoice;

        public PollStatus Status { get; set; } = PollStatus.Open;

        /// <summary>FK to AspNetUsers. Null means an anonymous creator.</summary>
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Null = never expires unless closed manually.</summary>
        public DateTime? ExpiresAt { get; set; }

        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();

        public ICollection<Vote> Votes { get; set; } = new List<Vote>();

        public bool IsAcceptingVotes()
        {
            if (Status != PollStatus.Open) return false;
            if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow) return false;
            return true;
        }
    }
}
