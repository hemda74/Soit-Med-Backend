using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Chat conversation between a customer and support staff
    /// </summary>
    public class ChatConversation
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string CustomerId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? AdminId { get; set; } // Assigned support staff (nullable for auto-assignment)

        /// <summary>
        /// Type of chat conversation (Support, Sales, or Maintenance)
        /// Determines which role can handle the conversation
        /// </summary>
        [Required]
        public ChatType ChatType { get; set; } = ChatType.Support;

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

