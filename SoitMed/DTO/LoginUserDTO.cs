using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
	public class LoginUserDTO
	{
		[Required]
		public required string UserName {  get; set; } // Can be either username or email
		
		[Required]
		public required string Password { get; set; }
	}
}
