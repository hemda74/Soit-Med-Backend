using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SoitMed.Controllers;
using SoitMed.Services;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Common;

namespace SoitMed.Tests.SalesModule
{
    public class ClientControllerTests
    {
        private readonly Mock<IClientService> _clientServiceMock;
        private readonly Mock<IValidationService> _validationServiceMock;
        private readonly Mock<ILogger<ClientController>> _loggerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ClientController _controller;

        public ClientControllerTests()
        {
            _clientServiceMock = new Mock<IClientService>();
            _validationServiceMock = new Mock<IValidationService>();
            _loggerMock = new Mock<ILogger<ClientController>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null!, null!, null!, null!, null!, null!, null!, null!);
            
            _controller = new ClientController(
                _clientServiceMock.Object,
                _validationServiceMock.Object,
                _loggerMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task SearchClients_WithValidQuery_ReturnsOk()
        {
            // Arrange
            var searchDto = new SearchClientDTO { Query = "test", Page = 1, PageSize = 20 };
            var clients = new List<ClientResponseDTO> { new ClientResponseDTO { Id = 1, Name = "Test Client" } };
            
            _validationServiceMock.Setup(v => v.ValidateClientSearch(It.IsAny<SearchClientDTO>()))
                .Returns(new ValidationResult { IsValid = true, Errors = new List<string>() });
            _clientServiceMock.Setup(s => s.SearchClientsAsync(It.IsAny<SearchClientDTO>(), It.IsAny<string>()))
                .ReturnsAsync(clients);

            // Setup user context
            var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.SearchClients("test", null, null, 1, 20);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateClient_WithValidData_ReturnsCreated()
        {
            // Arrange
            var createDto = new CreateClientDTO { Name = "New Client", Type = "Hospital" };
            var createdClient = new ClientResponseDTO { Id = 1, Name = "New Client" };

            _validationServiceMock.Setup(v => v.ValidateClientCreationAsync(It.IsAny<CreateClientDTO>()))
                .ReturnsAsync(new ValidationResult { IsValid = true, Errors = new List<string>() });
            _clientServiceMock.Setup(s => s.CreateClientAsync(It.IsAny<CreateClientDTO>(), It.IsAny<string>()))
                .ReturnsAsync(createdClient);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.CreateClient(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult);
        }

        [Fact]
        public async Task GetClient_WithValidId_ReturnsOk()
        {
            // Arrange
            var client = new ClientResponseDTO { Id = 1, Name = "Test Client" };
            _clientServiceMock.Setup(s => s.GetClientAsync(1))
                .ReturnsAsync(client);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetClient(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetClient_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _clientServiceMock.Setup(s => s.GetClientAsync(999))
                .ReturnsAsync((ClientResponseDTO?)null);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetClient(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateClient_WithValidData_ReturnsOk()
        {
            // Arrange
            var updateDto = new UpdateClientDTO { Name = "Updated Client" };
            var updatedClient = new ClientResponseDTO { Id = 1, Name = "Updated Client" };

            _validationServiceMock.Setup(v => v.ValidateClientUpdateAsync(It.IsAny<long>(), It.IsAny<UpdateClientDTO>()))
                .ReturnsAsync(new ValidationResult { IsValid = true, Errors = new List<string>() });
            _clientServiceMock.Setup(s => s.UpdateClientAsync(It.IsAny<long>(), It.IsAny<CreateClientDTO>(), It.IsAny<string>()))
                .ReturnsAsync(updatedClient);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.UpdateClient(1, updateDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMyClients_ReturnsOk()
        {
            // Arrange
            var clients = new List<ClientResponseDTO> { new ClientResponseDTO { Id = 1 } };
            _clientServiceMock.Setup(s => s.GetMyClientsAsync(It.IsAny<string>(), 1, 20))
                .ReturnsAsync(clients);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetMyClients(1, 20);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetClientsNeedingFollowUp_ReturnsOk()
        {
            // Arrange
            var clients = new List<ClientResponseDTO>();
            _clientServiceMock.Setup(s => s.GetClientsNeedingFollowUpAsync(It.IsAny<string>()))
                .ReturnsAsync(clients);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetClientsNeedingFollowUp();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetClientStatistics_ReturnsOk()
        {
            // Arrange
            var statistics = new ClientStatisticsDTO { TotalVisits = 10 };
            _clientServiceMock.Setup(s => s.GetClientStatisticsAsync(It.IsAny<string>()))
                .ReturnsAsync(statistics);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetClientStatistics();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetClientProfile_WithValidId_ReturnsOk()
        {
            // Arrange
            var profile = new ClientProfileDTO { ClientInfo = new ClientResponseDTO { Id = 1 } };
            _clientServiceMock.Setup(s => s.GetClientProfileAsync(1, It.IsAny<string>()))
                .ReturnsAsync(profile);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetClientProfile(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task FindOrCreateClient_WithValidData_ReturnsOk()
        {
            // Arrange
            var findDto = new FindOrCreateClientDTO { Name = "Test", Type = "Hospital" };
            var client = new ClientResponseDTO { Id = 1, Name = "Test" };

            _validationServiceMock.Setup(v => v.ValidateClientFindOrCreate(It.IsAny<FindOrCreateClientDTO>()))
                .Returns(new ValidationResult { IsValid = true, Errors = new List<string>() });
            _clientServiceMock.Setup(s => s.FindOrCreateClientAsync(It.IsAny<FindOrCreateClientDTO>(), It.IsAny<string>()))
                .ReturnsAsync(client);

            var user = new ApplicationUser { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("user1");
            _userManagerMock.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.FindOrCreateClient(findDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}

