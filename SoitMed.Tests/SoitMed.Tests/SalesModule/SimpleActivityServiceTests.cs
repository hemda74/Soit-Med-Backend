using Microsoft.Extensions.Logging;
using Moq;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using SoitMed.Services;
using Xunit;

namespace SoitMed.Tests.SalesModule
{
    public class SimpleActivityServiceTests
    {
        [Fact]
        public void ActivityService_ShouldBeCreated()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<ActivityService>>();

            // Act
            var service = new ActivityService(mockUnitOfWork.Object, mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void ManagerDashboardService_ShouldBeCreated()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<ManagerDashboardService>>();

            // Act
            var service = new ManagerDashboardService(mockUnitOfWork.Object, mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void CreateActivityRequestDto_ShouldBeValid()
        {
            // Arrange & Act
            var dto = new CreateActivityRequestDto
            {
                InteractionType = InteractionType.Visit,
                ClientType = ClientType.A,
                Result = ActivityResult.Interested,
                Comment = "Test comment"
            };

            // Assert
            Assert.Equal(InteractionType.Visit, dto.InteractionType);
            Assert.Equal(ClientType.A, dto.ClientType);
            Assert.Equal(ActivityResult.Interested, dto.Result);
            Assert.Equal("Test comment", dto.Comment);
        }

        [Fact]
        public void CreateDealDto_ShouldBeValid()
        {
            // Arrange & Act
            var dto = new CreateDealDto
            {
                DealValue = 50000m,
                ExpectedCloseDate = DateTime.Today.AddDays(30)
            };

            // Assert
            Assert.Equal(50000m, dto.DealValue);
            Assert.NotNull(dto.ExpectedCloseDate);
        }

        [Fact]
        public void CreateOfferDto_ShouldBeValid()
        {
            // Arrange & Act
            var dto = new CreateOfferDto
            {
                OfferDetails = "Test offer details",
                DocumentUrl = "https://example.com/document.pdf"
            };

            // Assert
            Assert.Equal("Test offer details", dto.OfferDetails);
            Assert.Equal("https://example.com/document.pdf", dto.DocumentUrl);
        }
    }
}



