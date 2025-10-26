using Microsoft.EntityFrameworkCore.Storage;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Context _context;
        private IDbContextTransaction? _transaction;

        // Core repositories
        private IDepartmentRepository? _departments;
        private IRoleRepository? _roles;

        // Hospital repositories
        private IHospitalRepository? _hospitals;
        private IDoctorRepository? _doctors;
        private ITechnicianRepository? _technicians;
        private IDoctorHospitalRepository? _doctorHospitals;

        // Location repositories
        private IEngineerRepository? _engineers;
        private IGovernorateRepository? _governorates;

        // Equipment repositories
        private IEquipmentRepository? _equipment;
        private IRepairRequestRepository? _repairRequests;

        // Identity repositories
        private IApplicationUserRepository? _users;
        private IUserImageRepository? _userImages;

        // Sales report repository
        private ISalesReportRepository? _salesReports;

        // Sales funnel repositories
        private IActivityLogRepository? _activityLogs;
        private IDealRepository? _deals;
        private IOfferRepository? _offers;

        // Workflow and notification repositories
        private IRequestWorkflowRepository? _requestWorkflows;
        private INotificationRepository? _notifications;

        // Client tracking repositories
        private IClientRepository? _clients;
        private IClientVisitRepository? _clientVisits;
        private IClientInteractionRepository? _clientInteractions;
        private IClientAnalyticsRepository? _clientAnalytics;

        // Weekly planning repositories
        private IWeeklyPlanRepository? _weeklyPlans;
        private IWeeklyPlanTaskRepository? _weeklyPlanTasks;
        private ITaskProgressRepository? _taskProgresses;

        // Sales workflow repositories
        private IOfferRequestRepository? _offerRequests;
        private ISalesOfferRepository? _salesOffers;
        private ISalesDealRepository? _salesDeals;
        
        // Enhanced offer repositories
        private IOfferEquipmentRepository? _offerEquipment;
        private IOfferTermsRepository? _offerTerms;
        private IInstallmentPlanRepository? _installmentPlans;

        public UnitOfWork(Context context)
        {
            _context = context;
        }

        // Core repositories
        public IDepartmentRepository Departments => 
            _departments ??= new DepartmentRepository(_context);

        public IRoleRepository Roles => 
            _roles ??= new RoleRepository(_context);

        // Hospital repositories
        public IHospitalRepository Hospitals => 
            _hospitals ??= new HospitalRepository(_context);

        public IDoctorRepository Doctors => 
            _doctors ??= new DoctorRepository(_context);

        public ITechnicianRepository Technicians => 
            _technicians ??= new TechnicianRepository(_context);

        public IDoctorHospitalRepository DoctorHospitals => 
            _doctorHospitals ??= new DoctorHospitalRepository(_context);

        // Location repositories
        public IEngineerRepository Engineers => 
            _engineers ??= new EngineerRepository(_context);

        public IGovernorateRepository Governorates => 
            _governorates ??= new GovernorateRepository(_context);

        // Equipment repositories
        public IEquipmentRepository Equipment => 
            _equipment ??= new EquipmentRepository(_context);

        public IRepairRequestRepository RepairRequests => 
            _repairRequests ??= new RepairRequestRepository(_context);

        // Identity repositories
        public IApplicationUserRepository Users => 
            _users ??= new ApplicationUserRepository(_context);

        public IUserImageRepository UserImages => 
            _userImages ??= new UserImageRepository(_context);

        // Sales report repository
        public ISalesReportRepository SalesReports => 
            _salesReports ??= new SalesReportRepository(_context);

        // Sales funnel repositories
        public IActivityLogRepository ActivityLogs => 
            _activityLogs ??= new ActivityLogRepository(_context);

        public IDealRepository Deals => 
            _deals ??= new DealRepository(_context);

        public IOfferRepository Offers => 
            _offers ??= new OfferRepository(_context);

        // Workflow and notification repositories
        public IRequestWorkflowRepository RequestWorkflows => 
            _requestWorkflows ??= new RequestWorkflowRepository(_context);

        public INotificationRepository Notifications => 
            _notifications ??= new NotificationRepository(_context);

        // Client tracking repositories
        public IClientRepository Clients => 
            _clients ??= new ClientRepository(_context);

        public IClientVisitRepository ClientVisits => 
            _clientVisits ??= new ClientVisitRepository(_context);

        public IClientInteractionRepository ClientInteractions => 
            _clientInteractions ??= new ClientInteractionRepository(_context);

        public IClientAnalyticsRepository ClientAnalytics => 
            _clientAnalytics ??= new ClientAnalyticsRepository(_context);

        // Weekly planning repositories
        public IWeeklyPlanRepository WeeklyPlans => 
            _weeklyPlans ??= new WeeklyPlanRepository(_context);

        public IWeeklyPlanTaskRepository WeeklyPlanTasks => 
            _weeklyPlanTasks ??= new WeeklyPlanTaskRepository(_context);

        public ITaskProgressRepository TaskProgresses => 
            _taskProgresses ??= new TaskProgressRepository(_context);

        // Sales workflow repositories
        public IOfferRequestRepository OfferRequests => 
            _offerRequests ??= new OfferRequestRepository(_context);

        public ISalesOfferRepository SalesOffers => 
            _salesOffers ??= new SalesOfferRepository(_context);

        public ISalesDealRepository SalesDeals => 
            _salesDeals ??= new SalesDealRepository(_context);
        
        // Enhanced offer repositories
        public IOfferEquipmentRepository OfferEquipment => 
            _offerEquipment ??= new OfferEquipmentRepository(_context);

        public IOfferTermsRepository OfferTerms => 
            _offerTerms ??= new OfferTermsRepository(_context);

        public IInstallmentPlanRepository InstallmentPlans => 
            _installmentPlans ??= new InstallmentPlanRepository(_context);


        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public Context GetContext()
        {
            return _context;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
