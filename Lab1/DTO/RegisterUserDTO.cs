using System.ComponentModel.DataAnnotations;

namespace Lab1.DTO
{
	public class RegisterUserDTO
	{
		[Required]
		public required string UserName {  get; set; }
		
		[Required]
		public required string Password { get; set; }
		
		[Required]
		[EmailAddress]
		public required string Email { get; set; }
		
		[Required]
		public required string Role { get; set; }
	}
}
