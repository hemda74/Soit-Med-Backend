using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using SoitMed.Services;
using Xunit;

namespace SoitMed.Tests.SalesModule
{
    public class OfferServiceLifecycleTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IDealService> _mockDealService;
        private readonly Mock<ILogger<OfferService>> _mockLogger;

        public OfferServiceLifecycleTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            _mockNotificationService = new Mock<INotificationService>();
            _mockDealService = new Mock<IDealService>();
            _mockLogger = new Mock<ILogger<OfferService>>();
        }

        #region Offer Creation Tests

        [Fact]
        public async Task CreateOfferFromRequest_ShouldCreateOfferWithDraftStatus()
        {
            // Arrange
            var offerRequest = new OfferRequest
            {
                Id = 1,
                Status = "Assigned",
                ClientId = 1
            };

            var mockOfferRequestRepo = new Mock<IOfferRequestRepository>();
            mockOfferRequestRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offerRequest);
            _mockUnitOfWork.Setup(u => u.OfferRequests).Returns(mockOfferRequestRepo.Object);

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offerRequest.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = "user-id", UserName = "user" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            var createDto = new CreateOfferDTO
            {
                OfferRequestId = 1,
                ClientId = 1,
                AssignedTo = "salesman-id",
                TotalAmount = 50000m,
                Products = "Test products"
            };

            // Act
            var result = await service.CreateOfferFromRequestAsync(createDto, "user-id");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Draft", result.Status);
            mockSalesOfferRepo.Verify(r => r.CreateAsync(It.Is<SalesOffer>(o => o.Status == "Draft"), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Status Transition Tests

        [Fact]
        public async Task SendOfferToSalesman_ShouldChangeStatusFromDraftToSent()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Draft",
                ClientId = 1,
                AssignedTo = "salesman-id"
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            mockSalesOfferRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesOffer>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesOffer o, CancellationToken ct) => o);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var assignedUser = new ApplicationUser { Id = offer.AssignedTo ?? "salesman-id", UserName = "salesman" };
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(assignedUser);
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUserManager.Setup(m => m.GetRolesAsync(assignedUser)).ReturnsAsync(new List<string> { "Salesman" });
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act
            await service.SendToSalesmanAsync(1, "user-id");

            // Assert
            Assert.Equal("Sent", offer.Status);
            mockSalesOfferRepo.Verify(r => r.UpdateAsync(offer, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendOfferToSalesman_ShouldThrowExceptionIfNotDraft()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Sent", // Already sent
                ClientId = 1,
                AssignedTo = "salesman-id"
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var assignedUser = new ApplicationUser { Id = offer.AssignedTo, UserName = "salesman" };
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(assignedUser);
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUserManager.Setup(m => m.GetRolesAsync(assignedUser)).ReturnsAsync(new List<string> { "Salesman" });

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SendToSalesmanAsync(1, "user-id"));
        }

        [Fact]
        public async Task RecordClientResponse_Accepted_ShouldChangeStatusToAccepted()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Sent",
                ClientId = 1,
                TotalAmount = 50000m,
                AssignedTo = "user-id" // Must match userId
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            mockSalesOfferRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesOffer>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesOffer o, CancellationToken ct) => o);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            _mockUnitOfWork.Setup(u => u.SalesDeals.GetQueryable()).Returns(new List<SalesDeal>().AsQueryable());
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var user = new ApplicationUser { Id = "user-id", UserName = "user" };
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act
            await service.RecordClientResponseAsync(1, "Client accepted", true, "user-id");

            // Assert
            Assert.Equal("Accepted", offer.Status);
        }

        [Fact]
        public async Task RecordClientResponse_Accepted_ShouldAutoCreateDeal()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Sent",
                ClientId = 1,
                TotalAmount = 50000m,
                PaymentTerms = "[]",
                DeliveryTerms = "[]",
                AssignedTo = "user-id" // Must match userId
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            mockSalesOfferRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesOffer>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesOffer o, CancellationToken ct) => o);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockDealRepo = new Mock<ISalesDealRepository>();
            var emptyDealsQuery = new List<SalesDeal>().AsQueryable();
            mockDealRepo.Setup(r => r.GetQueryable()).Returns(emptyDealsQuery);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            _mockUnitOfWork.Setup(u => u.SalesDeals.GetQueryable()).Returns(emptyDealsQuery);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var user = new ApplicationUser { Id = "user-id", UserName = "user" };
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var dealResponseDto = new DealResponseDTO
            {
                Id = 1,
                Status = "PendingManagerApproval"
            };
            _mockDealService.Setup(s => s.CreateDealAsync(It.IsAny<CreateDealDTO>(), It.IsAny<string>()))
                .ReturnsAsync(dealResponseDto);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act
            var result = await service.RecordClientResponseAsync(1, "Client accepted", true, "user-id");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Accepted", result.Status);
            _mockDealService.Verify(s => s.CreateDealAsync(
                It.Is<CreateDealDTO>(d => d.OfferId == 1 && d.ClientId == 1 && d.DealValue == 50000m),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RecordClientResponse_Rejected_ShouldChangeStatusToRejected()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Sent",
                ClientId = 1,
                AssignedTo = "user-id" // Must match userId
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            mockSalesOfferRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesOffer>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesOffer o, CancellationToken ct) => o);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var user = new ApplicationUser { Id = "user-id", UserName = "user" };
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act
            await service.RecordClientResponseAsync(1, "Client rejected", false, "user-id");

            // Assert
            Assert.Equal("Rejected", offer.Status);
            _mockDealService.Verify(s => s.CreateDealAsync(It.IsAny<CreateDealDTO>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task MarkAsNeedsModification_ShouldChangeStatusToNeedsModification()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Sent",
                ClientId = 1
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            mockSalesOfferRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesOffer>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesOffer o, CancellationToken ct) => o);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var user = new ApplicationUser { Id = "user-id", UserName = "user" };
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "SalesManager" });
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act
            await service.MarkAsNeedsModificationAsync(1, "Modification needed", "user-id");

            // Assert
            Assert.Equal("NeedsModification", offer.Status);
        }

        [Fact]
        public async Task MarkAsUnderReview_ShouldChangeStatusToUnderReview()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Sent",
                ClientId = 1
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            mockSalesOfferRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesOffer>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesOffer o, CancellationToken ct) => o);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = offer.CreatedBy ?? offer.AssignedTo ?? "user-id", UserName = "user" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act
            await service.MarkAsUnderReviewAsync(1, "user-id");

            // Assert
            Assert.Equal("UnderReview", offer.Status);
        }

        [Fact]
        public async Task CompleteOffer_ShouldChangeStatusToCompleted()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Accepted",
                ClientId = 1
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            mockSalesOfferRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesOffer>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesOffer o, CancellationToken ct) => o);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = offer.CreatedBy ?? offer.AssignedTo ?? "user-id", UserName = "user" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act
            await service.CompleteOfferAsync(1, "Completed successfully", "user-id");

            // Assert
            Assert.Equal("Completed", offer.Status);
        }

        [Fact]
        public async Task CompleteOffer_ShouldThrowExceptionIfNotAccepted()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Sent", // Not accepted
                ClientId = 1
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);

            var service = new OfferService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockDealService.Object,
                _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CompleteOfferAsync(1, "Notes", "user-id"));
        }

        #endregion
    }
}

