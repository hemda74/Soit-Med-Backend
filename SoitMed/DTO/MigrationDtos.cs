namespace SoitMed.DTO
{
    /// <summary>
    /// Result of equipment linking operation
    /// </summary>
    public class EquipmentLinkingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        
        public LinkingMethodResultDto? ViaVisits { get; set; }
        public LinkingMethodResultDto? ViaMaintenanceContracts { get; set; }
        public LinkingMethodResultDto? ViaSalesInvoices { get; set; }
        public LinkingMethodResultDto? ViaOrderOut { get; set; }
        
        public int TotalLinked { get; set; }
        public int TotalSkipped { get; set; }
        public int TotalErrors { get; set; }
        
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Result of a specific linking method
    /// </summary>
    public class LinkingMethodResultDto
    {
        public string MethodName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int LinkedCount { get; set; }
        public int SkippedCount { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Diagnostic statistics about equipment linking
    /// </summary>
    public class EquipmentLinkingDiagnosticsDto
    {
        public int TotalEquipment { get; set; }
        public int EquipmentWithLegacySourceId { get; set; }
        public int EquipmentLinkedToAdmin { get; set; }
        public int EquipmentLinkedToClients { get; set; }
        public int EquipmentUnlinked { get; set; }
        
        public int TotalVisitingReportsWithOoiId { get; set; }
        public int EquipmentWithMatchingVisits { get; set; }
        public int EquipmentWithMatchingClients { get; set; }
        
        public int TotalClients { get; set; }
        public int ClientsWithRelatedUserId { get; set; }
        public int ClientsWithLegacyCustomerId { get; set; }
        
        public Dictionary<string, int> LinkingMethodStats { get; set; } = new();
    }

    /// <summary>
    /// Report of unlinked equipment
    /// </summary>
    public class UnlinkedEquipmentReportDto
    {
        public int TotalUnlinked { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalUnlinked / PageSize);
        
        public List<UnlinkedEquipmentItemDto> Equipment { get; set; } = new();
    }

    /// <summary>
    /// Single unlinked equipment item
    /// </summary>
    public class UnlinkedEquipmentItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LegacySourceId { get; set; }
        public string? QRCode { get; set; }
        public string? CustomerId { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Verification result for a specific client's equipment
    /// </summary>
    public class ClientEquipmentVerificationDto
    {
        public long ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int? LegacyCustomerId { get; set; }
        public string? RelatedUserId { get; set; }
        
        public int TotalEquipment { get; set; }
        public int EquipmentFromVisits { get; set; }
        public int EquipmentFromContracts { get; set; }
        public int EquipmentFromSalesInvoices { get; set; }
        public int EquipmentFromOrderOut { get; set; }
        
        public List<EquipmentVerificationItemDto> EquipmentDetails { get; set; } = new();
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Equipment verification item
    /// </summary>
    public class EquipmentVerificationItemDto
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string? LegacySourceId { get; set; }
        public List<string> LinkingMethods { get; set; } = new();
        public bool IsLinked { get; set; }
    }
}

