using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SoitMed.Models;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;
using System.Reflection;
using Xunit;

namespace SoitMed.Tests
{
    public class ProgramTests : IDisposable
    {
        private readonly Context _context;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IServiceProvider> _mockServiceProvider;

        public ProgramTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Context(options);

            // Setup mock RoleManager
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            // Setup mock service provider
            _mockServiceProvider = new Mock<IServiceProvider>();
        }

        [Fact]
        public async Task SeedRoles_ShouldCreateAllRolesWhenNoneExist()
        {
            // Arrange
            var allRoles = UserRoles.GetAllRoles();
            var createdRoles = new List<string>();

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<IdentityRole>(role => createdRoles.Add(role.Name!));

            // Act
            await InvokeSeedRoles(_mockRoleManager.Object);

            // Assert
            Assert.Equal(allRoles.Count, createdRoles.Count);
            foreach (var role in allRoles)
            {
                Assert.Contains(role, createdRoles);
            }

            _mockRoleManager.Verify(rm => rm.RoleExistsAsync(It.IsAny<string>()), 
                Times.Exactly(allRoles.Count));
            _mockRoleManager.Verify(rm => rm.CreateAsync(It.IsAny<IdentityRole>()), 
                Times.Exactly(allRoles.Count));
        }

        [Fact]
        public async Task SeedRoles_ShouldNotCreateRolesWhenTheyAlreadyExist()
        {
            // Arrange
            var allRoles = UserRoles.GetAllRoles();

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            await InvokeSeedRoles(_mockRoleManager.Object);

            // Assert
            _mockRoleManager.Verify(rm => rm.RoleExistsAsync(It.IsAny<string>()), 
                Times.Exactly(allRoles.Count));
            _mockRoleManager.Verify(rm => rm.CreateAsync(It.IsAny<IdentityRole>()), 
                Times.Never);
        }

        [Fact]
        public async Task SeedRoles_ShouldCreateOnlyMissingRoles()
        {
            // Arrange
            var existingRoles = new[] { UserRoles.Admin, UserRoles.Doctor };
            var createdRoles = new List<string>();

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync((string role) => existingRoles.Contains(role));

            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<IdentityRole>(role => createdRoles.Add(role.Name!));

            // Act
            await InvokeSeedRoles(_mockRoleManager.Object);

            // Assert
            var allRoles = UserRoles.GetAllRoles();
            var expectedNewRoles = allRoles.Except(existingRoles).ToList();

            Assert.Equal(expectedNewRoles.Count, createdRoles.Count);
            foreach (var role in expectedNewRoles)
            {
                Assert.Contains(role, createdRoles);
            }

            foreach (var existingRole in existingRoles)
            {
                Assert.DoesNotContain(existingRole, createdRoles);
            }
        }

        [Fact]
        public async Task SeedDepartments_ShouldCreateAllDepartmentsWhenNoneExist()
        {
            // Arrange
            var expectedDepartments = new[]
            {
                "Administration", "Medical", "Sales", "Engineering", "Finance", "Legal"
            };

            // Act
            await InvokeSeedDepartments(_context);

            // Assert
            var departments = await _context.Departments.ToListAsync();
            Assert.Equal(expectedDepartments.Length, departments.Count);

            foreach (var expectedDept in expectedDepartments)
            {
                var department = departments.FirstOrDefault(d => d.Name == expectedDept);
                Assert.NotNull(department);
                Assert.NotNull(department.Description);
                Assert.NotEmpty(department.Description);
            }
        }

        [Fact]
        public async Task SeedDepartments_ShouldNotCreateDuplicateDepartments()
        {
            // Arrange - Add some departments first
            _context.Departments.AddRange(
                new Department { Name = "Administration", Description = "Existing admin dept" },
                new Department { Name = "Medical", Description = "Existing medical dept" }
            );
            await _context.SaveChangesAsync();

            var initialCount = await _context.Departments.CountAsync();

            // Act
            await InvokeSeedDepartments(_context);

            // Assert
            var finalCount = await _context.Departments.CountAsync();
            var expectedTotalCount = 6; // All 6 departments should exist
            
            Assert.Equal(expectedTotalCount, finalCount);
            
            // Verify no duplicates
            var departmentNames = await _context.Departments.Select(d => d.Name).ToListAsync();
            Assert.Equal(departmentNames.Count, departmentNames.Distinct().Count());
        }

        [Fact]
        public async Task SeedDepartments_ShouldHaveCorrectDescriptions()
        {
            // Act
            await InvokeSeedDepartments(_context);

            // Assert
            var departments = await _context.Departments.ToListAsync();
            
            var administrationDept = departments.FirstOrDefault(d => d.Name == "Administration");
            Assert.NotNull(administrationDept);
            Assert.Equal("Administrative and management roles", administrationDept.Description);

            var medicalDept = departments.FirstOrDefault(d => d.Name == "Medical");
            Assert.NotNull(medicalDept);
            Assert.Equal("Medical staff including doctors and technicians", medicalDept.Description);

            var salesDept = departments.FirstOrDefault(d => d.Name == "Sales");
            Assert.NotNull(salesDept);
            Assert.Equal("Sales team and customer relations", salesDept.Description);

            var engineeringDept = departments.FirstOrDefault(d => d.Name == "Engineering");
            Assert.NotNull(engineeringDept);
            Assert.Equal("Technical and engineering staff", engineeringDept.Description);

            var financeDept = departments.FirstOrDefault(d => d.Name == "Finance");
            Assert.NotNull(financeDept);
            Assert.Equal("Financial management and accounting", financeDept.Description);

            var legalDept = departments.FirstOrDefault(d => d.Name == "Legal");
            Assert.NotNull(legalDept);
            Assert.Equal("Legal affairs and compliance", legalDept.Description);
        }

        [Fact]
        public async Task SeedDepartments_ShouldSetCreatedAtToCurrentTime()
        {
            // Arrange
            var beforeSeeding = DateTime.UtcNow.AddMinutes(-1);

            // Act
            await InvokeSeedDepartments(_context);
            var afterSeeding = DateTime.UtcNow.AddMinutes(1);

            // Assert
            var departments = await _context.Departments.ToListAsync();
            
            foreach (var department in departments)
            {
                Assert.True(department.CreatedAt >= beforeSeeding, 
                    $"Department {department.Name} CreatedAt should be after {beforeSeeding}");
                Assert.True(department.CreatedAt <= afterSeeding, 
                    $"Department {department.Name} CreatedAt should be before {afterSeeding}");
            }
        }

        [Fact]
        public async Task SeedDepartments_ShouldSaveChangesToDatabase()
        {
            // Arrange
            var initialCount = await _context.Departments.CountAsync();

            // Act
            await InvokeSeedDepartments(_context);

            // Assert
            var finalCount = await _context.Departments.CountAsync();
            Assert.True(finalCount > initialCount);

            // Verify changes are persisted by checking the context directly
            // In-memory database persists changes within the same context instance
            var departments = await _context.Departments.ToListAsync();
            Assert.Equal(6, departments.Count); // Should have all 6 departments
            
            var expectedDepartmentNames = new[] { "Administration", "Medical", "Sales", "Engineering", "Finance", "Legal" };
            foreach (var expectedName in expectedDepartmentNames)
            {
                Assert.Contains(departments, d => d.Name == expectedName);
            }
        }

        [Fact]
        public async Task SeedDepartments_ShouldInitializeUsersCollection()
        {
            // Act
            await InvokeSeedDepartments(_context);

            // Assert
            var departments = await _context.Departments.ToListAsync();
            
            foreach (var department in departments)
            {
                Assert.NotNull(department.Users);
                Assert.Empty(department.Users); // Should be empty initially
            }
        }

        private static async Task InvokeSeedRoles(RoleManager<IdentityRole> roleManager)
        {
            // Use reflection to call the private static method
            var programType = typeof(SoitMed.Program);
            var seedRolesMethod = programType.GetMethod("SeedRoles", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            Assert.NotNull(seedRolesMethod);
            
            var task = (Task)seedRolesMethod.Invoke(null, new object[] { roleManager })!;
            await task;
        }

        private static async Task InvokeSeedDepartments(Context context)
        {
            // Use reflection to call the private static method
            var programType = typeof(SoitMed.Program);
            var seedDepartmentsMethod = programType.GetMethod("SeedDepartments", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            Assert.NotNull(seedDepartmentsMethod);
            
            var task = (Task)seedDepartmentsMethod.Invoke(null, new object[] { context })!;
            await task;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
