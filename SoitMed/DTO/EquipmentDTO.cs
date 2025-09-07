using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Equipment;

namespace SoitMed.DTO
{
    public class EquipmentDTO
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [MaxLength(100)]
        public string? QRCode { get; set; } // Optional - will be auto-generated if not provided

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }

        [Required]
        public required string HospitalId { get; set; }

        public EquipmentStatus Status { get; set; } = EquipmentStatus.Operational;
    }

    public class EquipmentResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string? QRCodeImageData { get; set; }
        public string? QRCodePdfPath { get; set; }
        public string? Description { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string HospitalId { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public int RepairVisitCount { get; set; }
        public EquipmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class RepairRequestDTO
    {
        [Required]
        public int EquipmentId { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Description { get; set; }

        [MaxLength(500)]
        public string? Symptoms { get; set; }

        public RepairPriority Priority { get; set; } = RepairPriority.Medium;

        // Either DoctorId or TechnicianId should be provided (not both)
        public int? DoctorId { get; set; }
        public int? TechnicianId { get; set; }

        public int? EstimatedHours { get; set; }
    }

    public class RepairRequestResponseDTO
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string EquipmentQRCode { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Symptoms { get; set; }
        public RepairPriority Priority { get; set; }
        public RepairStatus Status { get; set; }
        
        public string? RequestorName { get; set; }
        public string? RequestorType { get; set; } // "Doctor" or "Technician"
        
        public int? AssignedEngineerId { get; set; }
        public string? AssignedEngineerName { get; set; }
        
        public DateTime RequestedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public string? RepairNotes { get; set; }
        public string? PartsUsed { get; set; }
        public decimal? RepairCost { get; set; }
        public int? EstimatedHours { get; set; }
        public int? ActualHours { get; set; }
    }

    public class UpdateRepairRequestDTO
    {
        public RepairStatus? Status { get; set; }
        public int? AssignedEngineerId { get; set; }
        public string? RepairNotes { get; set; }
        public string? PartsUsed { get; set; }
        public decimal? RepairCost { get; set; }
        public int? ActualHours { get; set; }
    }
}
