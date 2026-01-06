using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    [Table("Stk_Order_Out_Items")]
    public class TbsOrderOutItem
    {
        [Key]
        [Column("OOI_ID")]
        public int OoiId { get; set; }

        [Column("OO_ID")]
        public int OoId { get; set; }

        [Column("item_ID")]
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }

        [Column("U_ID")]
        public int UId { get; set; }

        [Column("Item_DateExpire", TypeName = "datetime")]
        public DateTime? ItemDateExpire { get; set; }

        [Column("OOI_Main")]
        public int? OoiMain { get; set; }

        [StringLength(50)]
        public string? LotNum { get; set; }

        [StringLength(50)]
        public string? SerialNum { get; set; }

        [StringLength(150)]
        public string? DevicePlace { get; set; }

        [Column("IS_Returned")]
        public bool IsReturned { get; set; }

        public bool IsDeliveryForReplacement { get; set; }
    }
}

