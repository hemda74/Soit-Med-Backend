using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;

namespace SoitMed.Models.Equipment
{
    public class MaintenanceRequestAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaintenanceRequestId { get; set; }

        [ForeignKey("MaintenanceRequestId")]
        public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? FileType { get; set; } // MIME type

        public long? FileSize { get; set; } // in bytes

        [Required]
        public AttachmentType AttachmentType { get; set; } // Image, Video, Audio, Document

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? UploadedById { get; set; } // ApplicationUser who uploaded

        [ForeignKey("UploadedById")]
        public virtual Identity.ApplicationUser? UploadedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

