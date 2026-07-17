using Microsoft.AspNetCore.SignalR;

namespace PollSurveyBuilder.API.Hubs
{
    /// <summary>
    /// Clients viewing a results page join the group for that poll's code and
    /// receive a "resultsUpdated" push (see VotesController) every time someone votes -
    /// no polling required on the frontend.
    /// </summary>
    public class PollHub : Hub
    {
        public async Task JoinPoll(string pollCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(pollCode));
        }

        public async Task LeavePoll(string pollCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(pollCode));
        }

        public static string GroupName(string pollCode) => $"poll:{pollCode}";
    }
}
