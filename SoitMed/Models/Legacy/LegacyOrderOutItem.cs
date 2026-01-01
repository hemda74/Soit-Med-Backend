using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    /// <summary>
    /// Legacy order out items table from old SOIT system
    /// Maps to Stk_Order_Out_Items table
    /// </summary>
    [Table("Stk_Order_Out_Items")]
    public class LegacyOrderOutItem
    {
        [Key]
        [Column("OoiId")]
        public int OoiId { get; set; }

        [Column("CusId")]
        public int? CusId { get; set; }

        [Column("SerialNum")]
        [MaxLength(100)]
        public string? SerialNum { get; set; }

        [Column("ItemName")]
        [MaxLength(200)]
        public string? ItemName { get; set; }

        [Column("ItemModel")]
        [MaxLength(100)]
        public string? ItemModel { get; set; }

        [Column("ItemManufacturer")]
        [MaxLength(100)]
        public string? ItemManufacturer { get; set; }

        [Column("PurchaseDate")]
        public DateTime? PurchaseDate { get; set; }

        [Column("WarrantyExpiry")]
        public DateTime? WarrantyExpiry { get; set; }

        [Column("ItemDescription")]
        [MaxLength(500)]
        public string? ItemDescription { get; set; }

        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }
    }
}

