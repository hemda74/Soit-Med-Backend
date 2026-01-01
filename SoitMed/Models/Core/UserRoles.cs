using System.Linq;

namespace SoitMed.Models.Core
{
    public static class UserRoles
    {
        // Administrative Roles
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        
        // Medical Department Roles
        public const string Doctor = "Doctor";
        public const string Technician = "Technician";
        
        // Sales Department Roles
        public const string SalesManager = "SalesManager";
        public const string SalesMan = "SalesMan";
        public const string SalesSupport = "SalesSupport";
        
        // Engineering Department Roles
        public const string Engineer = "Engineer";
        public const string MaintenanceManager = "MaintenanceManager";
        public const string MaintenanceSupport = "MaintenanceSupport";
        public const string SparePartsCoordinator = "SparePartsCoordinator";
        public const string InventoryManager = "InventoryManager";
        
        // Finance Department Roles
        public const string FinanceManager = "FinanceManager";
        public const string FinanceEmployee = "FinanceEmployee";
        public const string CollectionDelegate = "CollectionDelegate";
        
        // Legal Department Roles
        public const string LegalManager = "LegalManager";
        public const string LegalEmployee = "LegalEmployee";

        // Get all available roles - easily extensible
        public static List<string> GetAllRoles()
        {
            return new List<string>
            {
                SuperAdmin,
                Admin,
                Doctor,
                Technician,
                SalesManager,
                SalesMan,
                SalesSupport,
                Engineer,
                FinanceManager,
                FinanceEmployee,
                CollectionDelegate,
                LegalManager,
                LegalEmployee,
                MaintenanceManager,
                MaintenanceSupport,
                SparePartsCoordinator,
                InventoryManager
            };
        }

        // Get roles by department for future route management
        public static Dictionary<string, List<string>> GetRolesByDepartment()
        {
            return new Dictionary<string, List<string>>
            {
                { "Administration", new List<string> { SuperAdmin, Admin } },
                { "Medical", new List<string> { Doctor, Technician } },
                { "Sales", new List<string> { SalesManager, SalesMan, SalesSupport } },
                { "Engineering", new List<string> { Engineer, MaintenanceManager, MaintenanceSupport, SparePartsCoordinator, InventoryManager } },
                { "Finance", new List<string> { FinanceManager, FinanceEmployee, CollectionDelegate } },
                { "Legal", new List<string> { LegalManager, LegalEmployee } }
            };
        }

        // Get manager roles for each department
        public static List<string> GetManagerRoles()
        {
            return new List<string>
            {
                SuperAdmin,
                Admin,
                SalesManager,
                FinanceManager,
                LegalManager,
                MaintenanceManager
            };
        }

        // Get department for a specific role
        public static string GetDepartmentForRole(string role)
        {
            var rolesByDepartment = GetRolesByDepartment();
            foreach (var department in rolesByDepartment)
            {
                if (department.Value.Contains(role))
                {
                    return department.Key;
                }
            }
            return "Unknown";
        }

        // Validate if a role exists
        public static bool IsValidRole(string role)
        {
            return GetAllRoles().Contains(role);
        }

        // Get roles that can register other users (for future authorization)
        public static List<string> GetAdminRoles()
        {
            return new List<string>
            {
                SuperAdmin,
                Admin,
                SalesManager,
                FinanceManager,
                LegalManager,
                MaintenanceManager
            };
        }

        // Check if role is a manager role
        public static bool IsManagerRole(string role)
        {
            return GetManagerRoles().Contains(role);
        }

        // Normalize and validate role name
        // Converts lowercase, hyphenated, or underscored role names to the proper capitalized format
        // Returns the normalized role name if valid, or null if invalid
        public static string? NormalizeAndValidateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return null;
            }

            // Remove hyphens and underscores, then convert to PascalCase
            var normalized = roleName
                .Replace("-", "")
                .Replace("_", "")
                .Trim();

            // Convert to PascalCase (first letter uppercase, rest as-is but ensure proper casing)
            if (normalized.Length == 0)
            {
                return null;
            }

            // Capitalize first letter
            normalized = char.ToUpper(normalized[0]) + (normalized.Length > 1 ? normalized.Substring(1) : "");

            // Try to find exact match first
            if (IsValidRole(normalized))
            {
                return normalized;
            }

            // Try case-insensitive match
            var allRoles = GetAllRoles();
            var matchedRole = allRoles.FirstOrDefault(r => 
                r.Equals(normalized, StringComparison.OrdinalIgnoreCase));

            if (matchedRole != null)
            {
                return matchedRole;
            }

            // Try matching after removing hyphens/underscores from both
            matchedRole = allRoles.FirstOrDefault(r =>
            {
                var normalizedRole = r.Replace("-", "").Replace("_", "");
                var normalizedInput = normalized.Replace("-", "").Replace("_", "");
                return normalizedRole.Equals(normalizedInput, StringComparison.OrdinalIgnoreCase);
            });

            return matchedRole;
        }
    }
}
