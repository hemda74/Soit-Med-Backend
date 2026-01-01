using SoitMed.Models.Enums;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for audit logging service
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Logs entity changes (old and new values)
        /// </summary>
        Task LogEntityChangeAsync<T>(T? oldEntity, T? newEntity, string userId, string changeType, string? description = null) where T : class;

        /// <summary>
        /// Logs a status change for a visit
        /// </summary>
        Task LogStatusChangeAsync(int visitId, VisitStatus oldStatus, VisitStatus newStatus, string userId, string? description = null);

        /// <summary>
        /// Logs payment confirmation
        /// </summary>
        Task LogPaymentConfirmationAsync(int paymentTransactionId, string userId, decimal amount, string? description = null);

        /// <summary>
        /// Logs assignment changes
        /// </summary>
        Task LogAssignmentChangeAsync(int visitId, string? oldEngineerId, string? newEngineerId, string userId, string? description = null);
    }
}

