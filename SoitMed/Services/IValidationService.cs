using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Service interface for business rules validation
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates client creation data
        /// </summary>
        Task<ValidationResult> ValidateClientCreationAsync(CreateClientDTO createDto);

        /// <summary>
        /// Validates client update data
        /// </summary>
        Task<ValidationResult> ValidateClientUpdateAsync(long clientId, UpdateClientDTO updateDto);

        /// <summary>
        /// Validates client search parameters
        /// </summary>
        ValidationResult ValidateClientSearch(SearchClientDTO searchDto);

        /// <summary>
        /// Validates client find or create parameters
        /// </summary>
        ValidationResult ValidateClientFindOrCreate(FindOrCreateClientDTO findDto);
    }

    /// <summary>
    /// Validation result wrapper
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? ErrorCode { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failure(string error, string? errorCode = null)
        {
            return new ValidationResult { IsValid = false, Errors = new List<string> { error }, ErrorCode = errorCode };
        }

        public static ValidationResult Failure(List<string> errors, string? errorCode = null)
        {
            return new ValidationResult { IsValid = false, Errors = errors, ErrorCode = errorCode };
        }
    }
}

