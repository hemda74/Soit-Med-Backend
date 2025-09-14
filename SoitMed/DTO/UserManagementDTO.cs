using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    // DTO for user activation/deactivation
    public class UserActivationDTO
    {
        [Required(ErrorMessage = "User ID is required")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "Action is required")]
        public required string Action { get; set; } // "activate" or "deactivate"

        public string? Reason { get; set; } // Optional reason for the action
    }

    // DTO for user statistics response
    public class UserStatisticsDTO
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int UsersByRole { get; set; }
        public DateTime GeneratedAt { get; set; }
        public Dictionary<string, int> UsersByRoleBreakdown { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UsersByDepartment { get; set; } = new Dictionary<string, int>();
    }

    // DTO for user filtering and sorting
    public class UserFilterDTO
    {
        public string? SearchTerm { get; set; } // Search in name, email, username
        public string? Role { get; set; } // Filter by specific role
        public int? DepartmentId { get; set; } // Filter by department
        public bool? IsActive { get; set; } // Filter by active status
        public DateTime? CreatedFrom { get; set; } // Filter by creation date range
        public DateTime? CreatedTo { get; set; } // Filter by creation date range
        public string? SortBy { get; set; } = "CreatedAt"; // Sort field: CreatedAt, FirstName, LastName, Email, IsActive
        public string? SortOrder { get; set; } = "desc"; // Sort order: asc, desc
        public int PageNumber { get; set; } = 1; // Pagination
        public int PageSize { get; set; } = 50; // Pagination
    }

    // DTO for paginated user response
    public class PaginatedUserResponseDTO
    {
        public List<UserDataDTO> Users { get; set; } = new List<UserDataDTO>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public UserFilterDTO AppliedFilters { get; set; } = new UserFilterDTO();
    }

    // DTO for user activation response
    public class UserActivationResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime ActionDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
