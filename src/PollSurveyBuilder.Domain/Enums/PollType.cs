namespace PollSurveyBuilder.Domain.Enums
{
    /// <summary>
    /// The kind of question a poll asks. Determines how the frontend renders
    /// the voting UI and how VoteService validates an incoming vote.
    /// </summary>
    public enum PollType
    {
        SingleChoice = 0,   // pick exactly one of up to 6 options
        YesNo = 1,          // two fixed options: Yes / No
        Rating = 2,         // 1-5 star rating, stored as an "option" per star value
        OpenText = 3        // free-text answer, not counted in the bar chart
    }
}
