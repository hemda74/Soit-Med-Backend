namespace SoitMed.Services
{
    /// <summary>
    /// Service for migrating employees from TBS database to LegacyEmployees table
    /// </summary>
    public interface ILegacyEmployeeMigrationService
    {
        /// <summary>
        /// Migrate all employees from TBS.EmpMas to LegacyEmployees table
        /// </summary>
        Task<EmployeeMigrationResult> MigrateEmployeesAsync();

        /// <summary>
        /// Migrate a single employee by EmpCode
        /// </summary>
        Task<bool> MigrateEmployeeAsync(int empCode);
    }

    /// <summary>
    /// Result of employee migration operation
    /// </summary>
    public class EmployeeMigrationResult
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int SkippedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
