namespace SoitMed.Common.DomainEvents
{
    /// <summary>
    /// Marker interface for all domain events
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}

