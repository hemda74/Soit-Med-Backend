using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Admin controller for administrative operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Context _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUnitOfWork unitOfWork,
            Context context,
            ILogger<AdminController> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Link a Client to a User account
        /// POST /api/Admin/Client/LinkAccount
        /// </summary>
        [HttpPost("Client/LinkAccount")]
        public async Task<IActionResult> LinkClientAccount([FromBody] LinkClientAccountDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Validate Client exists
                var client = await _unitOfWork.Clients.GetByIdAsync(dto.ClientId);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {dto.ClientId} not found" });
                }

                // Validate User exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.UserId);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {dto.UserId} not found" });
                }

                // Check if client is already linked to another user
                if (client.RelatedUserId != null && client.RelatedUserId != dto.UserId)
                {
                    return BadRequest(new 
                    { 
                        message = $"Client is already linked to another user (UserId: {client.RelatedUserId})" 
                    });
                }

                // Link the client to the user
                client.RelatedUserId = dto.UserId;
                client.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client {ClientId} linked to User {UserId} by {CurrentUser}", 
                    dto.ClientId, dto.UserId, User.Identity?.Name);

                return Ok(new
                {
                    success = true,
                    message = $"Client '{client.Name}' successfully linked to user account",
                    data = new
                    {
                        clientId = client.Id,
                        clientName = client.Name,
                        userId = dto.UserId,
                        userName = user.UserName,
                        linkedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking client {ClientId} to user {UserId}", 
                    dto.ClientId, dto.UserId);
                return StatusCode(500, new { message = "An error occurred while linking the client account", error = ex.Message });
            }
        }

        /// <summary>
        /// Unlink a Client from a User account
        /// POST /api/Admin/Client/UnlinkAccount
        /// </summary>
        [HttpPost("Client/UnlinkAccount")]
        public async Task<IActionResult> UnlinkClientAccount([FromBody] long clientId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                {
                    return NotFound(new { message = $"Client with ID {clientId} not found" });
                }

                if (client.RelatedUserId == null)
                {
                    return BadRequest(new { message = "Client is not linked to any user account" });
                }

                var previousUserId = client.RelatedUserId;
                client.RelatedUserId = null;
                client.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client {ClientId} unlinked from User {UserId} by {CurrentUser}", 
                    clientId, previousUserId, User.Identity?.Name);

                return Ok(new
                {
                    success = true,
                    message = $"Client '{client.Name}' successfully unlinked from user account",
                    data = new
                    {
                        clientId = client.Id,
                        clientName = client.Name,
                        unlinkedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking client {ClientId}", clientId);
                return StatusCode(500, new { message = "An error occurred while unlinking the client account", error = ex.Message });
            }
        }
    }
}

