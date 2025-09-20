using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class UserImageDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? AltText { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsProfileImage { get; set; }
    }

    public class CreateUserImageDTO
    {
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContentType { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "File size must be greater than 0")]
        public long FileSize { get; set; }

        [MaxLength(500)]
        public string? AltText { get; set; }

        public bool IsProfileImage { get; set; } = false;
    }

    public class UpdateUserImageDTO
    {
        [MaxLength(500)]
        public string? AltText { get; set; }

        public bool? IsProfileImage { get; set; }
    }

    public class UploadUserImageDTO
    {
        [Required(ErrorMessage = "File is required.")]
        public IFormFile File { get; set; } = null!;

        [MaxLength(500)]
        public string? AltText { get; set; }
    }

    public class UserImageUploadResponseDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? AltText { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsProfileImage { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
