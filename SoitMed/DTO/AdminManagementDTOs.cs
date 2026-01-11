namespace SoitMed.DTO
{
    // Client Email Management DTOs
    public class ClientWithEmailStatusDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool HasEmail { get; set; }
        public string EmailStatus { get; set; } = "Missing"; // Missing, Legacy, Valid
        public string? EmailCreatedBy { get; set; }
        public DateTime? EmailCreatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Contact Person Information
        public string? ContactPerson { get; set; }
        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? ContactPersonPosition { get; set; }
        public DateTime? LastContactDate { get; set; }
        public DateTime? NextContactDate { get; set; }
    }

    public class SetClientEmailDTO
    {
        public string Email { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class UpdateContactPersonDTO
    {
        public string ContactPerson { get; set; } = string.Empty;
        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? ContactPersonPosition { get; set; }
        public DateTime? NextContactDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ClientEmailHistoryDTO
    {
        public long ClientId { get; set; }
        public string? OldEmail { get; set; }
        public string NewEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // User Management DTOs
    public class UserStatusToggleDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool PreviousStatus { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Admin Dashboard DTOs
    public class AdminDashboardDTO
    {
        public int TotalClients { get; set; }
        public int ClientsWithEmail { get; set; }
        public int ClientsWithoutEmail { get; set; }
        public int LegacyEmailClients { get; set; }
        public double EmailCoveragePercentage { get; set; }
    }

    // Generic Service Result
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    // Generic Paged Result
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
