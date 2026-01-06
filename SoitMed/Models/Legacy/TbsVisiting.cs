using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    [Table("MNT_Visiting")]
    public class TbsVisiting
    {
        [Key]
        public int VisitingId { get; set; }

        public int VisitingTypeId { get; set; }

        public int? EmpCode { get; set; }

        [Column("TO_ID")]
        public int? ToId { get; set; }

        public int? ContractId { get; set; }

        public int? MainVisiting { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? VisitingDate { get; set; }

        [Column("Cus_ID")]
        public int CusId { get; set; }

        public string? Notes { get; set; }

        [Column("deliverGLDate", TypeName = "datetime")]
        public DateTime? DeliverGldate { get; set; }

        [Column("SO_ID")]
        public int? SoId { get; set; }

        public int SourceId { get; set; }

        public bool? InGuarantee { get; set; }

        public bool? HasSpareParts { get; set; }

        public bool? HasBill { get; set; }

        [Column("IS_Manual")]
        public bool? IsManual { get; set; }

        [Column("IS_Cancelled")]
        public bool? IsCancelled { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? VisitingValue { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? BillValue { get; set; }

        [StringLength(500)]
        public string? DefectDescription { get; set; }

        [StringLength(500)]
        public string? DevicePlace { get; set; }

        public int? ComplaintId { get; set; }

        public int? PriceListId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? BillDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? VisitingDateDefault { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? VisitingDateIdeal { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? TaxRatio { get; set; }

        public int? ManualId { get; set; }

        [StringLength(500)]
        public string? Files { get; set; }
    }
}

