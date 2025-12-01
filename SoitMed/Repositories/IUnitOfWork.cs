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
        
        // Maintenance repositories
        IMaintenanceRequestRepository MaintenanceRequests { get; }
        IMaintenanceVisitRepository MaintenanceVisits { get; }
        ISparePartRequestRepository SparePartRequests { get; }
        IMaintenanceRequestAttachmentRepository MaintenanceRequestAttachments { get; }
        
        // Payment repositories
        IPaymentRepository Payments { get; }

        // Identity repositories
        IApplicationUserRepository Users { get; }
        IUserImageRepository UserImages { get; }

        // Sales funnel repositories
        IActivityLogRepository ActivityLogs { get; }
        IDealRepository Deals { get; }
        IOfferRepository Offers { get; }

        // Workflow and notification repositories
        IRequestWorkflowRepository RequestWorkflows { get; }
        INotificationRepository Notifications { get; }

        // Client tracking repositories
        IClientRepository Clients { get; }
        IClientVisitRepository ClientVisits { get; }
        IClientInteractionRepository ClientInteractions { get; }
        IClientAnalyticsRepository ClientAnalytics { get; }

        // Weekly planning repositories
        IWeeklyPlanRepository WeeklyPlans { get; }
        IWeeklyPlanTaskRepository WeeklyPlanTasks { get; }
        ITaskProgressRepository TaskProgresses { get; }
        IOfferRequestRepository OfferRequests { get; }
        ISalesOfferRepository SalesOffers { get; }
        ISalesDealRepository SalesDeals { get; }

        // Chat repositories
        IChatConversationRepository ChatConversations { get; }
        IChatMessageRepository ChatMessages { get; }
        
        // Enhanced offer repositories
        IOfferEquipmentRepository OfferEquipment { get; }
        IOfferTermsRepository OfferTerms { get; }
        IInstallmentPlanRepository InstallmentPlans { get; }
        IRecentOfferActivityRepository RecentOfferActivities { get; }

        // Salesman targets repository
        ISalesmanTargetRepository SalesmanTargets { get; }

        // Products catalog repository
        IProductRepository Products { get; }

        // Transaction management
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        
        // Context access
        Context GetContext();
    }
}
