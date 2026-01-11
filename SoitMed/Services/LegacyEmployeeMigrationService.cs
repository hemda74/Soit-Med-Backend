using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoitMed.Models;
using SoitMed.Models.Legacy;
using SoitMed.Repositories;
using System.Data;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for migrating employees from TBS database to LegacyEmployees table
    /// Uses raw SQL queries to access TBS.EmpMas table
    /// </summary>
    public class LegacyEmployeeMigrationService : ILegacyEmployeeMigrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Context _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LegacyEmployeeMigrationService> _logger;

        public LegacyEmployeeMigrationService(
            IUnitOfWork unitOfWork,
            Context context,
            IConfiguration configuration,
            ILogger<LegacyEmployeeMigrationService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Migrate all employees from TBS.EmpMas to LegacyEmployees table
        /// </summary>
        public async Task<EmployeeMigrationResult> MigrateEmployeesAsync()
        {
            var result = new EmployeeMigrationResult
            {
                StartTime = DateTime.UtcNow
            };

            try
            {
                var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                if (string.IsNullOrEmpty(tbsConnectionString))
                {
                    result.Success = false;
                    result.Message = "TbsConnection string not configured";
                    result.EndTime = DateTime.UtcNow;
                    return result;
                }

                // Use raw SQL to query TBS database
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(tbsConnectionString);
                await connection.OpenAsync();

                // Get all employees from TBS
                var employeesQuery = @"
                    SELECT 
                        e.EmpCode,
                        e.EmpNma,
                        e.EmpNmaEn,
                        e.DepartMentCode,
                        d.DepartmentNma,
                        e.JobCode,
                        j.JobNMA,
                        e.Tel_Phone,
                        e.Tel_Mob,
                        e.Email,
                        e.Valid
                    FROM EmpMas e
                    LEFT JOIN DepartmentMas d ON e.DepartMentCode = d.DepartmentCode
                    LEFT JOIN JobMas j ON e.JobCode = j.JobCode";

                using var command = new Microsoft.Data.SqlClient.SqlCommand(employeesQuery, connection);
                using var reader = await command.ExecuteReaderAsync();

                var employees = new List<EmployeeData>();
                while (await reader.ReadAsync())
                {
                    employees.Add(new EmployeeData
                    {
                        EmpCode = reader.GetInt32(reader.GetOrdinal("EmpCode")),
                        EmpNma = reader.IsDBNull(reader.GetOrdinal("EmpNma")) ? null : reader.GetString(reader.GetOrdinal("EmpNma")),
                        EmpNmaEn = reader.IsDBNull(reader.GetOrdinal("EmpNmaEn")) ? null : reader.GetString(reader.GetOrdinal("EmpNmaEn")),
                        DepartMentCode = reader.IsDBNull(reader.GetOrdinal("DepartMentCode")) ? null : reader.GetInt32(reader.GetOrdinal("DepartMentCode")),
                        DepartmentNma = reader.IsDBNull(reader.GetOrdinal("DepartmentNma")) ? null : reader.GetString(reader.GetOrdinal("DepartmentNma")),
                        JobCode = reader.IsDBNull(reader.GetOrdinal("JobCode")) ? null : reader.GetInt32(reader.GetOrdinal("JobCode")),
                        JobNMA = reader.IsDBNull(reader.GetOrdinal("JobNMA")) ? null : reader.GetString(reader.GetOrdinal("JobNMA")),
                        TelPhone = reader.IsDBNull(reader.GetOrdinal("Tel_Phone")) ? null : reader.GetString(reader.GetOrdinal("Tel_Phone")),
                        TelMob = reader.IsDBNull(reader.GetOrdinal("Tel_Mob")) ? null : reader.GetString(reader.GetOrdinal("Tel_Mob")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                        Valid = reader.IsDBNull(reader.GetOrdinal("Valid")) ? 0 : reader.GetInt32(reader.GetOrdinal("Valid"))
                    });
                }

                result.TotalProcessed = employees.Count;
                _logger.LogInformation("Found {Count} employees in TBS database", employees.Count);

                // Migrate each employee
                foreach (var emp in employees)
                {
                    try
                    {
                        // Check if already exists
                        var exists = await _context.LegacyEmployees
                            .AnyAsync(le => le.LegacyEmployeeId == emp.EmpCode);

                        if (exists)
                        {
                            result.SkippedCount++;
                            continue;
                        }

                        // Create new LegacyEmployee
                        var legacyEmployee = new LegacyEmployee
                        {
                            LegacyEmployeeId = emp.EmpCode,
                            Name = emp.EmpNma,
                            NameEn = emp.EmpNmaEn,
                            DepartmentCode = emp.DepartMentCode,
                            DepartmentName = emp.DepartmentNma,
                            JobCode = emp.JobCode,
                            JobName = emp.JobNMA,
                            Phone = emp.TelPhone,
                            Mobile = emp.TelMob,
                            Email = emp.Email,
                            IsActive = emp.Valid == 1,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.LegacyEmployees.Add(legacyEmployee);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        var errorMsg = $"Error migrating employee {emp.EmpCode}: {ex.Message}";
                        result.Errors.Add(errorMsg);
                        _logger.LogError(ex, "Error migrating employee {EmpCode}", emp.EmpCode);
                    }
                }

                // Save all changes
                if (result.SuccessCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully migrated {Count} employees", result.SuccessCount);
                }

                result.Message = $"Migration completed: {result.SuccessCount} migrated, {result.SkippedCount} skipped, {result.ErrorCount} errors";
                result.EndTime = DateTime.UtcNow;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during employee migration");
                result.Success = false;
                result.Message = $"Migration failed: {ex.Message}";
                result.EndTime = DateTime.UtcNow;
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        /// <summary>
        /// Migrate a single employee by EmpCode
        /// </summary>
        public async Task<bool> MigrateEmployeeAsync(int empCode)
        {
            try
            {
                var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                if (string.IsNullOrEmpty(tbsConnectionString))
                {
                    _logger.LogWarning("TbsConnection string not configured");
                    return false;
                }

                // Check if already exists
                var exists = await _context.LegacyEmployees
                    .AnyAsync(le => le.LegacyEmployeeId == empCode);

                if (exists)
                {
                    _logger.LogInformation("Employee {EmpCode} already exists in LegacyEmployees", empCode);
                    return true;
                }

                // Use raw SQL to query TBS database
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(tbsConnectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        e.EmpCode,
                        e.EmpNma,
                        e.EmpNmaEn,
                        e.DepartMentCode,
                        d.DepartmentNma,
                        e.JobCode,
                        j.JobNMA,
                        e.Tel_Phone,
                        e.Tel_Mob,
                        e.Email,
                        e.Valid
                    FROM EmpMas e
                    LEFT JOIN DepartmentMas d ON e.DepartMentCode = d.DepartmentCode
                    LEFT JOIN JobMas j ON e.JobCode = j.JobCode
                    WHERE e.EmpCode = @EmpCode";

                using var command = new Microsoft.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmpCode", empCode);
                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    _logger.LogWarning("Employee {EmpCode} not found in TBS database", empCode);
                    return false;
                }

                var legacyEmployee = new LegacyEmployee
                {
                    LegacyEmployeeId = reader.GetInt32(reader.GetOrdinal("EmpCode")),
                    Name = reader.IsDBNull(reader.GetOrdinal("EmpNma")) ? null : reader.GetString(reader.GetOrdinal("EmpNma")),
                    NameEn = reader.IsDBNull(reader.GetOrdinal("EmpNmaEn")) ? null : reader.GetString(reader.GetOrdinal("EmpNmaEn")),
                    DepartmentCode = reader.IsDBNull(reader.GetOrdinal("DepartMentCode")) ? null : reader.GetInt32(reader.GetOrdinal("DepartMentCode")),
                    DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentNma")) ? null : reader.GetString(reader.GetOrdinal("DepartmentNma")),
                    JobCode = reader.IsDBNull(reader.GetOrdinal("JobCode")) ? null : reader.GetInt32(reader.GetOrdinal("JobCode")),
                    JobName = reader.IsDBNull(reader.GetOrdinal("JobNMA")) ? null : reader.GetString(reader.GetOrdinal("JobNMA")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Tel_Phone")) ? null : reader.GetString(reader.GetOrdinal("Tel_Phone")),
                    Mobile = reader.IsDBNull(reader.GetOrdinal("Tel_Mob")) ? null : reader.GetString(reader.GetOrdinal("Tel_Mob")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    IsActive = reader.IsDBNull(reader.GetOrdinal("Valid")) ? false : reader.GetInt32(reader.GetOrdinal("Valid")) == 1,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LegacyEmployees.Add(legacyEmployee);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully migrated employee {EmpCode}", empCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating employee {EmpCode}", empCode);
                return false;
            }
        }

        /// <summary>
        /// Helper class for employee data from TBS
        /// </summary>
        private class EmployeeData
        {
            public int EmpCode { get; set; }
            public string? EmpNma { get; set; }
            public string? EmpNmaEn { get; set; }
            public int? DepartMentCode { get; set; }
            public string? DepartmentNma { get; set; }
            public int? JobCode { get; set; }
            public string? JobNMA { get; set; }
            public string? TelPhone { get; set; }
            public string? TelMob { get; set; }
            public string? Email { get; set; }
            public int Valid { get; set; }
        }
    }
}
