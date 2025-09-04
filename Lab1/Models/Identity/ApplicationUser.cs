using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Lab1.Models.Core;

namespace Lab1.Models.Identity
{
	public class ApplicationUser:IdentityUser
	{
		// Department relationship
		public int? DepartmentId { get; set; }
		
		[ForeignKey("DepartmentId")]
		public virtual Department? Department { get; set; }

		// Additional user properties
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? LastLoginAt { get; set; }
		public bool IsActive { get; set; } = true;

		// Computed property for full name
		public string FullName => $"{FirstName} {LastName}".Trim();
	}
}
