using System.ComponentModel.DataAnnotations;

namespace Lab1.DTO
{
	public class LoginUserDTO
	{
		[Required]
		public required string UserName {  get; set; }
		
		[Required]
		public required string Password { get; set; }
	}
}
