using PollSurveyBuilder.Domain.Entities.Identity;

namespace PollSurveyBuilder.Application.IServices
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) CreateJwt(AppUser user);
    }
}
