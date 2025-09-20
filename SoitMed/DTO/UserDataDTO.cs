using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class UserDataDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        
        // Department information
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentDescription { get; set; }
        
        // Profile image information
        public UserImageDTO? ProfileImage { get; set; }
        public List<UserImageDTO> UserImages { get; set; } = new List<UserImageDTO>();
    }

    public class CurrentUserDataDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        
        // Department information
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentDescription { get; set; }
        
        // Additional current user specific data
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        
        // Profile image information
        public UserImageDTO? ProfileImage { get; set; }
        public List<UserImageDTO> UserImages { get; set; } = new List<UserImageDTO>();
    }
}

