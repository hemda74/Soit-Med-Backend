using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SoitMed.DTO
{
    public class UpdateUserImageDTO
    {
        [MaxLength(500, ErrorMessage = "Alt text cannot exceed 500 characters")]
        [Description("Alternative text for the profile image")]
        public string? AltText { get; set; }
    }

    public class UpdateUserImageResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public UserImageInfoDTO? ProfileImage { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DeleteUserImageResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime DeletedAt { get; set; }
    }
}
