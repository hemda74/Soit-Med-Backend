using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;

namespace SoitMed.Models.Identity
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
	public string? PersonalMail { get; set; }
	public DateTime? DateOfBirth { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? LastLoginAt { get; set; }
	public bool IsActive { get; set; } = true;

	// Customer-Client linking (optional - for Customer role users)
	public long? ClientId { get; set; }
	
	[ForeignKey("ClientId")]
	public virtual Client? Client { get; set; }

		// Computed property for full name
		public string FullName => $"{FirstName} {LastName}".Trim();

		// Navigation property for user images
		public virtual ICollection<UserImage> UserImages { get; set; } = new List<UserImage>();

		// Navigation property for profile image
		public virtual UserImage? ProfileImage => UserImages.FirstOrDefault(img => img.IsProfileImage && img.IsActive);
	}
}
