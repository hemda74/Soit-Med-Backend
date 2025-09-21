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

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
