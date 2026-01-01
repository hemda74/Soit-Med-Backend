using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    /// <summary>
    /// Legacy maintenance contract table from old SOIT system
    /// Maps to MNT_MaintenanceContract table
    /// </summary>
    [Table("MNT_MaintenanceContract")]
    public class LegacyMaintenanceContract
    {
        [Key]
        [Column("ContractId")]
        public int ContractId { get; set; }

        [Column("CusId")]
        public int? CusId { get; set; }

        [Column("OoiId")]
        public int? OoiId { get; set; }

        [Column("ContractNumber")]
        [MaxLength(100)]
        public string? ContractNumber { get; set; }

        [Column("StartDate")]
        public DateTime? StartDate { get; set; }

        [Column("EndDate")]
        public DateTime? EndDate { get; set; }

        [Column("MaintenanceFrequency")]
        [MaxLength(50)]
        public string? MaintenanceFrequency { get; set; }

        [Column("ContractValue", TypeName = "decimal(18,2)")]
        public decimal? ContractValue { get; set; }

        [Column("IsActive")]
        public bool? IsActive { get; set; }

        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }
    }
}

