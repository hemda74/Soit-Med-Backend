namespace SoitMed.Common.DomainEvents
{
    /// <summary>
    /// Interface for dispatching domain events
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches a domain event to all registered handlers
        /// </summary>
        Task DispatchAsync<T>(T domainEvent) where T : IDomainEvent;
    }
}

