using System.ComponentModel.DataAnnotations;

namespace Lab1.DTO
{
    public class DepartmentDTO
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class DepartmentResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
    }
}
