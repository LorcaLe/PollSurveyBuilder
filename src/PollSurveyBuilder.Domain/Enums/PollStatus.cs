namespace PollSurveyBuilder.Domain.Enums
{
    public enum PollStatus
    {
        Open = 0,
        Closed = 1,      // manually closed by the creator
        Expired = 2      // auto-closed by PollExpiryJob after ExpiresAt
    }
}
