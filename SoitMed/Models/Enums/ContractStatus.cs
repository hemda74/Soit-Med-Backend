namespace SoitMed.Models.Enums
{
    /// <summary>
    /// Contract lifecycle status
    /// </summary>
    public enum ContractStatus
    {
        Draft = 0,
        SentToCustomer = 1,
        UnderNegotiation = 2,
        Signed = 3,
        Cancelled = 4,
        Expired = 5
    }
}

