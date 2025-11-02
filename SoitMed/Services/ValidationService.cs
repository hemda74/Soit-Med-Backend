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

            if (!string.IsNullOrEmpty(createDto.Type) && createDto.Type.Length > 50)
                errors.Add("نوع العميل لا يجب أن يتجاوز 50 حرف");

            if (!string.IsNullOrEmpty(createDto.Specialization) && createDto.Specialization.Length > 100)
                errors.Add("التخصص لا يجب أن يتجاوز 100 حرف");

            // Validate email format
            if (!string.IsNullOrEmpty(createDto.Email) && !IsValidEmail(createDto.Email))
                errors.Add("صيغة البريد الإلكتروني غير صحيحة");

            // Validate phone format
            if (!string.IsNullOrEmpty(createDto.Phone) && !IsValidPhone(createDto.Phone))
                errors.Add("صيغة رقم الهاتف غير صحيحة");

            // Validate status and priority values
            if (!IsValidStatus(createDto.Status))
                errors.Add("حالة العميل غير صحيحة");

            if (!IsValidPriority(createDto.Priority))
                errors.Add("أولوية العميل غير صحيحة");

            // Validate potential value
            if (createDto.PotentialValue.HasValue && createDto.PotentialValue < 0)
                errors.Add("القيمة المحتملة لا يجب أن تكون سالبة");

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

            if (!string.IsNullOrEmpty(updateDto.Type) && updateDto.Type.Length > 50)
                errors.Add("نوع العميل لا يجب أن يتجاوز 50 حرف");

            if (!string.IsNullOrEmpty(updateDto.Specialization) && updateDto.Specialization.Length > 100)
                errors.Add("التخصص لا يجب أن يتجاوز 100 حرف");

            // Validate email format
            if (!string.IsNullOrEmpty(updateDto.Email) && !IsValidEmail(updateDto.Email))
                errors.Add("صيغة البريد الإلكتروني غير صحيحة");

            // Validate phone format
            if (!string.IsNullOrEmpty(updateDto.Phone) && !IsValidPhone(updateDto.Phone))
                errors.Add("صيغة رقم الهاتف غير صحيحة");

            // Validate status and priority values
            if (!string.IsNullOrEmpty(updateDto.Status) && !IsValidStatus(updateDto.Status))
                errors.Add("حالة العميل غير صحيحة");

            if (!string.IsNullOrEmpty(updateDto.Priority) && !IsValidPriority(updateDto.Priority))
                errors.Add("أولوية العميل غير صحيحة");

            // Validate potential value
            if (updateDto.PotentialValue.HasValue && updateDto.PotentialValue < 0)
                errors.Add("القيمة المحتملة لا يجب أن تكون سالبة");

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

            if (string.IsNullOrWhiteSpace(findDto.Type))
                errors.Add("نوع العميل مطلوب");

            if (findDto.Name.Length > 200)
                errors.Add("اسم العميل لا يجب أن يتجاوز 200 حرف");

            if (findDto.Type.Length > 50)
                errors.Add("نوع العميل لا يجب أن يتجاوز 50 حرف");

            if (!string.IsNullOrEmpty(findDto.Specialization) && findDto.Specialization.Length > 100)
                errors.Add("التخصص لا يجب أن يتجاوز 100 حرف");

            if (!IsValidType(findDto.Type))
                errors.Add("نوع العميل غير صحيح");

            if (errors.Any())
                return ValidationResult.Failure(errors, "VALIDATION_ERROR");

            return ValidationResult.Success();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhone(string phone)
        {
            // Basic phone validation - can be enhanced based on requirements
            return phone.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ' || c == '(' || c == ')');
        }

        private static bool IsValidStatus(string status)
        {
            return status == ClientStatus.Potential || 
                   status == ClientStatus.Active || 
                   status == ClientStatus.Inactive || 
                   status == ClientStatus.Lost;
        }

        private static bool IsValidPriority(string priority)
        {
            return priority == ClientPriority.Low || 
                   priority == ClientPriority.Medium || 
                   priority == ClientPriority.High;
        }

        private static bool IsValidType(string type)
        {
            return type == ClientTypeConstants.Doctor || 
                   type == ClientTypeConstants.Hospital || 
                   type == ClientTypeConstants.Clinic || 
                   type == ClientTypeConstants.Pharmacy || 
                   type == ClientTypeConstants.Laboratory;
        }
    }
}
