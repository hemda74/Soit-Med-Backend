using System.ComponentModel.DataAnnotations;

namespace Lab1.Models.Core
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Optional: Link to users who have this role (if needed for custom role management)
        // This is separate from ASP.NET Identity roles for business-specific role management
    }
}
