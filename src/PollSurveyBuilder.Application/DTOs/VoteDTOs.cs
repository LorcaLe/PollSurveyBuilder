namespace PollSurveyBuilder.Application.DTOs
{
    /// <summary>Body of POST /api/polls/{code}/vote.</summary>
    public class CastVoteDTO
    {
        /// <summary>Required for SingleChoice / YesNo / Rating. Null for OpenText.</summary>
        public int? OptionId { get; set; }

        /// <summary>Required for OpenText only.</summary>
        public string? TextAnswer { get; set; }
    }

    public class AuthResultDTO
    {
        public string Token { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
    }

    public class RegisterDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
    }

    public class LoginDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
