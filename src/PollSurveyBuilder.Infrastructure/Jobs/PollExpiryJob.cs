using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PollSurveyBuilder.Application.IServices;

namespace PollSurveyBuilder.Infrastructure.Jobs
{
    /// <summary>
    /// Scheduled hosted service (the "background job" required by the brief): every
    /// minute, closes any poll whose ExpiresAt has passed so the results page can
    /// show a final "poll closed" banner instead of silently still accepting votes.
    /// </summary>
    public class PollExpiryJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PollExpiryJob> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        public PollExpiryJob(IServiceScopeFactory scopeFactory, ILogger<PollExpiryJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var pollService = scope.ServiceProvider.GetRequiredService<IPollService>();
                    var closedCount = await pollService.CloseExpiredPollsAsync();
                    if (closedCount > 0)
                    {
                        _logger.LogInformation("PollExpiryJob closed {Count} expired poll(s).", closedCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PollExpiryJob run failed.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}
