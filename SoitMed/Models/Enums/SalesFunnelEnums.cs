namespace SoitMed.Models.Enums
{
    /// <summary>
    /// Reason for client rejection
    /// Note: Other enums (InteractionType, ClientType, ActivityResult, DealStatus, OfferStatus) 
    /// are defined in separate files to avoid conflicts.
    /// </summary>
    public enum RejectionReason
    {
        Cash = 1,    // No budget/cash flow issues
        Price = 2,   // Price too high
        Need = 3,    // No current need
        Other = 4    // Other reasons
    }
}



