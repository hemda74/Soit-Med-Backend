using SoitMed.Models.Enums;

namespace SoitMed.Common.DomainEvents
{
    /// <summary>
    /// Domain event fired when a maintenance visit is scheduled
    /// </summary>
    public class VisitScheduledEvent : IDomainEvent
    {
        public int VisitId { get; }
        public string TicketNumber { get; }
        public string CustomerId { get; }
        public string CustomerName { get; }
        public int DeviceId { get; }
        public string DeviceName { get; }
        public DateTime ScheduledDate { get; }
        public VisitOrigin Origin { get; }
        public List<string> AssignedEngineerIds { get; }
        public DateTime OccurredAt { get; }

        public VisitScheduledEvent(
            int visitId,
            string ticketNumber,
            string customerId,
            string customerName,
            int deviceId,
            string deviceName,
            DateTime scheduledDate,
            VisitOrigin origin,
            List<string> assignedEngineerIds)
        {
            VisitId = visitId;
            TicketNumber = ticketNumber;
            CustomerId = customerId;
            CustomerName = customerName;
            DeviceId = deviceId;
            DeviceName = deviceName;
            ScheduledDate = scheduledDate;
            Origin = origin;
            AssignedEngineerIds = assignedEngineerIds ?? new List<string>();
            OccurredAt = DateTime.UtcNow;
        }
    }
}

