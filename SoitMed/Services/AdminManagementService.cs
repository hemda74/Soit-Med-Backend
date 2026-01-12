using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public interface IAdminManagementService
    {
        Task<PagedResult<ClientWithEmailStatusDTO>> GetClientsWithEmailStatusAsync(
            int page, int pageSize, string? searchTerm, string? emailStatus);
        Task<ServiceResult<ClientWithEmailStatusDTO>> SetClientEmailAsync(
            long clientId, SetClientEmailDTO dto, string? adminUserId);
        Task<ServiceResult<ClientWithEmailStatusDTO>> UpdateContactPersonAsync(
            long clientId, UpdateContactPersonDTO dto, string? adminUserId);
        Task<List<ClientEmailHistoryDTO>> GetClientEmailHistoryAsync(long clientId);
        Task<AdminDashboardDTO> GetAdminDashboardAsync();
        Task<ServiceResult<UserStatusToggleDTO>> ToggleUserStatusAsync(
            string userId, string? adminUserId);
    }

    public class AdminManagementService : IAdminManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Context _context;
        private readonly ILogger<AdminManagementService> _logger;

        public AdminManagementService(
            IUnitOfWork unitOfWork,
            Context context,
            ILogger<AdminManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ClientWithEmailStatusDTO>> GetClientsWithEmailStatusAsync(
            int page, int pageSize, string? searchTerm, string? emailStatus)
        {
            try
            {
                var query = _context.Clients.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(c => 
                        c.Name.Contains(searchTerm) ||
                        (c.Email != null && c.Email.Contains(searchTerm)) ||
                        (c.Phone != null && c.Phone.Contains(searchTerm)));
                }

                // Apply email status filter
                if (!string.IsNullOrWhiteSpace(emailStatus))
                {
                    query = query.Where(c => c.EmailStatus == emailStatus);
                }

                var totalCount = await query.CountAsync();

                var clients = await query
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ClientWithEmailStatusDTO
                    {
                        Id = long.Parse(c.Id),
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        HasEmail = c.HasEmail,
                        EmailStatus = c.EmailStatus,
                        EmailCreatedBy = c.EmailCreatedBy,
                        EmailCreatedAt = c.EmailCreatedAt,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        
                        // Contact Person Information
                        ContactPerson = c.ContactPerson,
                        ContactPersonEmail = c.ContactPersonEmail,
                        ContactPersonPhone = c.ContactPersonPhone,
                        ContactPersonPosition = c.ContactPersonPosition,
                        LastContactDate = c.LastContactDate,
                        NextContactDate = c.NextContactDate
                    })
                    .ToListAsync();

                return new PagedResult<ClientWithEmailStatusDTO>
                {
                    Items = clients,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients with email status");
                throw;
            }
        }

        public async Task<ServiceResult<ClientWithEmailStatusDTO>> SetClientEmailAsync(
            long clientId, SetClientEmailDTO dto, string? adminUserId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                {
                    return new ServiceResult<ClientWithEmailStatusDTO>
                    {
                        Success = false,
                        Message = $"Client with ID {clientId} not found"
                    };
                }

                // Validate email format
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return new ServiceResult<ClientWithEmailStatusDTO>
                    {
                        Success = false,
                        Message = "Email address is required"
                    };
                }

                if (!IsValidEmail(dto.Email))
                {
                    return new ServiceResult<ClientWithEmailStatusDTO>
                    {
                        Success = false,
                        Message = "Invalid email format"
                    };
                }

                // Check if email is already used by another client
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == dto.Email && c.Id != clientId.ToString());
                
                if (existingClient != null)
                {
                    return new ServiceResult<ClientWithEmailStatusDTO>
                    {
                        Success = false,
                        Message = "Email address is already used by another client"
                    };
                }

                // Store old email for history
                var oldEmail = client.Email;
                var oldStatus = client.EmailStatus;

                // Update client email
                client.Email = dto.Email;
                client.HasEmail = true;
                client.EmailStatus = "Valid";
                client.EmailCreatedBy = adminUserId;
                client.EmailCreatedAt = DateTime.UtcNow;
                client.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                // Create email history record
                await CreateEmailHistoryAsync(clientId, oldEmail, dto.Email, adminUserId);

                var result = new ClientWithEmailStatusDTO
                {
                    Id = long.Parse(client.Id),
                    Name = client.Name,
                    Email = client.Email,
                    Phone = client.Phone,
                    HasEmail = client.HasEmail,
                    EmailStatus = client.EmailStatus,
                    EmailCreatedBy = client.EmailCreatedBy,
                    EmailCreatedAt = client.EmailCreatedAt,
                    CreatedAt = client.CreatedAt,
                    UpdatedAt = client.UpdatedAt,
                    
                    // Contact Person Information
                    ContactPerson = client.ContactPerson,
                    ContactPersonEmail = client.ContactPersonEmail,
                    ContactPersonPhone = client.ContactPersonPhone,
                    ContactPersonPosition = client.ContactPersonPosition,
                    LastContactDate = client.LastContactDate,
                    NextContactDate = client.NextContactDate
                };

                return new ServiceResult<ClientWithEmailStatusDTO>
                {
                    Success = true,
                    Message = "Client email updated successfully",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting email for client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<ServiceResult<ClientWithEmailStatusDTO>> UpdateContactPersonAsync(
            long clientId, UpdateContactPersonDTO dto, string? adminUserId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                {
                    return new ServiceResult<ClientWithEmailStatusDTO>
                    {
                        Success = false,
                        Message = $"Client with ID {clientId} not found"
                    };
                }

                // Validate contact person name
                if (string.IsNullOrWhiteSpace(dto.ContactPerson))
                {
                    return new ServiceResult<ClientWithEmailStatusDTO>
                    {
                        Success = false,
                        Message = "Contact person name is required"
                    };
                }

                // Validate email if provided
                if (!string.IsNullOrWhiteSpace(dto.ContactPersonEmail) && !IsValidEmail(dto.ContactPersonEmail))
                {
                    return new ServiceResult<ClientWithEmailStatusDTO>
                    {
                        Success = false,
                        Message = "Invalid contact person email format"
                    };
                }

                // Update client contact person information
                client.ContactPerson = dto.ContactPerson.Trim();
                client.ContactPersonEmail = !string.IsNullOrWhiteSpace(dto.ContactPersonEmail) 
                    ? dto.ContactPersonEmail.Trim() 
                    : null;
                client.ContactPersonPhone = !string.IsNullOrWhiteSpace(dto.ContactPersonPhone) 
                    ? dto.ContactPersonPhone.Trim() 
                    : null;
                client.ContactPersonPosition = !string.IsNullOrWhiteSpace(dto.ContactPersonPosition) 
                    ? dto.ContactPersonPosition.Trim() 
                    : null;
                client.NextContactDate = dto.NextContactDate;
                client.LastContactDate = DateTime.UtcNow; // Update last contact date when admin updates
                client.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                var result = new ClientWithEmailStatusDTO
                {
                    Id = long.Parse(client.Id),
                    Name = client.Name,
                    Email = client.Email,
                    Phone = client.Phone,
                    HasEmail = client.HasEmail,
                    EmailStatus = client.EmailStatus,
                    EmailCreatedBy = client.EmailCreatedBy,
                    EmailCreatedAt = client.EmailCreatedAt,
                    CreatedAt = client.CreatedAt,
                    UpdatedAt = client.UpdatedAt,
                    
                    // Contact Person Information
                    ContactPerson = client.ContactPerson,
                    ContactPersonEmail = client.ContactPersonEmail,
                    ContactPersonPhone = client.ContactPersonPhone,
                    ContactPersonPosition = client.ContactPersonPosition,
                    LastContactDate = client.LastContactDate,
                    NextContactDate = client.NextContactDate
                };

                _logger.LogInformation("Client {ClientId} contact person updated by {AdminUserId}", 
                    clientId, adminUserId);

                return new ServiceResult<ClientWithEmailStatusDTO>
                {
                    Success = true,
                    Message = "Contact person information updated successfully",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact person for client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<ClientEmailHistoryDTO>> GetClientEmailHistoryAsync(long clientId)
        {
            try
            {
                // For now, return a simple history. In a real implementation, 
                // you'd have a separate ClientEmailHistory table
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                {
                    return new List<ClientEmailHistoryDTO>();
                }

                var history = new List<ClientEmailHistoryDTO>
                {
                    new ClientEmailHistoryDTO
                    {
                        ClientId = clientId.ToString(),
                        OldEmail = client.Email,
                        NewEmail = client.Email,
                        Action = "Current Email",
                        CreatedBy = client.EmailCreatedBy,
                        CreatedAt = client.EmailCreatedAt ?? DateTime.UtcNow
                    }
                };

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email history for client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<AdminDashboardDTO> GetAdminDashboardAsync()
        {
            try
            {
                var totalClients = await _context.Clients.CountAsync();
                var clientsWithEmail = await _context.Clients.CountAsync(c => c.HasEmail);
                var clientsWithoutEmail = totalClients - clientsWithEmail;
                var legacyEmailClients = await _context.Clients.CountAsync(c => c.EmailStatus == "Legacy");

                return new AdminDashboardDTO
                {
                    TotalClients = totalClients,
                    ClientsWithEmail = clientsWithEmail,
                    ClientsWithoutEmail = clientsWithoutEmail,
                    LegacyEmailClients = legacyEmailClients,
                    EmailCoveragePercentage = totalClients > 0 ? 
                        Math.Round((double)clientsWithEmail / totalClients * 100, 2) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard");
                throw;
            }
        }

        public async Task<ServiceResult<UserStatusToggleDTO>> ToggleUserStatusAsync(
            string userId, string? adminUserId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user == null)
                {
                    return new ServiceResult<UserStatusToggleDTO>
                    {
                        Success = false,
                        Message = $"User with ID {userId} not found"
                    };
                }

                var oldStatus = user.IsActive;
                user.IsActive = !user.IsActive;

                await _context.SaveChangesAsync();

                var result = new UserStatusToggleDTO
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsActive = user.IsActive,
                    PreviousStatus = oldStatus,
                    UpdatedBy = adminUserId,
                    UpdatedAt = DateTime.UtcNow
                };

                return new ServiceResult<UserStatusToggleDTO>
                {
                    Success = true,
                    Message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {UserId}", userId);
                throw;
            }
        }

        private async Task CreateEmailHistoryAsync(long clientId, string? oldEmail, string newEmail, string? adminUserId)
        {
            // In a real implementation, you'd save this to a ClientEmailHistory table
            // For now, we'll just log it
            _logger.LogInformation("Client {ClientId} email changed from {OldEmail} to {NewEmail} by {AdminUserId}", 
                clientId, oldEmail, newEmail, adminUserId);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && !email.Contains("@legacy.local");
            }
            catch
            {
                return false;
            }
        }
    }
}
