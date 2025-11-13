namespace SoitMed.Models.Enums
{
    public enum MaintenanceRequestStatus
    {
        Pending = 1,
        Assigned = 2,
        InProgress = 3,
        NeedsSecondVisit = 4,
        NeedsSparePart = 5,
        WaitingForSparePart = 6,
        WaitingForCustomerApproval = 7,
        Completed = 8,
        Cancelled = 9,
        OnHold = 10
    }
}

