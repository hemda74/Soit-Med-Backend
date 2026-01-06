using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;

namespace SoitMed.Models.Contract
{
    /// <summary>
    /// Installment payment schedule for contracts
    /// </summary>
    public class InstallmentSchedule
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ContractId { get; set; }

        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;

        [Required]
        public int InstallmentNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InterestAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LatePenaltyAmount { get; set; }

        [Required]
        public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;

        public DateTime? PaidAt { get; set; }

        [MaxLength(2000)]
        public string? PaymentNotes { get; set; }

        // Notification tracking
        public bool NotificationSent7Days { get; set; } = false;
        public bool NotificationSent2Days { get; set; } = false;
        public bool NotificationSent1Day { get; set; } = false;
        public bool OverdueNotificationSent { get; set; } = false;
        public DateTime? LastReminderSentAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

