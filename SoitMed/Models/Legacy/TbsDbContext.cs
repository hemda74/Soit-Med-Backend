using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Models.Legacy
{
    /// <summary>
    /// DbContext for reading from legacy TBS database
    /// Used for cross-database migration queries
    /// </summary>
    public class TbsDbContext : DbContext
    {
        public TbsDbContext(DbContextOptions<TbsDbContext> options) : base(options)
        {
        }

        // Legacy TBS tables (same structure as soitmed_data_backend)
        public DbSet<TbsMaintenanceContract> MntMaintenanceContracts { get; set; }
        public DbSet<TbsMaintenanceContractItem> MntMaintenanceContractItems { get; set; }
        public DbSet<TbsSalesContract> StkSalesContracts { get; set; }
        public DbSet<TbsCustomer> StkCustomers { get; set; }
        public DbSet<TbsSalesInvoice> StkSalesInvs { get; set; }
        public DbSet<TbsOrderOut> StkOrderOuts { get; set; }
        public DbSet<TbsOrderOutItem> StkOrderOutItems { get; set; }
        public DbSet<TbsItem> StkItems { get; set; }
        public DbSet<TbsVisiting> MntVisitings { get; set; }
        public DbSet<TbsVisitingReport> MntVisitingReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TBS schema (database name in connection string)
            modelBuilder.Entity<TbsMaintenanceContract>().ToTable("MNT_MaintenanceContract", "dbo");
            modelBuilder.Entity<TbsMaintenanceContractItem>().ToTable("MNT_MaintenanceContract_Items", "dbo");
            modelBuilder.Entity<TbsSalesContract>().ToTable("Stk_Sales_Contract", "dbo");
            modelBuilder.Entity<TbsCustomer>().ToTable("Stk_Customers", "dbo");
            modelBuilder.Entity<TbsSalesInvoice>().ToTable("Stk_Sales_Inv", "dbo");
            modelBuilder.Entity<TbsOrderOut>().ToTable("Stk_Order_Out", "dbo");
            modelBuilder.Entity<TbsOrderOutItem>().ToTable("Stk_Order_Out_Items", "dbo");
            modelBuilder.Entity<TbsItem>().ToTable("Stk_Items", "dbo");
            modelBuilder.Entity<TbsVisiting>().ToTable("MNT_Visiting", "dbo");
            modelBuilder.Entity<TbsVisitingReport>().ToTable("MNT_VisitingReport", "dbo");
        }
    }

    /// <summary>
    /// Legacy TBS Maintenance Contract (full structure from soitmed_data_backend)
    /// </summary>
    [Table("MNT_MaintenanceContract")]
    public class TbsMaintenanceContract
    {
        [Key]
        [Column("ContractId")]
        public int ContractId { get; set; }

        [Column("Cus_ID")]
        public int CusId { get; set; }

        [Column("ClasserNumber")]
        [MaxLength(50)]
        public string ClasserNumber { get; set; } = string.Empty;

        [Column("ContractTotalValue", TypeName = "decimal(18,3)")]
        public decimal ContractTotalValue { get; set; }

        [Column("StartDate", TypeName = "datetime")]
        public DateTime StartDate { get; set; }

        [Column("EndDate", TypeName = "datetime")]
        public DateTime EndDate { get; set; }

        [Column("ContractCode")]
        public int? ContractCode { get; set; }

        [Column("BillNumber")]
        public int? BillNumber { get; set; }

        [Column("SO_ID")]
        public int? SoId { get; set; }

        [Column("Notes_Tech")]
        public string? NotesTech { get; set; }

        [Column("Notes_Finance")]
        public string? NotesFinance { get; set; }

        [Column("Notes_Admin")]
        public string? NotesAdmin { get; set; }

        [Column("SC_File")] // Sales contract file path (legacy)
        [MaxLength(255)]
        public string? ScFile { get; set; }

        // Payment plan fields (if exist in TBS)
        [Column("InstallmentMonths")]
        public int? InstallmentMonths { get; set; }

        [Column("InstallmentAmount", TypeName = "decimal(18,3)")]
        public decimal? InstallmentAmount { get; set; }

        [Column("MainContract")]
        public int? MainContract { get; set; }

        [Column("Contract_Root_Id")]
        public int? ContractRootId { get; set; }
    }

    /// <summary>
    /// Legacy TBS Sales Contract
    /// </summary>
    [Table("Stk_Sales_Contract")]
    public class TbsSalesContract
    {
        [Key]
        [Column("SC_ID")]
        public int ScId { get; set; }

        [Column("SC_Name")]
        [MaxLength(255)]
        public string ScName { get; set; } = string.Empty;

        [Column("Cus_ID")]
        public int CusId { get; set; }

        [Column("SC_StartDate", TypeName = "datetime")]
        public DateTime ScStartDate { get; set; }

        [Column("SC_EndDate", TypeName = "datetime")]
        public DateTime ScEndDate { get; set; }

        [Column("SC_File")]
        [MaxLength(255)]
        public string? ScFile { get; set; } // Legacy file path
    }

    /// <summary>
    /// Legacy TBS Customer
    /// </summary>
    [Table("Stk_Customers")]
    public class TbsCustomer
    {
        [Key]
        [Column("Cus_ID")]
        public int CusId { get; set; }

        [Column("Cus_Name")]
        [MaxLength(250)]
        public string? CusName { get; set; }

        [Column("Cus_Tel")]
        [MaxLength(20)]
        public string? CusTel { get; set; }

        [Column("Cus_Mobile")]
        [MaxLength(20)]
        public string? CusMobile { get; set; }

        [Column("Cus_address")]
        [MaxLength(300)]
        public string? CusAddress { get; set; }

        [Column("Cus_Email")]
        [MaxLength(100)]
        public string? CusEmail { get; set; }
    }

    /// <summary>
    /// Legacy TBS Sales Invoice (for payment/installment data)
    /// </summary>
    [Table("Stk_Sales_Inv")]
    public class TbsSalesInvoice
    {
        [Key]
        [Column("SI_ID")]
        public int SiId { get; set; }

        [Column("SC_ID")]
        public int? ScId { get; set; }

        [Column("ContractId")]
        public int? ContractId { get; set; }

        [Column("Amount", TypeName = "decimal(18,3)")]
        public decimal? Amount { get; set; }

        [Column("PaidAmount", TypeName = "decimal(18,3)")]
        public decimal? PaidAmount { get; set; }

        [Column("DueDate", TypeName = "datetime")]
        public DateTime? DueDate { get; set; }

        [Column("PaymentDate", TypeName = "datetime")]
        public DateTime? PaymentDate { get; set; }
    }
}

