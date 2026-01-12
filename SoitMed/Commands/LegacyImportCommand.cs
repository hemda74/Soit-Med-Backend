using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace SoitMed.Commands
{
    public class LegacyImportCommand : BackgroundService
    {
        private readonly ILogger<LegacyImportCommand> _logger;

        public LegacyImportCommand(ILogger<LegacyImportCommand> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Legacy Import Command Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Legacy Import Command Service is running.");
                
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Legacy Import Command Service is stopping.");
        }
    }
}
