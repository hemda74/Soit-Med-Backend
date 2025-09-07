using SoitMed.Models.Core;
using Xunit;

namespace SoitMed.Tests.Models.Core
{
    public class UserRolesTests
    {
        [Fact]
        public void GetAllRoles_ShouldReturnAllDefinedRoles()
        {
            // Act
            var roles = UserRoles.GetAllRoles();

            // Assert
            Assert.NotNull(roles);
            Assert.Equal(10, roles.Count);
            Assert.Contains(UserRoles.SuperAdmin, roles);
            Assert.Contains(UserRoles.Admin, roles);
            Assert.Contains(UserRoles.Doctor, roles);
            Assert.Contains(UserRoles.Technician, roles);
            Assert.Contains(UserRoles.Salesman, roles);
            Assert.Contains(UserRoles.Engineer, roles);
            Assert.Contains(UserRoles.FinanceManager, roles);
            Assert.Contains(UserRoles.FinanceEmployee, roles);
            Assert.Contains(UserRoles.LegalManager, roles);
            Assert.Contains(UserRoles.LegalEmployee, roles);
        }

        [Theory]
        [InlineData("SuperAdmin", true)]
        [InlineData("Admin", true)]
        [InlineData("Doctor", true)]
        [InlineData("InvalidRole", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidRole_ShouldReturnCorrectValidation(string role, bool expectedResult)
        {
            // Act
            var result = UserRoles.IsValidRole(role);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("SuperAdmin", "Administration")]
        [InlineData("Admin", "Administration")]
        [InlineData("Doctor", "Medical")]
        [InlineData("Technician", "Medical")]
        [InlineData("Salesman", "Sales")]
        [InlineData("Engineer", "Engineering")]
        [InlineData("FinanceManager", "Finance")]
        [InlineData("FinanceEmployee", "Finance")]
        [InlineData("LegalManager", "Legal")]
        [InlineData("LegalEmployee", "Legal")]
        [InlineData("InvalidRole", "Unknown")]
        public void GetDepartmentForRole_ShouldReturnCorrectDepartment(string role, string expectedDepartment)
        {
            // Act
            var department = UserRoles.GetDepartmentForRole(role);

            // Assert
            Assert.Equal(expectedDepartment, department);
        }

        [Fact]
        public void GetRolesByDepartment_ShouldReturnCorrectStructure()
        {
            // Act
            var rolesByDepartment = UserRoles.GetRolesByDepartment();

            // Assert
            Assert.NotNull(rolesByDepartment);
            Assert.Equal(6, rolesByDepartment.Count);
            
            // Check Administration department
            Assert.True(rolesByDepartment.ContainsKey("Administration"));
            Assert.Contains(UserRoles.SuperAdmin, rolesByDepartment["Administration"]);
            Assert.Contains(UserRoles.Admin, rolesByDepartment["Administration"]);
            
            // Check Medical department
            Assert.True(rolesByDepartment.ContainsKey("Medical"));
            Assert.Contains(UserRoles.Doctor, rolesByDepartment["Medical"]);
            Assert.Contains(UserRoles.Technician, rolesByDepartment["Medical"]);
            
            // Check other departments exist
            Assert.True(rolesByDepartment.ContainsKey("Sales"));
            Assert.True(rolesByDepartment.ContainsKey("Engineering"));
            Assert.True(rolesByDepartment.ContainsKey("Finance"));
            Assert.True(rolesByDepartment.ContainsKey("Legal"));
        }

        [Fact]
        public void GetManagerRoles_ShouldReturnCorrectManagerRoles()
        {
            // Act
            var managerRoles = UserRoles.GetManagerRoles();

            // Assert
            Assert.NotNull(managerRoles);
            Assert.Equal(4, managerRoles.Count);
            Assert.Contains(UserRoles.SuperAdmin, managerRoles);
            Assert.Contains(UserRoles.Admin, managerRoles);
            Assert.Contains(UserRoles.FinanceManager, managerRoles);
            Assert.Contains(UserRoles.LegalManager, managerRoles);
        }

        [Theory]
        [InlineData("SuperAdmin", true)]
        [InlineData("Admin", true)]
        [InlineData("FinanceManager", true)]
        [InlineData("LegalManager", true)]
        [InlineData("Doctor", false)]
        [InlineData("Engineer", false)]
        [InlineData("InvalidRole", false)]
        public void IsManagerRole_ShouldReturnCorrectResult(string role, bool expectedResult)
        {
            // Act
            var result = UserRoles.IsManagerRole(role);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetAdminRoles_ShouldReturnCorrectAdminRoles()
        {
            // Act
            var adminRoles = UserRoles.GetAdminRoles();

            // Assert
            Assert.NotNull(adminRoles);
            Assert.Equal(4, adminRoles.Count);
            Assert.Contains(UserRoles.SuperAdmin, adminRoles);
            Assert.Contains(UserRoles.Admin, adminRoles);
            Assert.Contains(UserRoles.FinanceManager, adminRoles);
            Assert.Contains(UserRoles.LegalManager, adminRoles);
        }

        [Fact]
        public void UserRoles_Constants_ShouldHaveCorrectValues()
        {
            // Assert - Verify all role constants have expected values
            Assert.Equal("SuperAdmin", UserRoles.SuperAdmin);
            Assert.Equal("Admin", UserRoles.Admin);
            Assert.Equal("Doctor", UserRoles.Doctor);
            Assert.Equal("Technician", UserRoles.Technician);
            Assert.Equal("Salesman", UserRoles.Salesman);
            Assert.Equal("Engineer", UserRoles.Engineer);
            Assert.Equal("FinanceManager", UserRoles.FinanceManager);
            Assert.Equal("FinanceEmployee", UserRoles.FinanceEmployee);
            Assert.Equal("LegalManager", UserRoles.LegalManager);
            Assert.Equal("LegalEmployee", UserRoles.LegalEmployee);
        }

        [Fact]
        public void GetAllRoles_ShouldNotReturnDuplicates()
        {
            // Act
            var roles = UserRoles.GetAllRoles();

            // Assert
            Assert.Equal(roles.Count, roles.Distinct().Count());
        }

        [Fact]
        public void GetRolesByDepartment_AllRolesShouldBeIncluded()
        {
            // Arrange
            var allRoles = UserRoles.GetAllRoles();
            var rolesByDepartment = UserRoles.GetRolesByDepartment();

            // Act
            var rolesInDepartments = rolesByDepartment.Values.SelectMany(x => x).ToList();

            // Assert
            Assert.Equal(allRoles.Count, rolesInDepartments.Count);
            foreach (var role in allRoles)
            {
                Assert.Contains(role, rolesInDepartments);
            }
        }
    }
}
