using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for client follow-up information
    /// </summary>
    public class ClientFollowUpDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime? LastContactDate { get; set; }
        public DateTime? NextContactDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? SatisfactionRating { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
    }
}
