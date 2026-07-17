using PollSurveyBuilder.Domain.Enums;

namespace PollSurveyBuilder.Application.DTOs
{
    /// <summary>Body of POST /api/polls.</summary>
    public class CreatePollDTO
    {
        public string Question { get; set; } = default!;
        public PollType Type { get; set; } = PollType.SingleChoice;

        /// <summary>Answer labels. Ignored for YesNo (fixed) and Rating (1-5, generated).
        /// Required (2-6 items) for SingleChoice. Ignored for OpenText.</summary>
        public List<string> Options { get; set; } = new();

        /// <summary>Minutes until the poll auto-closes. Null = never expires.</summary>
        public int? ExpiresInMinutes { get; set; }
    }

    /// <summary>Poll shape returned to the creator right after creation, includes the share link.</summary>
    public class CreatePollResultDTO
    {
        public string Code { get; set; } = default!;
        public string ShareUrl { get; set; } = default!;
        public string QrCodeUrl { get; set; } = default!;
    }

    /// <summary>Public poll shape shown on the voting page (no vote counts).</summary>
    public class PollVoteViewDTO
    {
        public string Code { get; set; } = default!;
        public string Question { get; set; } = default!;
        public PollType Type { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<PollOptionDTO> Options { get; set; } = new();
        public bool AlreadyVoted { get; set; }
    }

    public class PollOptionDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = default!;
    }

    /// <summary>Live results shape - what the SignalR hub broadcasts and GET /results returns.</summary>
    public class PollResultsDTO
    {
        public string Code { get; set; } = default!;
        public string Question { get; set; } = default!;
        public PollType Type { get; set; }
        public bool IsOpen { get; set; }
        public int TotalVotes { get; set; }
        public List<PollOptionResultDTO> Options { get; set; } = new();
        public List<string> OpenTextAnswers { get; set; } = new();
    }

    public class PollOptionResultDTO
    {
        public int OptionId { get; set; }
        public string Text { get; set; } = default!;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>One row of the creator's "My polls" dashboard.</summary>
    public class PollSummaryDTO
    {
        public string Code { get; set; } = default!;
        public string Question { get; set; } = default!;
        public bool IsOpen { get; set; }
        public int TotalVotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
