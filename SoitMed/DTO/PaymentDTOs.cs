using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;

namespace SoitMed.DTO
{
    // Payment DTOs
    public class CreatePaymentDTO
    {
        public int? MaintenanceRequestId { get; set; }
        public int? SparePartRequestId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class PaymentResponseDTO
    {
        public int Id { get; set; }
        public int? MaintenanceRequestId { get; set; }
        public int? SparePartRequestId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? PaymentReference { get; set; }
        public string? ProcessedByAccountantId { get; set; }
        public string? ProcessedByAccountantName { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? AccountingNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    // Payment Processing DTOs
    public class ProcessPaymentDTO
    {
        [MaxLength(500)]
        public string? PaymentReference { get; set; }
    }

    public class CashPaymentDTO : ProcessPaymentDTO
    {
        [MaxLength(100)]
        public string? ReceiptNumber { get; set; }
    }

    public class BankTransferDTO : ProcessPaymentDTO
    {
        [Required]
        [MaxLength(200)]
        public string BankName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankReference { get; set; }

        [MaxLength(50)]
        public string? AccountNumber { get; set; }
    }

    public class StripePaymentDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }

    public class PayPalPaymentDTO
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }

    public class LocalGatewayPaymentDTO
    {
        [Required]
        public string GatewayName { get; set; } = string.Empty;

        [Required]
        public string PaymentToken { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }

    // Accounting DTOs
    public class ConfirmPaymentDTO
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RejectPaymentDTO
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class RefundDTO
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; } // If null, full refund

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    // Financial Reports DTOs
    public class FinancialReportDTO
    {
        public DateTime ReportDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal OutstandingPayments { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
        public Dictionary<string, int> CountByPaymentMethod { get; set; } = new();
    }

    public class PaymentMethodStatisticsDTO
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public decimal SuccessRate { get; set; }
    }

    // Payment Filters
    public class PaymentFilters
    {
        public PaymentStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? MaintenanceRequestId { get; set; }
        public int? SparePartRequestId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

