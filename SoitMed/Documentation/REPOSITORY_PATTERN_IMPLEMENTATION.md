# Repository Pattern Implementation

This document explains the Repository pattern implementation in the Soit-Med Backend project.

## Overview

The Repository pattern has been implemented to abstract data access logic and provide a clean separation between the business logic and data access layers. This pattern makes the code more maintainable, testable, and follows the Single Responsibility Principle.

## Architecture

### 1. Base Repository

#### IBaseRepository<T>

- Generic interface providing common CRUD operations
- Includes methods for Get, Create, Update, Delete operations
- Supports filtering, pagination, and existence checks
- Provides queryable access for complex queries

#### BaseRepository<T>

- Generic implementation of IBaseRepository<T>
- Uses Entity Framework DbContext for data access
- Implements all common CRUD operations
- Can be extended by specific repository implementations

### 2. Specific Repositories

Each entity has its own repository interface and implementation:

#### Core Entities

- **IDepartmentRepository** / **DepartmentRepository**
- **IRoleRepository** / **RoleRepository**

#### Hospital Entities

- **IHospitalRepository** / **HospitalRepository**
- **IDoctorRepository** / **DoctorRepository**
- **ITechnicianRepository** / **TechnicianRepository**

#### Location Entities

- **IEngineerRepository** / **EngineerRepository**
- **IGovernorateRepository** / **GovernorateRepository**

#### Equipment Entities

- **IEquipmentRepository** / **EquipmentRepository**
- **IRepairRequestRepository** / **RepairRequestRepository**

#### Identity Entities

- **IApplicationUserRepository** / **ApplicationUserRepository**
- **IUserImageRepository** / **UserImageRepository**

#### Sales Report Entity

- **ISalesReportRepository** / **SalesReportRepository** (already existed)

### 3. Unit of Work Pattern

#### IUnitOfWork

- Coordinates all repository operations
- Manages database transactions
- Provides access to all repositories through a single interface
- Ensures data consistency across multiple operations

#### UnitOfWork

- Implements IUnitOfWork interface
- Manages DbContext instance
- Provides lazy initialization of repositories
- Handles transaction management

## Key Features

### 1. Generic CRUD Operations

All repositories inherit common operations:

- `GetByIdAsync(id)` - Get entity by ID
- `GetAllAsync()` - Get all entities
- `GetFilteredAsync(predicate)` - Get entities matching criteria
- `CreateAsync(entity)` - Create new entity
- `UpdateAsync(entity)` - Update existing entity
- `DeleteAsync(id/entity)` - Delete entity
- `ExistsAsync(id/predicate)` - Check if entity exists
- `CountAsync()` - Get entity count

### 2. Entity-Specific Operations

Each repository includes specialized methods:

- **DepartmentRepository**: `GetByNameAsync()`, `ExistsByNameAsync()`
- **HospitalRepository**: `GetByHospitalIdAsync()`, `GetHospitalWithDoctorsAsync()`
- **EquipmentRepository**: `GetByQRCodeAsync()`, `GetByStatusAsync()`
- **RepairRequestRepository**: `GetByEquipmentIdAsync()`, `GetByStatusAsync()`

### 3. Navigation Property Loading

Repositories provide methods to load related entities:

- `GetDepartmentWithUsersAsync()`
- `GetHospitalWithAllDetailsAsync()`
- `GetEquipmentWithRepairRequestsAsync()`

### 4. Transaction Management

Unit of Work provides transaction support:

- `BeginTransactionAsync()`
- `CommitTransactionAsync()`
- `RollbackTransactionAsync()`

## Usage Examples

### Controller Implementation

```csharp
public class DepartmentController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _unitOfWork.Departments.GetDepartmentsWithUsersAsync();
        // Process and return data
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment(DepartmentDTO dto)
    {
        if (await _unitOfWork.Departments.ExistsByNameAsync(dto.Name))
        {
            return BadRequest("Department already exists");
        }

        var department = new Department { /* ... */ };
        await _unitOfWork.Departments.CreateAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return Ok("Department created successfully");
    }
}
```

### Service Implementation

```csharp
public class EquipmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Equipment> CreateEquipmentAsync(EquipmentDTO dto)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(dto.HospitalId);
            if (hospital == null) throw new InvalidOperationException("Hospital not found");

            var equipment = new Equipment { /* ... */ };
            await _unitOfWork.Equipment.CreateAsync(equipment);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return equipment;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

## Benefits

### 1. Separation of Concerns

- Business logic is separated from data access logic
- Controllers focus on HTTP concerns, not database operations
- Data access logic is centralized in repositories

### 2. Testability

- Repositories can be easily mocked for unit testing
- Business logic can be tested without database dependencies
- Unit of Work pattern enables testing transaction scenarios

### 3. Maintainability

- Changes to data access logic are isolated to repositories
- Common operations are implemented once in BaseRepository
- Entity-specific operations are clearly defined in interfaces

### 4. Flexibility

- Easy to switch between different data access technologies
- Repository implementations can be optimized independently
- Unit of Work pattern enables complex transaction scenarios

### 5. Code Reusability

- Common CRUD operations are inherited by all repositories
- Entity-specific operations can be reused across services
- Unit of Work provides consistent access to all repositories

## Dependency Injection

All repositories and Unit of Work are registered in `Program.cs`:

```csharp
// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Individual repositories are automatically available through Unit of Work
// No need to register each repository individually
```

## Migration from Direct DbContext Usage

The existing code has been updated to use the Repository pattern:

1. **Controllers**: Updated to use `IUnitOfWork` instead of direct `Context` access
2. **Services**: Can now use repositories for data access
3. **Existing Logic**: All existing business logic remains unchanged
4. **Database Operations**: Now go through repository methods instead of direct DbContext calls

## Future Enhancements

1. **Caching**: Add caching layer to repositories
2. **Specification Pattern**: Implement specification pattern for complex queries
3. **Audit Logging**: Add audit logging to repository operations
4. **Performance Monitoring**: Add performance metrics to repository operations
5. **Async/Await Optimization**: Optimize async operations for better performance

## Conclusion

The Repository pattern implementation provides a clean, maintainable, and testable architecture for data access in the Soit-Med Backend project. It follows SOLID principles and provides a solid foundation for future enhancements and modifications.
