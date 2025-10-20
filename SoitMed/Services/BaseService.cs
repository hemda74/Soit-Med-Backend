using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Base service class providing common functionality for all services
    /// </summary>
    public abstract class BaseService
    {
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly ILogger Logger;

        protected BaseService(IUnitOfWork unitOfWork, ILogger logger)
        {
            UnitOfWork = unitOfWork;
            Logger = logger;
        }

        /// <summary>
        /// Handles service exceptions with proper logging
        /// </summary>
        protected ServiceResult<T> HandleException<T>(Exception ex, string operation, string errorMessage)
        {
            Logger.LogError(ex, "Error in {Operation}", operation);
            return ServiceResult<T>.Failure(errorMessage, "SERVICE_ERROR");
        }

        /// <summary>
        /// Validates that an entity exists
        /// </summary>
        protected async Task<bool> EntityExistsAsync<T>(object id) where T : class
        {
            try
            {
                // This would need to be implemented based on the specific repository pattern
                // For now, we'll return true and let the calling method handle the validation
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error checking if entity {EntityType} with id {Id} exists", typeof(T).Name, id);
                return false;
            }
        }

        /// <summary>
        /// Gets an entity by ID with null check
        /// </summary>
        protected async Task<T?> GetEntityByIdAsync<T>(object id) where T : class
        {
            try
            {
                // This would need to be implemented based on the specific repository pattern
                // For now, we'll return null and let the calling method handle the validation
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting entity {EntityType} with id {Id}", typeof(T).Name, id);
                return null;
            }
        }
    }
}
