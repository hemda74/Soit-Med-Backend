using SoitMed.Services;

namespace SoitMed.Common.DomainEvents
{
    /// <summary>
    /// Handles VisitScheduledEvent by sending notifications to assigned engineers
    /// </summary>
    public class VisitScheduledEventHandler : IDomainEventHandler<VisitScheduledEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<VisitScheduledEventHandler> _logger;

        public VisitScheduledEventHandler(
            INotificationService notificationService,
            ILogger<VisitScheduledEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleAsync(VisitScheduledEvent domainEvent)
        {
            try
            {
                var title = "New Maintenance Visit Scheduled";
                var message = $"Visit {domainEvent.TicketNumber} for {domainEvent.DeviceName} scheduled on {domainEvent.ScheduledDate:yyyy-MM-dd HH:mm}";

                var metadata = new Dictionary<string, object>
                {
                    ["visitId"] = domainEvent.VisitId,
                    ["ticketNumber"] = domainEvent.TicketNumber,
                    ["deviceId"] = domainEvent.DeviceId,
                    ["scheduledDate"] = domainEvent.ScheduledDate,
                    ["origin"] = domainEvent.Origin.ToString()
                };

                // Send notification to all assigned engineers
                foreach (var engineerId in domainEvent.AssignedEngineerIds)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            engineerId,
                            title,
                            message,
                            "VisitScheduled",
                            "High",
                            null,
                            null,
                            true, // isMobilePush
                            metadata
                        );

                        _logger.LogInformation("Visit scheduled notification sent to Engineer {EngineerId} for Visit {VisitId}",
                            engineerId, domainEvent.VisitId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification to Engineer {EngineerId} for Visit {VisitId}",
                            engineerId, domainEvent.VisitId);
                    }
                }

                // Also send notification to customer
                try
                {
                    await _notificationService.CreateNotificationAsync(
                        domainEvent.CustomerId,
                        "Maintenance Visit Scheduled",
                        $"Your maintenance visit for {domainEvent.DeviceName} has been scheduled for {domainEvent.ScheduledDate:yyyy-MM-dd HH:mm}",
                        "VisitScheduled",
                        "Medium",
                        null,
                        null,
                        true, // isMobilePush
                        metadata
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification to Customer {CustomerId} for Visit {VisitId}",
                        domainEvent.CustomerId, domainEvent.VisitId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling VisitScheduledEvent for Visit {VisitId}", domainEvent.VisitId);
            }
        }
    }
}

