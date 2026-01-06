using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;
using SoitMed.Models.Enums;

namespace SoitMed.Models.Contract
{
    /// <summary>
    /// Contract entity for sales and maintenance contracts
    /// Supports lifecycle management, negotiations, and installment payments
    /// </summary>
    public class Contract
    {
        [Key]
        public long Id { get; set; }

        public long? DealId { get; set; }

        [ForeignKey("DealId")]
        public virtual SalesDeal? Deal { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string? ContractContent { get; set; }

        [MaxLength(500)]
        public string? DocumentUrl { get; set; } // Transformed media path

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public DateTime? DraftedAt { get; set; }
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        [MaxLength(2000)]
        public string? CancellationReason { get; set; }

        [Required]
        [MaxLength(450)]
        public string DraftedBy { get; set; } = string.Empty;

        [ForeignKey("DraftedBy")]
        public virtual ApplicationUser Drafter { get; set; } = null!;

        [Column(TypeName = "nvarchar(max)")]
        public string? LastReviewedBy { get; set; }

        public DateTime? LastReviewedAt { get; set; }

        [Required]
        public long ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;

        [MaxLength(450)]
        public string? CustomerSignedBy { get; set; }

        [ForeignKey("CustomerSignedBy")]
        public virtual ApplicationUser? CustomerSigner { get; set; }

        public DateTime? CustomerSignedAt { get; set; }

        // Financial configuration
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CashAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InstallmentAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? InterestRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? LatePenaltyRate { get; set; }

        public int? InstallmentDurationMonths { get; set; }

        public DateTime? FinancialConfigurationCompletedAt { get; set; }

        [MaxLength(450)]
        public string? FinancialConfiguredBy { get; set; }

        [ForeignKey("FinancialConfiguredBy")]
        public virtual ApplicationUser? FinancialConfigurator { get; set; }

        // Legacy migration support
        public int? LegacyContractId { get; set; } // Maps to TBS MNT_MaintenanceContract.ContractId

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<ContractNegotiation> Negotiations { get; set; } = new List<ContractNegotiation>();
        public virtual ICollection<InstallmentSchedule> InstallmentSchedules { get; set; } = new List<InstallmentSchedule>();
    }
}

