using Microsoft.Extensions.DependencyInjection;

namespace SoitMed.Common.DomainEvents
{
    /// <summary>
    /// Dispatches domain events to registered handlers
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task DispatchAsync<T>(T domainEvent) where T : IDomainEvent
        {
            try
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(typeof(T));
                var handlers = _serviceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    try
                    {
                        var handleMethod = handlerType.GetMethod("HandleAsync");
                        if (handleMethod != null)
                        {
                            var task = (Task?)handleMethod.Invoke(handler, new object[] { domainEvent });
                            if (task != null)
                            {
                                await task;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling domain event {EventType} in handler {HandlerType}",
                            typeof(T).Name, handler.GetType().Name);
                        // Continue with other handlers even if one fails
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event {EventType}", typeof(T).Name);
                // Don't throw - event dispatching should not break the main operation
            }
        }
    }

    /// <summary>
    /// Interface for domain event handlers
    /// </summary>
    public interface IDomainEventHandler<T> where T : IDomainEvent
    {
        Task HandleAsync(T domainEvent);
    }
}

