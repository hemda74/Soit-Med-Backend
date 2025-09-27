using SoitMed.Models.Identity;

namespace SoitMed.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Core repositories
        IDepartmentRepository Departments { get; }
        IRoleRepository Roles { get; }

        // Hospital repositories
        IHospitalRepository Hospitals { get; }
        IDoctorRepository Doctors { get; }
        ITechnicianRepository Technicians { get; }

        // Location repositories
        IEngineerRepository Engineers { get; }
        IGovernorateRepository Governorates { get; }

        // Equipment repositories
        IEquipmentRepository Equipment { get; }
        IRepairRequestRepository RepairRequests { get; }

        // Identity repositories
        IApplicationUserRepository Users { get; }
        IUserImageRepository UserImages { get; }

        // Sales report repository (already exists)
        ISalesReportRepository SalesReports { get; }

        // Transaction management
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
