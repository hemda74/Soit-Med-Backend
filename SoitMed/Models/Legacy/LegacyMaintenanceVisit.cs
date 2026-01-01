using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    /// <summary>
    /// Legacy maintenance visiting table from old SOIT system
    /// Maps to MNT_Visiting table
    /// </summary>
    [Table("MNT_Visiting")]
    public class LegacyMaintenanceVisit
    {
        [Key]
        [Column("VisitingId")]
        public int VisitingId { get; set; }

        [Column("OoiId")]
        public int? OoiId { get; set; }

        [Column("CusId")]
        public int? CusId { get; set; }

        [Column("VisitingDate")]
        public DateTime? VisitingDate { get; set; }

        [Column("VisitingStatus")]
        [MaxLength(50)]
        public string? VisitingStatus { get; set; }

        [Column("VisitingNotes")]
        public string? VisitingNotes { get; set; }

        [Column("EngineerName")]
        [MaxLength(200)]
        public string? EngineerName { get; set; }

        [Column("ServiceFee", TypeName = "decimal(18,2)")]
        public decimal? ServiceFee { get; set; }

        [Column("ActionsTaken")]
        [MaxLength(1000)]
        public string? ActionsTaken { get; set; }

        [Column("PartsUsed")]
        [MaxLength(500)]
        public string? PartsUsed { get; set; }

        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }
    }
}

