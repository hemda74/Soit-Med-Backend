using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using SoitMed.Models.Equipment;
using SoitMed.Models.Payment;
using SoitMed.Models.Legacy;
using SoitMed.Models.Contract;

namespace SoitMed.Models
{
    public class Context : IdentityDbContext<ApplicationUser>
    {
        // Core entities
        public DbSet<Department> Departments { get; set; }
        public DbSet<Role> BusinessRoles { get; set; }

        // Hospital entities
        public DbSet<Hospital.Hospital> Hospitals { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<DoctorHospital> DoctorHospitals { get; set; }

        // Location entities
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Engineer> Engineers { get; set; }
        public DbSet<EngineerGovernorate> EngineerGovernorates { get; set; }

        // Equipment entities
        public DbSet<Equipment.Equipment> Equipment { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }
        
        // Maintenance entities
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<MaintenanceVisit> MaintenanceVisits { get; set; }
        public DbSet<MaintenanceRequestAttachment> MaintenanceRequestAttachments { get; set; }
        public DbSet<SparePartRequest> SparePartRequests { get; set; }
        public DbSet<MaintenanceRequestRating> MaintenanceRequestRatings { get; set; }
        
        // Payment entities
        public DbSet<Payment.Invoice> Invoices { get; set; }
        public DbSet<VisitAssignees> VisitAssignees { get; set; }
        public DbSet<VisitReport> VisitReports { get; set; }
        
        // Audit entities
        public DbSet<Core.EntityChangeLog> EntityChangeLogs { get; set; }
        
        // Payment entities
        public DbSet<Payment.Payment> Payments { get; set; }
        public DbSet<Payment.PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Payment.PaymentGatewayConfig> PaymentGatewayConfigs { get; set; }

        // User image entities
        public DbSet<UserImage> UserImages { get; set; }

        // Sales funnel entities
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Offer> Offers { get; set; }

        // Workflow and notification entities
        public DbSet<RequestWorkflow> RequestWorkflows { get; set; }
        public DbSet<DeliveryTerms> DeliveryTerms { get; set; }
        public DbSet<PaymentTerms> PaymentTerms { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }

        // Client tracking entities
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientVisit> ClientVisits { get; set; }
        public DbSet<ClientInteraction> ClientInteractions { get; set; }
        public DbSet<ClientAnalytics> ClientAnalytics { get; set; }

        // Weekly planning entities
        public DbSet<WeeklyPlan> WeeklyPlans { get; set; }
        public DbSet<WeeklyPlanTask> WeeklyPlanTasks { get; set; }
        public DbSet<TaskProgress> TaskProgresses { get; set; }
        public DbSet<DailyProgress> DailyProgresses { get; set; }

        // Chat entities
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        // Sales workflow entities
        public DbSet<OfferRequest> OfferRequests { get; set; }
        public DbSet<SalesOffer> SalesOffers { get; set; }
        public DbSet<SalesDeal> SalesDeals { get; set; }
        
        // Enhanced offer entities
        public DbSet<OfferEquipment> OfferEquipment { get; set; }
        public DbSet<OfferTerms> OfferTerms { get; set; }
        public DbSet<InstallmentPlan> InstallmentPlans { get; set; }
        public DbSet<RecentOfferActivity> RecentOfferActivities { get; set; }
        
        // SalesMan targets and statistics
        public DbSet<SalesManTarget> SalesManTargets { get; set; }
        
        // Products catalog
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        
        // Contract entities
        public DbSet<Contract.Contract> Contracts { get; set; }
        public DbSet<Contract.ContractNegotiation> ContractNegotiations { get; set; }
        public DbSet<Contract.InstallmentSchedule> InstallmentSchedules { get; set; }

        // Legacy tables (from old SOIT system - same database)
        public DbSet<Legacy.LegacyCustomer> LegacyCustomers { get; set; }
        public DbSet<Legacy.LegacyOrderOutItem> LegacyOrderOutItems { get; set; }
        public DbSet<Legacy.LegacyMaintenanceVisit> LegacyMaintenanceVisits { get; set; }
        public DbSet<Legacy.LegacyMaintenanceContract> LegacyMaintenanceContracts { get; set; }
        public DbSet<Legacy.LegacyEmployee> LegacyEmployees { get; set; }
        
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Department-User relationship
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Department entity
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // Configure Doctor-Hospital many-to-many relationship
            modelBuilder.Entity<DoctorHospital>()
                .HasOne(dh => dh.Doctor)
                .WithMany(d => d.DoctorHospitals)
                .HasForeignKey(dh => dh.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorHospital>()
                .HasOne(dh => dh.Hospital)
                .WithMany(h => h.DoctorHospitals)
                .HasForeignKey(dh => dh.HospitalId)
                .OnDelete(DeleteBehavior.NoAction);

            // Ensure unique combination of Doctor-Hospital
            modelBuilder.Entity<DoctorHospital>()
                .HasIndex(dh => new { dh.DoctorId, dh.HospitalId })
                .IsUnique();

            modelBuilder.Entity<Technician>()
                .HasOne(t => t.Hospital)
                .WithMany(h => h.Technicians)
                .HasForeignKey(t => t.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Hospital entity
            modelBuilder.Entity<Hospital.Hospital>()
                .HasIndex(h => h.HospitalId)
                .IsUnique();

            // Configure Engineer-Governorate many-to-many relationship
            modelBuilder.Entity<EngineerGovernorate>()
                .HasOne(eg => eg.Engineer)
                .WithMany(e => e.EngineerGovernorates)
                .HasForeignKey(eg => eg.EngineerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EngineerGovernorate>()
                .HasOne(eg => eg.Governorate)
                .WithMany(g => g.EngineerGovernorates)
                .HasForeignKey(eg => eg.GovernorateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique combination of Engineer-Governorate
            modelBuilder.Entity<EngineerGovernorate>()
                .HasIndex(eg => new { eg.EngineerId, eg.GovernorateId })
                .IsUnique();

            // Configure unique indexes
            modelBuilder.Entity<Governorate>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Configure optional User relationships
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Technician>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Engineer>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Equipment relationships
            modelBuilder.Entity<Equipment.Equipment>()
                .HasOne(e => e.Hospital)
                .WithMany(h => h.Equipment)
                .HasForeignKey(e => e.HospitalId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Equipment.Equipment>()
                .HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ensure Equipment has either HospitalId OR CustomerId, not both
            modelBuilder.Entity<Equipment.Equipment>()
                .ToTable(t => t.HasCheckConstraint("CK_Equipment_Owner",
                    "(HospitalId IS NOT NULL AND CustomerId IS NULL) OR (HospitalId IS NULL AND CustomerId IS NOT NULL)"));

            // Ensure unique QR codes
            modelBuilder.Entity<Equipment.Equipment>()
                .HasIndex(e => e.QRCode)
                .IsUnique();

            // Ensure unique QrToken
            modelBuilder.Entity<Equipment.Equipment>()
                .HasIndex(e => e.QrToken)
                .IsUnique();

            // Configure Client relationships
            modelBuilder.Entity<Client>()
                .HasOne(c => c.RelatedUser)
                .WithMany()
                .HasForeignKey(c => c.RelatedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure RepairRequest relationships
            modelBuilder.Entity<RepairRequest>()
                .HasOne(rr => rr.Equipment)
                .WithMany(e => e.RepairRequests)
                .HasForeignKey(rr => rr.EquipmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RepairRequest>()
                .HasOne(rr => rr.RequestingDoctor)
                .WithMany(d => d.RepairRequests)
                .HasForeignKey(rr => rr.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RepairRequest>()
                .HasOne(rr => rr.RequestingTechnician)
                .WithMany(t => t.RepairRequests)
                .HasForeignKey(rr => rr.TechnicianId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RepairRequest>()
                .HasOne(rr => rr.AssignedEngineer)
                .WithMany(e => e.AssignedRepairRequests)
                .HasForeignKey(rr => rr.AssignedEngineerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure decimal precision for RepairCost
            modelBuilder.Entity<RepairRequest>()
                .Property(rr => rr.RepairCost)
                .HasPrecision(18, 2);

            // Ensure either Doctor or Technician is specified (not both)
            modelBuilder.Entity<RepairRequest>()
                .ToTable(t => t.HasCheckConstraint("CK_RepairRequest_Requestor", 
                    "(DoctorId IS NOT NULL AND TechnicianId IS NULL) OR (DoctorId IS NULL AND TechnicianId IS NOT NULL)"));

            // Configure UserImage relationships
            modelBuilder.Entity<UserImage>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserImages)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure unique constraint for profile image per user
            modelBuilder.Entity<UserImage>()
                .HasIndex(ui => new { ui.UserId, ui.IsProfileImage })
                .HasFilter("[IsProfileImage] = 1")
                .IsUnique();

            // Configure Sales Funnel entities
            // Note: ActivityLog relationships with Deal and Offer are handled in their respective models

            // Configure RequestWorkflow relationships
            modelBuilder.Entity<RequestWorkflow>()
                .HasOne(rw => rw.ActivityLog)
                .WithMany()
                .HasForeignKey(rw => rw.ActivityLogId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RequestWorkflow>()
                .HasOne(rw => rw.Offer)
                .WithMany()
                .HasForeignKey(rw => rw.OfferId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RequestWorkflow>()
                .HasOne(rw => rw.Deal)
                .WithMany()
                .HasForeignKey(rw => rw.DealId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RequestWorkflow>()
                .HasOne(rw => rw.DeliveryTerms)
                .WithMany()
                .HasForeignKey(rw => rw.DeliveryTermsId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RequestWorkflow>()
                .HasOne(rw => rw.PaymentTerms)
                .WithMany()
                .HasForeignKey(rw => rw.PaymentTermsId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.RequestWorkflow)
                .WithMany()
                .HasForeignKey(n => n.RequestWorkflowId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.ActivityLog)
                .WithMany()
                .HasForeignKey(n => n.ActivityLogId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure DeviceToken relationships
            modelBuilder.Entity<DeviceToken>()
                .HasOne(dt => dt.User)
                .WithMany()
                .HasForeignKey(dt => dt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for efficient queries
            modelBuilder.Entity<DeviceToken>()
                .HasIndex(dt => new { dt.UserId, dt.IsActive });

            // Configure indexes for better performance
            modelBuilder.Entity<RequestWorkflow>()
                .HasIndex(rw => new { rw.FromUserId, rw.Status });

            modelBuilder.Entity<RequestWorkflow>()
                .HasIndex(rw => new { rw.ToUserId, rw.Status });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.CreatedAt);

            // ========== NEW SALES MODULE RELATIONSHIPS ==========
            
            // WeeklyPlan relationships
            modelBuilder.Entity<WeeklyPlan>()
                .HasOne(wp => wp.Employee)
                .WithMany()
                .HasForeignKey(wp => wp.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // WeeklyPlanTask relationships
            modelBuilder.Entity<WeeklyPlanTask>()
                .HasOne(wpt => wpt.WeeklyPlan)
                .WithMany(wp => wp.Tasks)
                .HasForeignKey(wpt => wpt.WeeklyPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WeeklyPlanTask>()
                .HasOne(wpt => wpt.Client)
                .WithMany()
                .HasForeignKey(wpt => wpt.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            // TaskProgress - Explicit table and column mapping
            modelBuilder.Entity<TaskProgress>()
                .ToTable("TaskProgresses");
            
            modelBuilder.Entity<TaskProgress>()
                .Property(tp => tp.TaskId)
                .HasColumnName("TaskId");
            
            // TaskProgress relationships
            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.Task)
                .WithMany(wpt => wpt.Progresses)
                .HasForeignKey(tp => tp.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.Client)
                .WithMany(c => c.TaskProgresses)
                .HasForeignKey(tp => tp.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.Employee)
                .WithMany()
                .HasForeignKey(tp => tp.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // OfferRequest relationships
            modelBuilder.Entity<OfferRequest>()
                .HasOne(or => or.Requester)
                .WithMany()
                .HasForeignKey(or => or.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OfferRequest>()
                .HasOne(or => or.Client)
                .WithMany()
                .HasForeignKey(or => or.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure OfferRequest -> TaskProgress relationship (one-way: OfferRequest references TaskProgress)
            modelBuilder.Entity<OfferRequest>()
                .HasOne(or => or.TaskProgress)
                .WithMany()
                .HasForeignKey(or => or.TaskProgressId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Ignore the TaskProgress.OfferRequest navigation to avoid shadow property
            modelBuilder.Entity<TaskProgress>()
                .Ignore(tp => tp.OfferRequest);
            
            modelBuilder.Entity<OfferRequest>()
                .HasOne<ApplicationUser>("AssignedSupportUser")
                .WithMany()
                .HasForeignKey("AssignedTo")
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes for performance
            modelBuilder.Entity<OfferRequest>()
                .HasIndex(or => new { or.Status, or.RequestedBy });

            modelBuilder.Entity<TaskProgress>()
                .HasIndex(tp => new { tp.ClientId, tp.ProgressDate });

            modelBuilder.Entity<WeeklyPlanTask>()
                .HasIndex(wpt => new { wpt.WeeklyPlanId, wpt.PlannedDate });

            // ==================== Sales Module Relationships ====================
            
            // SalesOffer relationships
            modelBuilder.Entity<OfferRequest>()
                .HasOne(or => or.CreatedOffer)
                .WithOne(so => so.OfferRequest)
                .HasForeignKey<OfferRequest>(or => or.CreatedOfferId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesOffer>()
                .HasOne(so => so.Client)
                .WithMany()
                .HasForeignKey(so => so.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOffer>()
                .HasOne(so => so.Creator)
                .WithMany()
                .HasForeignKey(so => so.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOffer>()
                .HasOne(so => so.SalesMan)
                .WithMany()
                .HasForeignKey(so => so.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);

            // SalesDeal relationships - Configure as optional to handle orphaned deals
            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.Offer)
                .WithOne(so => so.Deal)
                .HasForeignKey<SalesDeal>(sd => sd.OfferId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.Client)
                .WithMany()
                .HasForeignKey(sd => sd.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.SalesMan)
                .WithMany()
                .HasForeignKey(sd => sd.SalesManId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.ManagerApprover)
                .WithMany()
                .HasForeignKey(sd => sd.ManagerApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.SuperAdminApprover)
                .WithMany()
                .HasForeignKey(sd => sd.SuperAdminApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // RecentOfferActivity relationships
            modelBuilder.Entity<RecentOfferActivity>()
                .HasOne(roa => roa.Offer)
                .WithMany()
                .HasForeignKey(roa => roa.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<SalesOffer>()
                .HasIndex(so => new { so.Status, so.AssignedTo });

            modelBuilder.Entity<RecentOfferActivity>()
                .HasIndex(roa => roa.ActivityTimestamp);

            modelBuilder.Entity<SalesDeal>()
                .HasIndex(sd => new { sd.Status, sd.SalesManId });

            modelBuilder.Entity<SalesDeal>()
                .HasIndex(sd => new { sd.Status, sd.ManagerApprovedBy });

            modelBuilder.Entity<SalesDeal>()
                .HasIndex(sd => new { sd.Status, sd.SuperAdminApprovedBy });

            // Enhanced offer relationships - Configure table names to match database
            modelBuilder.Entity<OfferEquipment>()
                .ToTable("OfferEquipment")
                .HasOne(oe => oe.Offer)
                .WithMany(so => so.Equipment)
                .HasForeignKey(oe => oe.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OfferTerms>()
                .ToTable("OfferTerms")
                .HasOne(ot => ot.Offer)
                .WithOne(so => so.Terms)
                .HasForeignKey<OfferTerms>(ot => ot.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InstallmentPlan>()
                .ToTable("InstallmentPlan")
                .HasOne(ip => ip.Offer)
                .WithMany(so => so.InstallmentPlans)
                .HasForeignKey(ip => ip.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            // SalesManTarget relationships
            modelBuilder.Entity<SalesManTarget>()
                .HasOne(st => st.Manager)
                .WithMany()
                .HasForeignKey(st => st.CreatedByManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesManTarget>()
                .HasOne(st => st.SalesMan)
                .WithMany()
                .HasForeignKey(st => st.SalesManId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for efficient queries
            modelBuilder.Entity<SalesManTarget>()
                .HasIndex(st => new { st.SalesManId, st.Year, st.Quarter })
                .IsUnique();

            // Product catalog indexes for efficient searching
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.Category, p.IsActive, p.InStock });
            
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.IsActive);
            
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.Name, p.Model, p.Provider });

            // ========== MAINTENANCE MODULE RELATIONSHIPS ==========
            
            // MaintenanceRequest relationships
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.Customer)
                .WithMany()
                .HasForeignKey(mr => mr.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.Hospital)
                .WithMany()
                .HasForeignKey(mr => mr.HospitalId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.Equipment)
                .WithMany()
                .HasForeignKey(mr => mr.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.AssignedToEngineer)
                .WithMany()
                .HasForeignKey(mr => mr.AssignedToEngineerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.AssignedByMaintenanceSupport)
                .WithMany()
                .HasForeignKey(mr => mr.AssignedByMaintenanceSupportId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure decimal precision for payment amounts
            modelBuilder.Entity<MaintenanceRequest>()
                .Property(mr => mr.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceRequest>()
                .Property(mr => mr.PaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceRequest>()
                .Property(mr => mr.RemainingAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceRequest>()
                .Property(mr => mr.LaborFees)
                .HasPrecision(18, 2);

            // MaintenanceRequest future-proofing relationships
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.CollectionDelegate)
                .WithMany()
                .HasForeignKey(mr => mr.CollectionDelegateId)
                .OnDelete(DeleteBehavior.NoAction);

            // MaintenanceRequestAttachment relationships
            modelBuilder.Entity<MaintenanceRequestAttachment>()
                .HasOne(mra => mra.MaintenanceRequest)
                .WithMany(mr => mr.Attachments)
                .HasForeignKey(mra => mra.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceRequestAttachment>()
                .HasOne(mra => mra.UploadedBy)
                .WithMany()
                .HasForeignKey(mra => mra.UploadedById)
                .OnDelete(DeleteBehavior.SetNull);

            // MaintenanceVisit relationships
            modelBuilder.Entity<MaintenanceVisit>()
                .HasOne(mv => mv.MaintenanceRequest)
                .WithMany(mr => mr.Visits)
                .HasForeignKey(mv => mv.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceVisit>()
                .HasOne(mv => mv.Customer)
                .WithMany()
                .HasForeignKey(mv => mv.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceVisit>()
                .HasOne(mv => mv.Device)
                .WithMany()
                .HasForeignKey(mv => mv.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceVisit>()
                .HasOne(mv => mv.Engineer)
                .WithMany()
                .HasForeignKey(mv => mv.EngineerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship for rescheduled visits
            modelBuilder.Entity<MaintenanceVisit>()
                .HasOne(mv => mv.ParentVisit)
                .WithMany(mv => mv.ChildVisits)
                .HasForeignKey(mv => mv.ParentVisitId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MaintenanceVisit>()
                .HasOne(mv => mv.SparePartRequest)
                .WithOne(spr => spr.MaintenanceVisit)
                .HasForeignKey<MaintenanceVisit>(mv => mv.SparePartRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-one relationship with VisitReport
            modelBuilder.Entity<MaintenanceVisit>()
                .HasOne(mv => mv.VisitReport)
                .WithOne(vr => vr.Visit)
                .HasForeignKey<VisitReport>(vr => vr.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision
            modelBuilder.Entity<MaintenanceVisit>()
                .Property(mv => mv.ServiceFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceVisit>()
                .Property(mv => mv.Cost)
                .HasPrecision(18, 2);

            // Indexes for MaintenanceVisit
            modelBuilder.Entity<MaintenanceVisit>()
                .HasIndex(mv => mv.TicketNumber)
                .IsUnique();

            modelBuilder.Entity<MaintenanceVisit>()
                .HasIndex(mv => mv.Status);

            modelBuilder.Entity<MaintenanceVisit>()
                .HasIndex(mv => mv.ScheduledDate);

            modelBuilder.Entity<MaintenanceVisit>()
                .HasIndex(mv => new { mv.CustomerId, mv.Status });

            modelBuilder.Entity<MaintenanceVisit>()
                .HasIndex(mv => new { mv.EngineerId, mv.Status });

            // VisitAssignees relationships (many-to-many)
            modelBuilder.Entity<VisitAssignees>()
                .HasKey(va => new { va.VisitId, va.EngineerId });

            modelBuilder.Entity<VisitAssignees>()
                .HasOne(va => va.Visit)
                .WithMany(mv => mv.Assignees)
                .HasForeignKey(va => va.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitAssignees>()
                .HasOne(va => va.Engineer)
                .WithMany()
                .HasForeignKey(va => va.EngineerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitAssignees>()
                .HasOne(va => va.AssignedBy)
                .WithMany()
                .HasForeignKey(va => va.AssignedById)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes for VisitAssignees
            modelBuilder.Entity<VisitAssignees>()
                .HasIndex(va => va.VisitId);

            modelBuilder.Entity<VisitAssignees>()
                .HasIndex(va => va.EngineerId);

            // VisitReport relationships (one-to-one)
            modelBuilder.Entity<VisitReport>()
                .HasIndex(vr => vr.VisitId)
                .IsUnique();

            // EntityChangeLog relationships
            modelBuilder.Entity<Core.EntityChangeLog>()
                .HasOne(ecl => ecl.User)
                .WithMany()
                .HasForeignKey(ecl => ecl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for EntityChangeLog
            modelBuilder.Entity<Core.EntityChangeLog>()
                .HasIndex(ecl => new { ecl.EntityName, ecl.EntityId });

            modelBuilder.Entity<Core.EntityChangeLog>()
                .HasIndex(ecl => ecl.UserId);

            modelBuilder.Entity<Core.EntityChangeLog>()
                .HasIndex(ecl => ecl.ChangeDate);

            // SparePartRequest relationships
            modelBuilder.Entity<SparePartRequest>()
                .HasOne(spr => spr.MaintenanceRequest)
                .WithMany()
                .HasForeignKey(spr => spr.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SparePartRequest>()
                .HasOne(spr => spr.ApprovedByWarehouseKeeper)
                .WithMany()
                .HasForeignKey(spr => spr.ApprovedByWarehouseKeeperId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SparePartRequest>()
                .HasOne(spr => spr.Visit)
                .WithMany()
                .HasForeignKey(spr => spr.VisitId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SparePartRequest>()
                .HasOne(spr => spr.AssignedToCoordinator)
                .WithMany()
                .HasForeignKey(spr => spr.AssignedToCoordinatorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SparePartRequest>()
                .HasOne(spr => spr.AssignedToInventoryManager)
                .WithMany()
                .HasForeignKey(spr => spr.AssignedToInventoryManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SparePartRequest>()
                .HasOne(spr => spr.PriceSetByManager)
                .WithMany()
                .HasForeignKey(spr => spr.PriceSetByManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure decimal precision for spare part prices
            modelBuilder.Entity<SparePartRequest>()
                .Property(spr => spr.OriginalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SparePartRequest>()
                .Property(spr => spr.CompanyPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SparePartRequest>()
                .Property(spr => spr.CustomerPrice)
                .HasPrecision(18, 2);

            // MaintenanceRequestRating relationships
            modelBuilder.Entity<MaintenanceRequestRating>()
                .HasOne(mrr => mrr.MaintenanceRequest)
                .WithMany(mr => mr.Ratings)
                .HasForeignKey(mrr => mrr.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceRequestRating>()
                .HasOne(mrr => mrr.Customer)
                .WithMany()
                .HasForeignKey(mrr => mrr.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequestRating>()
                .HasOne(mrr => mrr.Engineer)
                .WithMany()
                .HasForeignKey(mrr => mrr.EngineerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== PAYMENT MODULE RELATIONSHIPS ==========
            
            // Payment relationships
            modelBuilder.Entity<Payment.Payment>()
                .HasOne(p => p.MaintenanceRequest)
                .WithMany(mr => mr.Payments)
                .HasForeignKey(p => p.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Payment.Payment>()
                .HasOne(p => p.SparePartRequest)
                .WithMany(spr => spr.Payments)
                .HasForeignKey(p => p.SparePartRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment.Payment>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment.Payment>()
                .HasOne(p => p.ProcessedByAccountant)
                .WithMany()
                .HasForeignKey(p => p.ProcessedByAccountantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure decimal precision for payment amount
            modelBuilder.Entity<Payment.Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // PaymentTransaction relationships
            modelBuilder.Entity<Payment.PaymentTransaction>()
                .HasOne(pt => pt.Payment)
                .WithMany(p => p.Transactions)
                .HasForeignKey(pt => pt.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment.PaymentTransaction>()
                .HasOne(pt => pt.Visit)
                .WithMany()
                .HasForeignKey(pt => pt.VisitId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment.PaymentTransaction>()
                .HasOne(pt => pt.CollectionDelegate)
                .WithMany()
                .HasForeignKey(pt => pt.CollectionDelegateId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment.PaymentTransaction>()
                .HasOne(pt => pt.AccountsApprover)
                .WithMany()
                .HasForeignKey(pt => pt.AccountsApproverId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure decimal precision for transaction amount
            modelBuilder.Entity<Payment.PaymentTransaction>()
                .Property(pt => pt.Amount)
                .HasPrecision(18, 2);

            // Invoice relationships
            modelBuilder.Entity<Payment.Invoice>()
                .HasOne(i => i.MaintenanceRequest)
                .WithMany()
                .HasForeignKey(i => i.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment.Invoice>()
                .HasOne(i => i.CollectionDelegate)
                .WithMany()
                .HasForeignKey(i => i.CollectionDelegateId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure decimal precision for invoice amounts
            modelBuilder.Entity<Payment.Invoice>()
                .Property(i => i.LaborFees)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment.Invoice>()
                .Property(i => i.SparePartsTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment.Invoice>()
                .Property(i => i.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment.Invoice>()
                .Property(i => i.PaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment.Invoice>()
                .Property(i => i.RemainingAmount)
                .HasPrecision(18, 2);

            // Unique index on InvoiceNumber
            modelBuilder.Entity<Payment.Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            // Indexes for Invoice
            modelBuilder.Entity<Payment.Invoice>()
                .HasIndex(i => new { i.MaintenanceRequestId, i.Status });

            modelBuilder.Entity<Payment.Invoice>()
                .HasIndex(i => new { i.Status, i.CreatedAt });

            // Indexes for performance
            modelBuilder.Entity<MaintenanceRequest>()
                .HasIndex(mr => new { mr.CustomerId, mr.Status });

            modelBuilder.Entity<MaintenanceRequest>()
                .HasIndex(mr => new { mr.AssignedToEngineerId, mr.Status });

            modelBuilder.Entity<Payment.Payment>()
                .HasIndex(p => new { p.CustomerId, p.Status });

            modelBuilder.Entity<Payment.Payment>()
                .HasIndex(p => new { p.Status, p.CreatedAt });

            // ProductCategory relationships
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.ParentCategory)
                .WithMany(pc => pc.SubCategories)
                .HasForeignKey(pc => pc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductCategory>()
                .HasIndex(pc => pc.ParentCategoryId);

            modelBuilder.Entity<ProductCategory>()
                .HasIndex(pc => new { pc.IsActive, pc.DisplayOrder });

            // Product-Category relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.ProductCategory)
                .WithMany(pc => pc.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId);

            // Contract relationships
            modelBuilder.Entity<Contract.Contract>()
                .HasOne(c => c.Deal)
                .WithMany()
                .HasForeignKey(c => c.DealId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Contract.Contract>()
                .HasOne(c => c.Client)
                .WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Contract.Contract>()
                .HasIndex(c => c.ContractNumber)
                .IsUnique();

            modelBuilder.Entity<Contract.Contract>()
                .HasIndex(c => c.LegacyContractId);

            modelBuilder.Entity<Contract.Contract>()
                .HasIndex(c => new { c.Status, c.CreatedAt });

            // ContractNegotiation relationships
            modelBuilder.Entity<Contract.ContractNegotiation>()
                .HasOne(cn => cn.Contract)
                .WithMany(c => c.Negotiations)
                .HasForeignKey(cn => cn.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Contract.ContractNegotiation>()
                .HasIndex(cn => new { cn.ContractId, cn.SubmittedAt });

            // InstallmentSchedule relationships
            modelBuilder.Entity<Contract.InstallmentSchedule>()
                .HasOne(ins => ins.Contract)
                .WithMany(c => c.InstallmentSchedules)
                .HasForeignKey(ins => ins.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Contract.InstallmentSchedule>()
                .HasIndex(ins => new { ins.ContractId, ins.InstallmentNumber })
                .IsUnique();

            modelBuilder.Entity<Contract.InstallmentSchedule>()
                .HasIndex(ins => ins.DueDate);

            modelBuilder.Entity<Contract.InstallmentSchedule>()
                .HasIndex(ins => new { ins.Status, ins.DueDate });

            // LegacyEmployees configuration
            modelBuilder.Entity<Legacy.LegacyEmployee>()
                .HasIndex(le => le.LegacyEmployeeId)
                .IsUnique();

            modelBuilder.Entity<Legacy.LegacyEmployee>()
                .HasIndex(le => new { le.LegacyEmployeeId, le.IsActive })
                .HasFilter("[IsActive] = 1");
        }
    }
}
