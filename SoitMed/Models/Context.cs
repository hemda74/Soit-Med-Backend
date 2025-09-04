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

        // Location entities
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Engineer> Engineers { get; set; }
        public DbSet<EngineerGovernorate> EngineerGovernorates { get; set; }

        // Equipment entities
        public DbSet<Equipment.Equipment> Equipment { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }

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

            // Configure Hospital relationships
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Hospital)
                .WithMany(h => h.Doctors)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

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
        }
    }
}
