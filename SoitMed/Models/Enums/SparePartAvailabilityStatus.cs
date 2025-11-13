namespace SoitMed.Models.Enums
{
    public enum SparePartAvailabilityStatus
    {
        Checking = 1,
        LocalAvailable = 2,
        GlobalRequired = 3,
        ReadyForEngineer = 4,
        DeliveredToEngineer = 5,
        WaitingForCustomerApproval = 6,
        CustomerApproved = 7,
        CustomerRejected = 8,
        Cancelled = 9
    }
}

