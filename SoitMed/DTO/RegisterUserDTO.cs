using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
	public class RegisterUserDTO
	{
		[Required]
		public required string Password { get; set; }
		
		[Required]
		[EmailAddress]
		public required string Email { get; set; }
		
		[Required]
		public required string Role { get; set; }

		// Optional user details
		public string? FirstName { get; set; }
		public string? LastName { get; set; }

		// Optional phone number
		[Phone(ErrorMessage = "Invalid phone number format")]
		[StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
		public string? PhoneNumber { get; set; }

		// Optional personal mail
		[EmailAddress(ErrorMessage = "Please provide a valid personal email address")]
		[MaxLength(200, ErrorMessage = "Personal mail cannot exceed 200 characters")]
		public string? PersonalMail { get; set; }

		// Department assignment (optional - will be auto-assigned based on role if not provided)
		public int? DepartmentId { get; set; }
	}
}
