using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    [Table("MNT_MaintenanceContract_Items")]
    public class TbsMaintenanceContractItem
    {
        [Key]
        [Column("Contract_ItemsId")]
        public int ContractItemsId { get; set; }

        public int ContractId { get; set; }

        [Column("SOI_ID")]
        public int? SoiId { get; set; }

        [Column("OOI_ID")]
        public int? OoiId { get; set; }
    }
}

