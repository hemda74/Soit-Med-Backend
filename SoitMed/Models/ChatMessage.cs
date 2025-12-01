using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Chat message in a conversation (supports text and voice messages)
    /// </summary>
    public class ChatMessage
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ConversationId { get; set; }

        [Required]
        [MaxLength(450)]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string MessageType { get; set; } = "Text"; // "Text" or "Voice"

        [MaxLength(2000)]
        public string? Content { get; set; } // Text message content

        [MaxLength(500)]
        public string? VoiceFilePath { get; set; } // Path to voice file

        public int? VoiceDuration { get; set; } // Duration in seconds

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ConversationId")]
        public virtual ChatConversation? Conversation { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; }
    }
}

