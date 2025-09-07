using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class EngineerDTO
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Specialty { get; set; }

        public string? UserId { get; set; }

        public List<int>? GovernorateIds { get; set; }
    }

    public class EngineerResponseDTO
    {
        public int EngineerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? UserId { get; set; }
        public List<GovernorateSimpleDTO> Governorates { get; set; } = new List<GovernorateSimpleDTO>();
    }

    public class GovernorateDTO
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
    }

    public class GovernorateResponseDTO
    {
        public int GovernorateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int EngineerCount { get; set; }
    }

    public class GovernorateSimpleDTO
    {
        public int GovernorateId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AssignEngineerToGovernorateDTO
    {
        [Required]
        public int EngineerId { get; set; }

        [Required]
        public int GovernorateId { get; set; }
    }
}
