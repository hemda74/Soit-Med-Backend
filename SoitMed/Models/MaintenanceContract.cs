using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;
using SoitMed.Models.Enums;
using SoitMed.Models.Equipment;

namespace SoitMed.Models
{
    public class MaintenanceContract
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(450)]
        public string ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual ApplicationUser Client { get; set; }

        [Required]
        [MaxLength(200)]
        public string ContractNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [MaxLength(20)]
        public string PaymentTerms { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<MaintenanceVisit> Visits { get; set; } = new List<MaintenanceVisit>();
    }
}
