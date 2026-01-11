using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace SoitMed.DTO
{
    public class CreateClientDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Type { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Governorate { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Potential";

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        [MaxLength(10)]
        public string? Classification { get; set; }

        public int? Rating { get; set; }

        public decimal? PotentialValue { get; set; }

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? ContactPersonPhone { get; set; }

        [MaxLength(100)]
        public string? ContactPersonEmail { get; set; }

        [MaxLength(100)]
        public string? ContactPersonPosition { get; set; }

        public string? AssignedTo { get; set; }
    }

    public class UpdateClientDTO
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Governorate { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(50)]
        public string? Priority { get; set; }

        public decimal? PotentialValue { get; set; }

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? ContactPersonPhone { get; set; }

        [MaxLength(100)]
        public string? ContactPersonEmail { get; set; }

        [MaxLength(100)]
        public string? ContactPersonPosition { get; set; }

        public string? AssignedTo { get; set; }
    }

    public class SearchClientDTO
    {
        public string Query { get; set; } = string.Empty;
        public string? Classification { get; set; } // A, B, C, D
        public string? AssignedSalesManId { get; set; }
        public string? City { get; set; }
        public int? GovernorateId { get; set; }
        public List<string>? EquipmentCategories { get; set; } // List of equipment category names
        public string? ClientCategory { get; set; } // "Potential" or "Existing" - filter by client type
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class FindOrCreateClientDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Specialization { get; set; }
    }

    public class PaginatedClientsResponseDTO
    {
        public List<ClientResponseDTO> Clients { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class ClientResponseDTO
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public string? Type { get; set; } // Can be null if not provided
        
        [JsonPropertyName("organizationName")]
        public string? OrganizationName { get; set; }
        
        [JsonPropertyName("specialization")]
        public string? Specialization { get; set; }
        
        [JsonPropertyName("location")]
        public string? Location { get; set; }
        
        [JsonPropertyName("phone")]
        public string? Phone { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("website")]
        public string? Website { get; set; }
        
        [JsonPropertyName("address")]
        public string? Address { get; set; }
        
        [JsonPropertyName("city")]
        public string? City { get; set; }
        
        [JsonPropertyName("governorate")]
        public string? Governorate { get; set; }
        
        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }
        
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("priority")]
        public string Priority { get; set; } = string.Empty;
        
        [JsonPropertyName("classification")]
        public string? Classification { get; set; }
        
        [JsonPropertyName("rating")]
        public int? Rating { get; set; }
        
        [JsonPropertyName("potentialValue")]
        public decimal? PotentialValue { get; set; }
        
        [JsonPropertyName("contactPerson")]
        public string? ContactPerson { get; set; }
        
        [JsonPropertyName("contactPersonPhone")]
        public string? ContactPersonPhone { get; set; }
        
        [JsonPropertyName("contactPersonEmail")]
        public string? ContactPersonEmail { get; set; }
        
        [JsonPropertyName("contactPersonPosition")]
        public string? ContactPersonPosition { get; set; }
        
        [JsonPropertyName("lastContactDate")]
        public DateTime? LastContactDate { get; set; }
        
        [JsonPropertyName("nextContactDate")]
        public DateTime? NextContactDate { get; set; }
        
        [JsonPropertyName("satisfactionRating")]
        public int? SatisfactionRating { get; set; }
        
        [JsonPropertyName("interestedEquipmentCategories")]
        public List<string>? InterestedEquipmentCategories { get; set; } // Parsed list for frontend (use this instead of JSON string)
        
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;
        
        [JsonPropertyName("assignedTo")]
        public string? AssignedTo { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("nationalId")]
        public string? NationalId { get; set; }
        
        [JsonPropertyName("nationalIdFrontImage")]
        public string? NationalIdFrontImage { get; set; }
        
        [JsonPropertyName("nationalIdBackImage")]
        public string? NationalIdBackImage { get; set; }
        
        [JsonPropertyName("taxCardNumber")]
        public string? TaxCardNumber { get; set; }
        
        [JsonPropertyName("taxCardImage")]
        public string? TaxCardImage { get; set; }
        
        [JsonPropertyName("legalDocumentsSubmittedAt")]
        public DateTime? LegalDocumentsSubmittedAt { get; set; }
        
        [JsonPropertyName("hasAccount")]
        public bool HasAccount { get; set; }
        
        /// <summary>
        /// Indicates if the client has any equipment, contracts, or deals (existing client vs potential client)
        /// </summary>
        [JsonPropertyName("hasEquipment")]
        public bool HasEquipment { get; set; }
        
        /// <summary>
        /// Client category: "Potential" (no equipment/contracts) or "Existing" (has equipment/contracts)
        /// </summary>
        [JsonPropertyName("clientCategory")]
        public string ClientCategory { get; set; } = "Potential";
        
        /// <summary>
        /// Number of contracts the client has
        /// </summary>
        [JsonPropertyName("contractCount")]
        public int ContractCount { get; set; }
        
        /// <summary>
        /// Number of deals the client has
        /// </summary>
        [JsonPropertyName("dealCount")]
        public int DealCount { get; set; }
        
        /// <summary>
        /// Legacy customer ID from TBS system (indicates existing client with equipment)
        /// </summary>
        [JsonPropertyName("legacyCustomerId")]
        public int? LegacyCustomerId { get; set; }
    }

    public class SubmitClientLegalDocumentsDTO
    {
        [Required(ErrorMessage = "National ID is required")]
        [MaxLength(50)]
        public string NationalId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "National ID front image is required")]
        public IFormFile NationalIdFrontImage { get; set; } = null!;
        
        [Required(ErrorMessage = "National ID back image is required")]
        public IFormFile NationalIdBackImage { get; set; } = null!;
        
        [MaxLength(50)]
        public string? TaxCardNumber { get; set; }
        
        public IFormFile? TaxCardImage { get; set; }
    }
}
