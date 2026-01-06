using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    [Table("MNT_VisitingReport")]
    public class TbsVisitingReport
    {
        [Key]
        public int VisitingReportId { get; set; }

        public int VisitingId { get; set; }

        [Column("OOI_ID")]
        public int OoiId { get; set; }

        public string? ReportDecription { get; set; }

        [StringLength(50)]
        public string? FromTime { get; set; }

        [StringLength(50)]
        public string? ToTime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime ReportDate { get; set; }

        public int? VisitingResultId { get; set; }

        public bool? IsApproved { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ReportApprovedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? VisitingDateEffective { get; set; }

        [StringLength(500)]
        public string? Files { get; set; }

        public int? RepVisitStatusId { get; set; }

        public int? RecptRepVisitTypeId { get; set; }
    }
}

