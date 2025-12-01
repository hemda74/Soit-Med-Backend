using Microsoft.Extensions.Hosting;

namespace SoitMed.Services
{
    /// <summary>
    /// Background service to delete chat messages older than 30 days
    /// Runs daily at 2 AM
    /// </summary>
    public class ChatCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChatCleanupService> _logger;

        public ChatCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ChatCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Chat Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calculate next run time (2 AM)
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddDays(1).AddHours(2); // Tomorrow at 2 AM
                    var delay = nextRun - now;

                    _logger.LogInformation("Chat Cleanup Service will run next at {NextRun}", nextRun);

                    // Wait until next run time
                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    // Perform cleanup
                    await PerformCleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Chat Cleanup Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Chat Cleanup Service");
                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Chat Cleanup Service stopped");
        }

        private async Task PerformCleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting chat cleanup - deleting messages older than 30 days");

                using var scope = _serviceProvider.CreateScope();
                var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

                await chatService.DeleteOldMessagesAsync(cancellationToken);

                _logger.LogInformation("Chat cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during chat cleanup");
                throw;
            }
        }
    }
}

