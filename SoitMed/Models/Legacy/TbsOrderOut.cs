using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    [Table("Stk_Order_Out")]
    public class TbsOrderOut
    {
        [Key]
        [Column("OO_ID")]
        public int OoId { get; set; }

        [Column("SI_ID")]
        public long? SiId { get; set; }

        [Column("OO_Code")]
        [StringLength(10)]
        public string? OoCode { get; set; }

        [Column("OO_Date", TypeName = "datetime")]
        public DateTime? OoDate { get; set; }

        [Column("Cus_ID")]
        public int? CusId { get; set; }
    }
}

