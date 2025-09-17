using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Hospital;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Services
{
    public class UserIdGenerationService
    {
        private readonly Context _context;

        public UserIdGenerationService(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates a unique user ID in the format: FirstName_LastName_DepartmentName_Number
        /// For users/technicians, uses Hospital name instead of Department name
        /// </summary>
        public async Task<string> GenerateUserIdAsync(string firstName, string lastName, string role, int? departmentId = null, string? hospitalId = null)
        {
            // Clean names (remove spaces, special characters, convert to proper case)
            string cleanFirstName = CleanName(firstName);
            string cleanLastName = CleanName(lastName);
            
            string organizationName;
            
            // Determine organization name based on role
            if (role == "Doctor" || role == "Technician")
            {
                // For doctors and technicians, use hospital name
                if (string.IsNullOrEmpty(hospitalId))
                {
                    throw new ArgumentException("Hospital ID is required for Doctor and Technician roles");
                }
                
                var hospital = await _context.Hospitals.FindAsync(hospitalId);
                if (hospital == null)
                {
                    throw new ArgumentException($"Hospital with ID {hospitalId} not found");
                }
                
                organizationName = CleanName(hospital.Name);
            }
            else
            {
                // For other roles, use department name
                if (departmentId == null)
                {
                    throw new ArgumentException("Department ID is required for non-Doctor/Technician roles");
                }
                
                var department = await _context.Departments.FindAsync(departmentId);
                if (department == null)
                {
                    throw new ArgumentException($"Department with ID {departmentId} not found");
                }
                
                organizationName = CleanName(department.Name);
            }

            // Generate the base ID pattern
            string baseId = $"{cleanFirstName}_{cleanLastName}_{organizationName}";
            
            // Find the next available number for this pattern
            int nextNumber = await GetNextAvailableNumberAsync(baseId);
            
            return $"{baseId}_{nextNumber:D3}"; // Format number with leading zeros (001, 002, etc.)
        }

        /// <summary>
        /// Cleans a name by removing spaces, special characters, and converting to proper case
        /// </summary>
        private string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // Remove extra spaces and convert to proper case
            name = name.Trim();
            
            // Remove special characters except letters, numbers, and spaces
            name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9\s]", "");
            
            // Convert to proper case (first letter of each word capitalized)
            name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            
            // Remove spaces
            name = name.Replace(" ", "");
            
            return name;
        }

        /// <summary>
        /// Finds the next available number for a given base ID pattern
        /// </summary>
        private async Task<int> GetNextAvailableNumberAsync(string baseId)
        {
            // Get all existing user IDs that start with the base pattern
            var existingIds = await _context.Users
                .Where(u => u.Id.StartsWith(baseId + "_"))
                .Select(u => u.Id)
                .ToListAsync();

            // Extract numbers from existing IDs
            var usedNumbers = new HashSet<int>();
            
            foreach (var id in existingIds)
            {
                // Extract the number part after the last underscore
                var parts = id.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int number))
                {
                    usedNumbers.Add(number);
                }
            }

            // Find the first available number starting from 1
            int nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return nextNumber;
        }

        /// <summary>
        /// Validates if a generated ID is unique
        /// </summary>
        public async Task<bool> IsUserIdUniqueAsync(string userId)
        {
            return !await _context.Users.AnyAsync(u => u.Id == userId);
        }

    }
}
