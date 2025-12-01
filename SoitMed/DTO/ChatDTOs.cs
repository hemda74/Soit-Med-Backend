using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    // ==================== Chat Conversation DTOs ====================
    
    public class ChatConversationResponseDTO
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? AdminId { get; set; }
        public string? AdminName { get; set; }
        public DateTime LastMessageAt { get; set; }
        public string? LastMessagePreview { get; set; }
        public bool IsActive { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateConversationDTO
    {
        [Required]
        [MaxLength(450)]
        public string CustomerId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? AdminId { get; set; } // Optional - can be auto-assigned
    }

    public class AssignConversationDTO
    {
        [Required]
        [MaxLength(450)]
        public string AdminId { get; set; } = string.Empty;
    }

    // ==================== Chat Message DTOs ====================

    public class ChatMessageResponseDTO
    {
        public long Id { get; set; }
        public long ConversationId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string MessageType { get; set; } = "Text"; // "Text" or "Voice"
        public string? Content { get; set; }
        public string? VoiceFilePath { get; set; }
        public string? VoiceFileUrl { get; set; } // Full URL for accessing the file
        public int? VoiceDuration { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendTextMessageDTO
    {
        [Required]
        public long ConversationId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
    }

    public class SendVoiceMessageDTO
    {
        [Required]
        public long ConversationId { get; set; }

        [Required]
        public int VoiceDuration { get; set; } // Duration in seconds
    }

    public class MarkMessagesReadDTO
    {
        [Required]
        public long ConversationId { get; set; }
    }

    public class TypingIndicatorDTO
    {
        [Required]
        public long ConversationId { get; set; }

        [Required]
        public bool IsTyping { get; set; }
    }
}

