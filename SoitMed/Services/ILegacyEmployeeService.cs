using SoitMed.Models.Legacy;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing legacy employees from TBS system
    /// </summary>
    public interface ILegacyEmployeeService
    {
        /// <summary>
        /// Get legacy employee by legacy employee ID (EmpCode from TBS)
        /// </summary>
        Task<LegacyEmployee?> GetByLegacyEmployeeIdAsync(int legacyEmployeeId);

        /// <summary>
        /// Get employee name (Arabic or English) by legacy employee ID
        /// </summary>
        Task<string?> GetEmployeeNameAsync(int legacyEmployeeId, bool preferEnglish = false);
    }
}
