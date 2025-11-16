using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service implementation for business rules validation
    /// </summary>
    public class ValidationService : BaseService, IValidationService
    {
        public ValidationService(IUnitOfWork unitOfWork, ILogger<ValidationService> logger) 
            : base(unitOfWork, logger)
        {
        }

        public async Task<ValidationResult> ValidateClientCreationAsync(CreateClientDTO createDto)
        {
            var errors = new List<string>();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(createDto.Name))
                errors.Add("اسم العميل مطلوب");

            // Validate field lengths
            if (createDto.Name.Length > 200)
                errors.Add("اسم العميل لا يجب أن يتجاوز 200 حرف");

            if (!string.IsNullOrEmpty(createDto.OrganizationName) && createDto.OrganizationName.Length > 200)
                errors.Add("اسم المؤسسة لا يجب أن يتجاوز 200 حرف");

            // Validate phone format
            if (!string.IsNullOrEmpty(createDto.Phone) && !IsValidPhone(createDto.Phone))
                errors.Add("صيغة رقم الهاتف غير صحيحة");

            // Validate classification
            if (!string.IsNullOrEmpty(createDto.Classification) && !Models.ClientClassificationConstants.IsValidClassification(createDto.Classification))
                errors.Add("التصنيف يجب أن يكون A أو B أو C أو D");

            if (errors.Any())
                return ValidationResult.Failure(errors, "VALIDATION_ERROR");

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateClientUpdateAsync(long clientId, UpdateClientDTO updateDto)
        {
            var errors = new List<string>();

            // Check if client exists
            var client = await UnitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null)
                return ValidationResult.Failure("العميل غير موجود", "CLIENT_NOT_FOUND");

            // Validate field lengths
            if (!string.IsNullOrEmpty(updateDto.Name) && updateDto.Name.Length > 200)
                errors.Add("اسم العميل لا يجب أن يتجاوز 200 حرف");

            if (!string.IsNullOrEmpty(updateDto.OrganizationName) && updateDto.OrganizationName.Length > 200)
                errors.Add("اسم المؤسسة لا يجب أن يتجاوز 200 حرف");

            // Validate phone format
            if (!string.IsNullOrEmpty(updateDto.Phone) && !IsValidPhone(updateDto.Phone))
                errors.Add("صيغة رقم الهاتف غير صحيحة");

            // Validate classification
            if (!string.IsNullOrEmpty(updateDto.Classification) && !Models.ClientClassificationConstants.IsValidClassification(updateDto.Classification))
                errors.Add("التصنيف يجب أن يكون A أو B أو C أو D");

            if (errors.Any())
                return ValidationResult.Failure(errors, "VALIDATION_ERROR");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateClientSearch(SearchClientDTO searchDto)
        {
            var errors = new List<string>();

            // Search term is optional - allow empty search for browsing all clients
            // Only validate length if a search term is provided
            if (!string.IsNullOrWhiteSpace(searchDto.Query) && searchDto.Query.Length < 2)
                errors.Add("نص البحث يجب أن يكون حرفين على الأقل");

            if (searchDto.Page < 1)
                errors.Add("رقم الصفحة يجب أن يكون أكبر من صفر");

            if (searchDto.PageSize < 1 || searchDto.PageSize > 100)
                errors.Add("حجم الصفحة يجب أن يكون بين 1 و 100");

            if (errors.Any())
                return ValidationResult.Failure(errors, "VALIDATION_ERROR");

            return ValidationResult.Success();
        }

        public ValidationResult ValidateClientFindOrCreate(FindOrCreateClientDTO findDto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(findDto.Name))
                errors.Add("اسم العميل مطلوب");

            if (findDto.Name.Length > 200)
                errors.Add("اسم العميل لا يجب أن يتجاوز 200 حرف");

            if (!string.IsNullOrEmpty(findDto.OrganizationName) && findDto.OrganizationName.Length > 200)
                errors.Add("اسم المؤسسة لا يجب أن يتجاوز 200 حرف");

            if (!string.IsNullOrEmpty(findDto.Phone) && !IsValidPhone(findDto.Phone))
                errors.Add("صيغة رقم الهاتف غير صحيحة");

            if (errors.Any())
                return ValidationResult.Failure(errors, "VALIDATION_ERROR");

            return ValidationResult.Success();
        }

        private static bool IsValidPhone(string phone)
        {
            // Basic phone validation - can be enhanced based on requirements
            return phone.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ' || c == '(' || c == ')');
        }
    }
}
