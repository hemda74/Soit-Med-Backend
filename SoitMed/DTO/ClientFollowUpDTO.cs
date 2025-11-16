namespace SoitMed.DTO
{
    public class ClientFollowUpDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? OrganizationName { get; set; }
        public string? AssignedTo { get; set; }
    }
}
