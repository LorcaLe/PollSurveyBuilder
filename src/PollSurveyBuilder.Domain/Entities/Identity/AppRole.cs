using Microsoft.AspNetCore.Identity;

namespace PollSurveyBuilder.Domain.Entities.Identity
{
    public class AppRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
