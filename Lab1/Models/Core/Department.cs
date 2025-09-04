using System.ComponentModel.DataAnnotations;
using Lab1.Models.Identity;

namespace Lab1.Models.Core
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for users in this department
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
