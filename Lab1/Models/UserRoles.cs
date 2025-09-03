namespace Lab1.Models
{
    public static class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Salesman = "Salesman";
        public const string Doctor = "Doctor";
        public const string Technician = "Technician";
        public const string Engineer = "Engineer";

        // Get all available roles - easily extensible
        public static List<string> GetAllRoles()
        {
            return new List<string>
            {
                SuperAdmin,
                Admin,
                Salesman,
                Doctor,
                Technician,
                Engineer
            };
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
                Admin
            };
        }
    }
}
