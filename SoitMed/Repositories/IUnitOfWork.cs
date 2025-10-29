using SoitMed.Models;
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
        IDoctorHospitalRepository DoctorHospitals { get; }

        // Location repositories
        IEngineerRepository Engineers { get; }
        IGovernorateRepository Governorates { get; }

        // Equipment repositories
        IEquipmentRepository Equipment { get; }
        IRepairRequestRepository RepairRequests { get; }

        // Identity repositories
        IApplicationUserRepository Users { get; }
        IUserImageRepository UserImages { get; }

        // Sales report repository (Legacy)
        ISalesReportRepository SalesReports { get; }

        // Weekly plan repositories (New system)
        IWeeklyPlanRepository WeeklyPlans { get; }
        IWeeklyPlanTaskRepository WeeklyPlanTasks { get; }
        IDailyProgressRepository DailyProgresses { get; }

        // Sales funnel repositories
        IActivityLogRepository ActivityLogs { get; }
        IDealRepository Deals { get; }
        IOfferRepository Offers { get; }

        // Transaction management
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        
        // Context access for execution strategy
        Context GetContext();
    }
}
