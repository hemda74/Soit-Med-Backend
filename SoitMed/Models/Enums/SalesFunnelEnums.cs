namespace SoitMed.Models.Enums
{
    /// <summary>
    /// Type of interaction with the client
    /// </summary>
    public enum InteractionType
    {
        Visit = 1,
        FollowUp = 2
    }

    /// <summary>
    /// Client classification based on potential value
    /// </summary>
    public enum ClientType
    {
        A = 1, // High value clients
        B = 2, // Medium value clients
        C = 3, // Low value clients
        D = 4  // Very low value clients
    }

    /// <summary>
    /// Result of the activity interaction
    /// </summary>
    public enum ActivityResult
    {
        Interested = 1,
        NotInterested = 2
    }

    /// <summary>
    /// Reason for client rejection
    /// </summary>
    public enum RejectionReason
    {
        Cash = 1,    // No budget/cash flow issues
        Price = 2,   // Price too high
        Need = 3,    // No current need
        Other = 4    // Other reasons
    }

    /// <summary>
    /// Status of a deal
    /// </summary>
    public enum DealStatus
    {
        Pending = 1,
        Won = 2,
        Lost = 3
    }

    /// <summary>
    /// Status of an offer
    /// </summary>
    public enum OfferStatus
    {
        Draft = 1,
        Sent = 2,
        Accepted = 3,
        Rejected = 4
    }
}



