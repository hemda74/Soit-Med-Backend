namespace SoitMed.Models.Enums
{
    /// <summary>
    /// Status of a maintenance visit
    /// </summary>
    public enum VisitStatus
    {
        PendingApproval = 1,
        Scheduled = 2,
        InProgress = 3,
        NeedsSpareParts = 4,
        Completed = 5,
        Rescheduled = 6,
        Cancelled = 7
    }
}

