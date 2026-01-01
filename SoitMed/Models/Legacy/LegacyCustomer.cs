using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    /// <summary>
    /// Legacy customer table from old SOIT system
    /// Maps to Stk_Customers table
    /// </summary>
    [Table("Stk_Customers")]
    public class LegacyCustomer
    {
        [Key]
        [Column("CusId")]
        public int CusId { get; set; }

        [Column("CusName")]
        [MaxLength(200)]
        public string? CusName { get; set; }

        [Column("CusMobile")]
        [MaxLength(20)]
        public string? CusMobile { get; set; }

        [Column("CusPhone")]
        [MaxLength(20)]
        public string? CusPhone { get; set; }

        [Column("CusEmail")]
        [MaxLength(100)]
        public string? CusEmail { get; set; }

        [Column("CusAddress")]
        [MaxLength(500)]
        public string? CusAddress { get; set; }

        [Column("CusCity")]
        [MaxLength(100)]
        public string? CusCity { get; set; }

        [Column("CusGovernorate")]
        [MaxLength(100)]
        public string? CusGovernorate { get; set; }

        [Column("CusNotes")]
        public string? CusNotes { get; set; }

        [Column("CusCreatedDate")]
        public DateTime? CusCreatedDate { get; set; }
    }
}

