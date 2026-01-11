using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Legacy;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing legacy employees from TBS system
    /// </summary>
    public class LegacyEmployeeService : ILegacyEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Context _context;
        private readonly ILogger<LegacyEmployeeService> _logger;

        public LegacyEmployeeService(
            IUnitOfWork unitOfWork,
            Context context,
            ILogger<LegacyEmployeeService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get legacy employee by legacy employee ID (EmpCode from TBS)
        /// </summary>
        public async Task<LegacyEmployee?> GetByLegacyEmployeeIdAsync(int legacyEmployeeId)
        {
            try
            {
                return await _context.LegacyEmployees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(le => le.LegacyEmployeeId == legacyEmployeeId && le.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving legacy employee with ID {LegacyEmployeeId}", legacyEmployeeId);
                return null;
            }
        }

        /// <summary>
        /// Get employee name (Arabic or English) by legacy employee ID
        /// </summary>
        public async Task<string?> GetEmployeeNameAsync(int legacyEmployeeId, bool preferEnglish = false)
        {
            try
            {
                var employee = await GetByLegacyEmployeeIdAsync(legacyEmployeeId);
                
                if (employee == null)
                    return null;

                // Prefer English if requested and available, otherwise use Arabic
                if (preferEnglish && !string.IsNullOrWhiteSpace(employee.NameEn))
                    return employee.NameEn;

                return employee.Name ?? employee.NameEn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee name for legacy employee ID {LegacyEmployeeId}", legacyEmployeeId);
                return null;
            }
        }
    }
}
