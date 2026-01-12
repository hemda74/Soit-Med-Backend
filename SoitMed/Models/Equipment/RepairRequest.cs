using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;

namespace SoitMed.Models.Equipment
{
    public class RepairRequest
    {
        [Key]
        public string Id { get; set; }

        // Foreign Key to Equipment
        [Required]
        public string EquipmentId { get; set; }

        [ForeignKey("EquipmentId")]
        public virtual Equipment Equipment { get; set; } = null!;

        // Request details
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Symptoms { get; set; }

        public RepairPriority Priority { get; set; } = RepairPriority.Medium;
        public RepairStatus Status { get; set; } = RepairStatus.Pending;

        // Requestor information (Doctor or Technician)
        public int? DoctorId { get; set; }
        public int? TechnicianId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? RequestingDoctor { get; set; }

        [ForeignKey("TechnicianId")]
        public virtual Technician? RequestingTechnician { get; set; }

        // Assigned Engineer (from the hospital's governorate)
        public string? AssignedEngineerId { get; set; }

        [ForeignKey("AssignedEngineerId")]
        public virtual Engineer? AssignedEngineer { get; set; }

        // Timestamps
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Repair details
        [MaxLength(1000)]
        public string? RepairNotes { get; set; }

        [MaxLength(500)]
        public string? PartsUsed { get; set; }

        public decimal? RepairCost { get; set; }

        // Estimated and actual duration
        public int? EstimatedHours { get; set; }
        public int? ActualHours { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public enum RepairPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4,
        Emergency = 5
    }

    public enum RepairStatus
    {
        Pending = 1,
        Assigned = 2,
        InProgress = 3,
        WaitingForParts = 4,
        Completed = 5,
        Cancelled = 6,
        OnHold = 7
    }
}
