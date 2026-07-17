using Microsoft.AspNetCore.Identity;

namespace PollSurveyBuilder.Domain.Entities.Identity
{
    /// <summary>Poll creators / admins. Voters never need an account.</summary>
    public class AppUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
