using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using SoitMed.Models.Equipment;

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

        // User image entities
        public DbSet<UserImage> UserImages { get; set; }

        // Sales report entities
        public DbSet<SalesReport> SalesReports { get; set; }

        // Sales funnel entities
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Offer> Offers { get; set; }

        // Workflow and notification entities
        public DbSet<RequestWorkflow> RequestWorkflows { get; set; }
        public DbSet<DeliveryTerms> DeliveryTerms { get; set; }
        public DbSet<PaymentTerms> PaymentTerms { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Client tracking entities
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientVisit> ClientVisits { get; set; }
        public DbSet<ClientInteraction> ClientInteractions { get; set; }
        public DbSet<ClientAnalytics> ClientAnalytics { get; set; }

        // Weekly planning entities
        public DbSet<WeeklyPlan> WeeklyPlans { get; set; }
        public DbSet<WeeklyPlanTask> WeeklyPlanTasks { get; set; }
        public DbSet<TaskProgress> TaskProgresses { get; set; }

        // Sales workflow entities
        public DbSet<OfferRequest> OfferRequests { get; set; }
        public DbSet<SalesOffer> SalesOffers { get; set; }
        public DbSet<SalesDeal> SalesDeals { get; set; }
        
        // Enhanced offer entities
        public DbSet<OfferEquipment> OfferEquipment { get; set; }
        public DbSet<OfferTerms> OfferTerms { get; set; }
        public DbSet<InstallmentPlan> InstallmentPlans { get; set; }
        public Context(DbContextOptions options) : base(options)
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
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique QR codes
            modelBuilder.Entity<Equipment.Equipment>()
                .HasIndex(e => e.QRCode)
                .IsUnique();

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

            // Ensure either doctor or technician is specified (not both)
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
                .HasIndex(wpt => new { wpt.WeeklyPlanId, wpt.Status });

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
                .HasOne(so => so.Salesman)
                .WithMany()
                .HasForeignKey(so => so.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);

            // SalesDeal relationships
            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.Offer)
                .WithOne(so => so.Deal)
                .HasForeignKey<SalesDeal>(sd => sd.OfferId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.Client)
                .WithMany()
                .HasForeignKey(sd => sd.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesDeal>()
                .HasOne(sd => sd.Salesman)
                .WithMany()
                .HasForeignKey(sd => sd.SalesmanId)
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

            // Indexes for performance
            modelBuilder.Entity<SalesOffer>()
                .HasIndex(so => new { so.Status, so.AssignedTo });

            modelBuilder.Entity<SalesDeal>()
                .HasIndex(sd => new { sd.Status, sd.SalesmanId });

            modelBuilder.Entity<SalesDeal>()
                .HasIndex(sd => new { sd.Status, sd.ManagerApprovedBy });

            modelBuilder.Entity<SalesDeal>()
                .HasIndex(sd => new { sd.Status, sd.SuperAdminApprovedBy });

            // Enhanced offer relationships
            modelBuilder.Entity<OfferEquipment>()
                .HasOne(oe => oe.Offer)
                .WithMany(so => so.Equipment)
                .HasForeignKey(oe => oe.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OfferTerms>()
                .HasOne(ot => ot.Offer)
                .WithOne(so => so.Terms)
                .HasForeignKey<OfferTerms>(ot => ot.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InstallmentPlan>()
                .HasOne(ip => ip.Offer)
                .WithMany(so => so.InstallmentPlans)
                .HasForeignKey(ip => ip.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
