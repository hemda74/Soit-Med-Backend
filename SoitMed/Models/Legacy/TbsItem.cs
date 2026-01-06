using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    [Table("Stk_Items")]
    public class TbsItem
    {
        [Key]
        [Column("Item_ID")]
        public int ItemId { get; set; }

        [Column("Item_Name_Ar")]
        [StringLength(200)]
        public string ItemNameAr { get; set; } = null!;

        [Column("Item_Name_En")]
        [StringLength(200)]
        public string? ItemNameEn { get; set; }

        [Column("item_Code")]
        [StringLength(255)]
        public string? ItemCode { get; set; }
    }
}

