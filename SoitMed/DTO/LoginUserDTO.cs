using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
	public class LoginUserDTO
	{
		[Required]
		public required string UserName {  get; set; }
		
		[Required]
		public required string Password { get; set; }
	}
}
