using SoitMed.Models.Core;
using SoitMed.Models.Identity;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace SoitMed.Tests.Models.Core
{
    public class DepartmentTests
    {
        [Fact]
        public void Department_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var department = new Department();

            // Assert
            Assert.Equal(0, department.Id);
            Assert.Equal(string.Empty, department.Name);
            Assert.Null(department.Description);
            Assert.True(department.CreatedAt <= DateTime.UtcNow);
            Assert.True(department.CreatedAt > DateTime.UtcNow.AddMinutes(-1)); // Created within last minute
            Assert.NotNull(department.Users);
            Assert.Empty(department.Users);
        }

        [Fact]
        public void Department_SetProperties_ShouldAssignCorrectly()
        {
            // Arrange
            var department = new Department();
            var testDate = DateTime.UtcNow.AddDays(-1);

            // Act
            department.Id = 1;
            department.Name = "Test Department";
            department.Description = "Test Description";
            department.CreatedAt = testDate;

            // Assert
            Assert.Equal(1, department.Id);
            Assert.Equal("Test Department", department.Name);
            Assert.Equal("Test Description", department.Description);
            Assert.Equal(testDate, department.CreatedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Department_Name_RequiredValidation_ShouldFail(string name)
        {
            // Arrange
            var department = new Department { Name = name };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Department_Name_ValidValue_ShouldPass()
        {
            // Arrange
            var department = new Department { Name = "Valid Department Name" };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Department_Name_MaxLengthValidation_ShouldFailWhenTooLong()
        {
            // Arrange
            var longName = new string('A', 101); // 101 characters, exceeds MaxLength(100)
            var department = new Department { Name = longName };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Department_Name_MaxLengthValidation_ShouldPassWhenAtLimit()
        {
            // Arrange
            var maxLengthName = new string('A', 100); // Exactly 100 characters
            var department = new Department { Name = maxLengthName };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Department_Description_MaxLengthValidation_ShouldFailWhenTooLong()
        {
            // Arrange
            var longDescription = new string('A', 501); // 501 characters, exceeds MaxLength(500)
            var department = new Department 
            { 
                Name = "Valid Name",
                Description = longDescription 
            };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Description"));
        }

        [Fact]
        public void Department_Description_MaxLengthValidation_ShouldPassWhenAtLimit()
        {
            // Arrange
            var maxLengthDescription = new string('A', 500); // Exactly 500 characters
            var department = new Department 
            { 
                Name = "Valid Name",
                Description = maxLengthDescription 
            };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Department_Description_NullValue_ShouldPass()
        {
            // Arrange
            var department = new Department 
            { 
                Name = "Valid Name",
                Description = null 
            };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Department_Users_NavigationProperty_ShouldAllowAddingUsers()
        {
            // Arrange
            var department = new Department { Name = "Test Department" };
            var user1 = new ApplicationUser { UserName = "user1@test.com" };
            var user2 = new ApplicationUser { UserName = "user2@test.com" };

            // Act
            department.Users.Add(user1);
            department.Users.Add(user2);

            // Assert
            Assert.Equal(2, department.Users.Count);
            Assert.Contains(user1, department.Users);
            Assert.Contains(user2, department.Users);
        }

        [Fact]
        public void Department_CompleteValidObject_ShouldPassValidation()
        {
            // Arrange
            var department = new Department
            {
                Id = 1,
                Name = "Information Technology",
                Description = "Responsible for managing IT infrastructure and software development",
                CreatedAt = DateTime.UtcNow
            };
            var context = new ValidationContext(department);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(department, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Department_CreatedAt_ShouldBeSetToCurrentUtcTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var department = new Department();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(department.CreatedAt >= beforeCreation);
            Assert.True(department.CreatedAt <= afterCreation);
        }
    }
}
