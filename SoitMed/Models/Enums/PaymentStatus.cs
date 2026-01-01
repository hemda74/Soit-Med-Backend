namespace SoitMed.Models.Enums
{
    public enum PaymentStatus
    {
        NotRequired = 0,
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5,
        Refunded = 6,
        Unpaid = 7,
        PendingCollection = 8,
        Collected = 9,
        PaidOnline = 10
    }
}

