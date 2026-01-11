using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    #region Customer DTOs

    public class CustomerDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // "New" or "Legacy"
    }

    public class CustomerSearchCriteria
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeLegacy { get; set; } = true;
    }

    #endregion

    #region Equipment DTOs

    public class EnhancedEquipmentDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // "New" or "Legacy"
    }

    #endregion

    #region Visit DTOs

    public class VisitDTO
    {
        public string Id { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string EngineerName { get; set; } = string.Empty;
        public string Report { get; set; } = string.Empty;
        public string ActionsTaken { get; set; } = string.Empty;
        public string PartsUsed { get; set; } = string.Empty;
        public decimal? ServiceFee { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // "New" or "Legacy"
    }

    public class CompleteVisitDTO
    {
        [Required]
        public string VisitId { get; set; } = string.Empty;

        [Required]
        public string Source { get; set; } = string.Empty; // "New" or "Legacy"

        public string Report { get; set; } = string.Empty;
        public string ActionsTaken { get; set; } = string.Empty;
        public string PartsUsed { get; set; } = string.Empty;
        public decimal? ServiceFee { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class VisitCompletionDTO
    {
        public bool Success { get; set; }
        public string VisitId { get; set; } = string.Empty;
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    #endregion

    #region Combined DTOs

    public class CustomerEquipmentVisitsDTO
    {
        public CustomerDTO? Customer { get; set; }
        public List<EnhancedEquipmentDTO> Equipment { get; set; } = new();
        public List<VisitDTO> Visits { get; set; } = new();
    }

    public class EquipmentVisitsDTO
    {
        public EnhancedEquipmentDTO? Equipment { get; set; }
        public List<VisitDTO> Visits { get; set; } = new();
    }

    public class CustomerVisitStatsDTO
    {
        public string CustomerId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalVisits { get; set; }
        public int CompletedVisits { get; set; }
        public int PendingVisits { get; set; }
        public int CancelledVisits { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CompletionRate { get; set; }
    }

    #endregion

    #region Response DTOs

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }

    #endregion
}
