using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Legacy
{
    /// <summary>
    /// Legacy employee table from old SOIT system (TBS)
    /// Maps to EmpMas table
    /// Used for mapping engineers from legacy system
    /// </summary>
    [Table("LegacyEmployees")]
    public class LegacyEmployee
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// EmpCode from TBS.EmpMas - unique identifier from legacy system
        /// </summary>
        [Required]
        [Column("LegacyEmployeeId")]
        public int LegacyEmployeeId { get; set; }

        /// <summary>
        /// Employee name in Arabic (EmpNma from TBS)
        /// </summary>
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Employee name in English (EmpNmaEn from TBS)
        /// </summary>
        [MaxLength(200)]
        public string? NameEn { get; set; }

        /// <summary>
        /// Department code from TBS (DepartMentCode)
        /// </summary>
        public int? DepartmentCode { get; set; }

        /// <summary>
        /// Department name from DepartmentMas
        /// </summary>
        [MaxLength(200)]
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Job code from TBS (JobCode)
        /// </summary>
        public int? JobCode { get; set; }

        /// <summary>
        /// Job name from JobMas
        /// </summary>
        [MaxLength(200)]
        public string? JobName { get; set; }

        /// <summary>
        /// Phone number (TelPhone from TBS)
        /// </summary>
        [MaxLength(50)]
        public string? Phone { get; set; }

        /// <summary>
        /// Mobile number (TelMob from TBS)
        /// </summary>
        [MaxLength(50)]
        public string? Mobile { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Active status (Valid == 1 from TBS)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
