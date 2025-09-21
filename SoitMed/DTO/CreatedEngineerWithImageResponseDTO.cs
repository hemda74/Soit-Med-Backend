namespace SoitMed.DTO
{
    public class CreatedEngineerWithImageResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string EngineerId { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public List<string> GovernorateNames { get; set; } = new List<string>();
        public EngineerImageInfo? ProfileImage { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class EngineerImageInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? AltText { get; set; }
        public bool IsProfileImage { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}

