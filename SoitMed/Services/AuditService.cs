using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Core;
using SoitMed.Models.Enums;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for logging entity changes to audit log
    /// Tracks all critical changes with old and new values as JSON
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuditService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuditService(IUnitOfWork unitOfWork, ILogger<AuditService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
        }

        /// <summary>
        /// Logs entity changes (old and new values)
        /// </summary>
        public async Task LogEntityChangeAsync<T>(T? oldEntity, T? newEntity, string userId, string changeType, string? description = null) where T : class
        {
            try
            {
                var entityName = typeof(T).Name;
                int entityId = 0;

                // Try to get entity ID using reflection
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null)
                {
                    if (newEntity != null)
                    {
                        var idValue = idProperty.GetValue(newEntity);
                        if (idValue != null && int.TryParse(idValue.ToString(), out int parsedId))
                            entityId = parsedId;
                    }
                    else if (oldEntity != null)
                    {
                        var idValue = idProperty.GetValue(oldEntity);
                        if (idValue != null && int.TryParse(idValue.ToString(), out int parsedId))
                            entityId = parsedId;
                    }
                }

                var oldValueJson = oldEntity != null ? JsonSerializer.Serialize(oldEntity, _jsonOptions) : null;
                var newValueJson = newEntity != null ? JsonSerializer.Serialize(newEntity, _jsonOptions) : null;

                var changeLog = new EntityChangeLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    UserId = userId,
                    OldValue = oldValueJson,
                    NewValue = newValueJson,
                    ChangeType = changeType,
                    ChangeDescription = description,
                    ChangeDate = DateTime.UtcNow
                };

                var context = _unitOfWork.GetContext();
                await context.EntityChangeLogs.AddAsync(changeLog);
                await context.SaveChangesAsync();

                _logger.LogInformation("Audit log created: {EntityName} #{EntityId}, ChangeType: {ChangeType}, User: {UserId}",
                    entityName, entityId, changeType, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log entity change for {EntityType}", typeof(T).Name);
                // Don't throw - audit logging should not break the main operation
            }
        }

        /// <summary>
        /// Logs a status change for a visit
        /// </summary>
        public async Task LogStatusChangeAsync(int visitId, VisitStatus oldStatus, VisitStatus newStatus, string userId, string? description = null)
        {
            try
            {
                var changeLog = new EntityChangeLog
                {
                    EntityName = "MaintenanceVisit",
                    EntityId = visitId,
                    UserId = userId,
                    OldValue = JsonSerializer.Serialize(new { Status = oldStatus.ToString() }, _jsonOptions),
                    NewValue = JsonSerializer.Serialize(new { Status = newStatus.ToString() }, _jsonOptions),
                    ChangeType = "Updated",
                    ChangeDescription = description ?? $"Status changed from {oldStatus} to {newStatus}",
                    ChangeDate = DateTime.UtcNow
                };

                var context = _unitOfWork.GetContext();
                await context.EntityChangeLogs.AddAsync(changeLog);
                await context.SaveChangesAsync();

                _logger.LogInformation("Status change logged: Visit #{VisitId}, {OldStatus} -> {NewStatus}, User: {UserId}",
                    visitId, oldStatus, newStatus, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log status change for Visit #{VisitId}", visitId);
            }
        }

        /// <summary>
        /// Logs payment confirmation
        /// </summary>
        public async Task LogPaymentConfirmationAsync(int paymentTransactionId, string userId, decimal amount, string? description = null)
        {
            try
            {
                var changeLog = new EntityChangeLog
                {
                    EntityName = "PaymentTransaction",
                    EntityId = paymentTransactionId,
                    UserId = userId,
                    OldValue = null,
                    NewValue = JsonSerializer.Serialize(new { Status = "Confirmed", Amount = amount }, _jsonOptions),
                    ChangeType = "Updated",
                    ChangeDescription = description ?? $"Payment confirmed: {amount:C}",
                    ChangeDate = DateTime.UtcNow
                };

                var context = _unitOfWork.GetContext();
                await context.EntityChangeLogs.AddAsync(changeLog);
                await context.SaveChangesAsync();

                _logger.LogInformation("Payment confirmation logged: Transaction #{TransactionId}, Amount: {Amount}, User: {UserId}",
                    paymentTransactionId, amount, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log payment confirmation for Transaction #{TransactionId}", paymentTransactionId);
            }
        }

        /// <summary>
        /// Logs assignment changes
        /// </summary>
        public async Task LogAssignmentChangeAsync(int visitId, string? oldEngineerId, string? newEngineerId, string userId, string? description = null)
        {
            try
            {
                var changeLog = new EntityChangeLog
                {
                    EntityName = "MaintenanceVisit",
                    EntityId = visitId,
                    UserId = userId,
                    OldValue = JsonSerializer.Serialize(new { EngineerId = oldEngineerId }, _jsonOptions),
                    NewValue = JsonSerializer.Serialize(new { EngineerId = newEngineerId }, _jsonOptions),
                    ChangeType = "Updated",
                    ChangeDescription = description ?? $"Engineer assignment changed",
                    ChangeDate = DateTime.UtcNow
                };

                var context = _unitOfWork.GetContext();
                await context.EntityChangeLogs.AddAsync(changeLog);
                await context.SaveChangesAsync();

                _logger.LogInformation("Assignment change logged: Visit #{VisitId}, User: {UserId}",
                    visitId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log assignment change for Visit #{VisitId}", visitId);
            }
        }
    }
}

