using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Chat conversation between a customer and admin
    /// </summary>
    public class ChatConversation
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string CustomerId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? AdminId { get; set; } // Assigned admin (nullable for auto-assignment)

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? LastMessagePreview { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser? Customer { get; set; }

        [ForeignKey("AdminId")]
        public virtual ApplicationUser? Admin { get; set; }

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}

