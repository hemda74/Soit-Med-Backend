using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
	/// <summary>
	/// DTO for users to update their own profile
	/// </summary>
	public class UpdateMyProfileDTO
	{
		[MaxLength(100)]
		public string? FirstName { get; set; }

		[MaxLength(100)]
		public string? LastName { get; set; }

		[Phone]
		public string? PhoneNumber { get; set; }

		[EmailAddress]
		public string? PersonalMail { get; set; }

		public DateTime? DateOfBirth { get; set; }
	}
}

