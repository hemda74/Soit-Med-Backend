using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;
using System.Security.Claims;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ChatController(
            IChatService chatService,
            UserManager<ApplicationUser> userManager,
            ILogger<ChatController> logger,
            IWebHostEnvironment environment)
            : base(userManager)
        {
            _chatService = chatService;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Get or create conversation for current user (customer) or get all conversations (admin)
        /// </summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized();
                }

                var isAdmin = await UserManager.IsInRoleAsync(user, "SuperAdmin") || 
                             await UserManager.IsInRoleAsync(user, "SalesManager") ||
                             await UserManager.IsInRoleAsync(user, "SalesSupport");

                var conversations = await _chatService.GetConversationsAsync(userId, isAdmin, cancellationToken);
                return SuccessResponse(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations");
                return ErrorResponse("An error occurred while retrieving conversations", 500);
            }
        }

        /// <summary>
        /// Get or create conversation for customer
        /// </summary>
        [HttpPost("conversations")]
        public async Task<IActionResult> GetOrCreateConversation([FromBody] CreateConversationDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Only customers can create conversations for themselves
                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized();
                }

                var isAdmin = await UserManager.IsInRoleAsync(user, "SuperAdmin") || 
                             await UserManager.IsInRoleAsync(user, "SalesManager") ||
                             await UserManager.IsInRoleAsync(user, "SalesSupport");

                // If admin is creating, they can specify customerId, otherwise use current user
                var customerId = isAdmin ? dto.CustomerId : userId;

                var conversation = await _chatService.GetOrCreateConversationAsync(customerId, dto.AdminId, cancellationToken);
                return SuccessResponse(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return ErrorResponse("An error occurred while creating conversation", 500);
            }
        }

        /// <summary>
        /// Get conversation by ID
        /// </summary>
        [HttpGet("conversations/{id}")]
        public async Task<IActionResult> GetConversation(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized();
                }

                var isAdmin = await UserManager.IsInRoleAsync(user, "SuperAdmin") || 
                             await UserManager.IsInRoleAsync(user, "SalesManager") ||
                             await UserManager.IsInRoleAsync(user, "SalesSupport");

                var conversation = await _chatService.GetConversationByIdAsync(id, userId, isAdmin, cancellationToken);
                if (conversation == null)
                {
                    return NotFound();
                }

                return SuccessResponse(conversation);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to conversation {ConversationId}", id);
                return StatusCode(403, ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation {ConversationId}", id);
                return ErrorResponse("An error occurred while retrieving conversation", 500);
            }
        }

        /// <summary>
        /// Assign admin to conversation
        /// </summary>
        [HttpPut("conversations/{id}/assign")]
        [Authorize(Roles = "SuperAdmin,SalesManager")]
        public async Task<IActionResult> AssignAdmin(long id, [FromBody] AssignConversationDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var conversation = await _chatService.AssignAdminAsync(id, dto.AdminId, cancellationToken);
                return SuccessResponse(conversation);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for assigning admin to conversation {ConversationId}", id);
                return BadRequest(ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning admin to conversation {ConversationId}", id);
                return ErrorResponse("An error occurred while assigning admin", 500);
            }
        }

        /// <summary>
        /// Get messages for a conversation
        /// </summary>
        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized();
                }

                var isAdmin = await UserManager.IsInRoleAsync(user, "SuperAdmin") || 
                             await UserManager.IsInRoleAsync(user, "SalesManager") ||
                             await UserManager.IsInRoleAsync(user, "SalesSupport");

                var messages = await _chatService.GetMessagesAsync(id, userId, isAdmin, page, pageSize, cancellationToken);
                return SuccessResponse(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to messages in conversation {ConversationId}", id);
                return StatusCode(403, ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for conversation {ConversationId}", id);
                return ErrorResponse("An error occurred while retrieving messages", 500);
            }
        }

        /// <summary>
        /// Send text message
        /// </summary>
        [HttpPost("messages")]
        public async Task<IActionResult> SendTextMessage([FromBody] SendTextMessageDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var message = await _chatService.SendTextMessageAsync(dto.ConversationId, userId, dto.Content, cancellationToken);
                return SuccessResponse(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to send message");
                return StatusCode(403, ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for sending message");
                return BadRequest(ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return ErrorResponse("An error occurred while sending message", 500);
            }
        }

        /// <summary>
        /// Upload and send voice message
        /// </summary>
        [HttpPost("messages/voice")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<IActionResult> SendVoiceMessage(
            [FromForm] IFormFile file,
            [FromForm] long conversationId,
            [FromForm] int voiceDuration,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ErrorResponse("Voice file is required"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".ogg" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(ErrorResponse("Invalid file type. Allowed types: mp3, wav, m4a, aac, ogg"));
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(ErrorResponse("File size exceeds 10MB limit"));
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Save voice file
                var conversationFolder = Path.Combine(_environment.WebRootPath, "chat", "voice", conversationId.ToString());
                Directory.CreateDirectory(conversationFolder);

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(conversationFolder, fileName);
                var relativePath = Path.Combine("chat", "voice", conversationId.ToString(), fileName).Replace('\\', '/');

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                // Create message
                var message = await _chatService.SendVoiceMessageAsync(conversationId, userId, relativePath, voiceDuration, cancellationToken);
                
                return SuccessResponse(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to send voice message");
                return StatusCode(403, ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for sending voice message");
                return BadRequest(ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending voice message");
                return ErrorResponse("An error occurred while sending voice message", 500);
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        [HttpPut("conversations/{id}/read")]
        public async Task<IActionResult> MarkMessagesAsRead(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _chatService.MarkMessagesAsReadAsync(id, userId, cancellationToken);
                return SuccessResponse("Messages marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return ErrorResponse("An error occurred while marking messages as read", 500);
            }
        }

        /// <summary>
        /// Get unread message count for a conversation
        /// </summary>
        [HttpGet("conversations/{id}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var count = await _chatService.GetUnreadCountAsync(id, userId, cancellationToken);
                return SuccessResponse(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread count");
                return ErrorResponse("An error occurred while retrieving unread count", 500);
            }
        }
    }
}

