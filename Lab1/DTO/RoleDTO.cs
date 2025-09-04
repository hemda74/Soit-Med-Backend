using System.ComponentModel.DataAnnotations;

namespace Lab1.DTO
{
    public class RoleDTO
    {
        [Required]
        [MaxLength(100)]
        public required string RoleName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class RoleResponseDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
