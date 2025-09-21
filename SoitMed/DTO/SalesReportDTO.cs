using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class CreateSalesReportDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required.")]
        [MaxLength(2000, ErrorMessage = "Body cannot exceed 2000 characters.")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Report date is required.")]
        public DateOnly ReportDate { get; set; }
    }

    public class UpdateSalesReportDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required.")]
        [MaxLength(2000, ErrorMessage = "Body cannot exceed 2000 characters.")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Report date is required.")]
        public DateOnly ReportDate { get; set; }
    }

    public class SalesReportResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateOnly ReportDate { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class FilterSalesReportsDto
    {
        public string? EmployeeId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class RateSalesReportDto
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int? Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; }
    }

    public class PaginatedSalesReportsResponseDto
    {
        public List<SalesReportResponseDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}

