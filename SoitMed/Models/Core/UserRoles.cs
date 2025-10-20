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
        public const string Salesman = "Salesman";
        public const string SalesSupport = "SalesSupport";
        
        // Engineering Department Roles
        public const string Engineer = "Engineer";
        public const string MaintenanceManager = "MaintenanceManager";
        public const string MaintenanceSupport = "MaintenanceSupport";
        
        // Finance Department Roles
        public const string FinanceManager = "FinanceManager";
        public const string FinanceEmployee = "FinanceEmployee";
        
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
                Salesman,
                SalesSupport,
                Engineer,
                FinanceManager,
                FinanceEmployee,
                LegalManager,
                LegalEmployee,
                MaintenanceManager,
                MaintenanceSupport
            };
        }

        // Get roles by department for future route management
        public static Dictionary<string, List<string>> GetRolesByDepartment()
        {
            return new Dictionary<string, List<string>>
            {
                { "Administration", new List<string> { SuperAdmin, Admin } },
                { "Medical", new List<string> { Doctor, Technician } },
                { "Sales", new List<string> { SalesManager, Salesman, SalesSupport } },
                { "Engineering", new List<string> { Engineer, MaintenanceManager, MaintenanceSupport } },
                { "Finance", new List<string> { FinanceManager, FinanceEmployee } },
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
    }
}
