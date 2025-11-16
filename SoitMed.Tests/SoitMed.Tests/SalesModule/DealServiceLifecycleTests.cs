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
    public class DealServiceLifecycleTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<DealService>> _mockLogger;

        public DealServiceLifecycleTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<DealService>>();
        }

        #region Deal Creation Tests

        [Fact]
        public async Task CreateDeal_ShouldCreateDealWithPendingManagerApprovalStatus()
        {
            // Arrange
            var offer = new SalesOffer
            {
                Id = 1,
                Status = "Accepted",
                ClientId = 1,
                TotalAmount = 50000m
            };

            var mockSalesOfferRepo = new Mock<ISalesOfferRepository>();
            mockSalesOfferRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);
            _mockUnitOfWork.Setup(u => u.SalesOffers).Returns(mockSalesOfferRepo.Object);

            var mockDealRepo = new Mock<ISalesDealRepository>();
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = offer.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = "salesman-id", UserName = "salesman" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var createDto = new CreateDealDTO
            {
                OfferId = 1,
                ClientId = 1,
                DealValue = 50000m
            };

            // Act
            var result = await service.CreateDealAsync(createDto, "salesman-id");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PendingManagerApproval", result.Status);
            mockDealRepo.Verify(r => r.CreateAsync(It.Is<SalesDeal>(d => d.Status == "PendingManagerApproval"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateDeal_ShouldThrowExceptionIfOfferNotAccepted()
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

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var createDto = new CreateDealDTO
            {
                OfferId = 1,
                ClientId = 1,
                DealValue = 50000m
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateDealAsync(createDto, "salesman-id"));
        }

        #endregion

        #region Manager Approval Tests

        [Fact]
        public async Task ManagerApproval_Approved_ShouldChangeStatusToPendingSuperAdminApproval()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "PendingManagerApproval",
                ClientId = 1,
                SalesmanId = "salesman-id"
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            mockDealRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesDeal>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesDeal d, CancellationToken ct) => d);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = deal.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = deal.SalesmanId, UserName = "salesman" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var approvalDto = new ApproveDealDTO
            {
                Approved = true,
                Comments = "Approved by manager"
            };

            // Act
            await service.ManagerApprovalAsync(1, approvalDto, "manager-id");

            // Assert
            Assert.Equal("PendingSuperAdminApproval", deal.Status);
            Assert.NotNull(deal.ManagerApprovedBy);
            Assert.NotNull(deal.ManagerApprovedAt);
        }

        [Fact]
        public async Task ManagerApproval_Rejected_ShouldChangeStatusToRejectedByManager()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "PendingManagerApproval",
                ClientId = 1,
                SalesmanId = "salesman-id"
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            mockDealRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesDeal>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesDeal d, CancellationToken ct) => d);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = deal.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = deal.SalesmanId, UserName = "salesman" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var approvalDto = new ApproveDealDTO
            {
                Approved = false,
                RejectionReason = "Money",
                Comments = "Rejected due to budget"
            };

            // Act
            await service.ManagerApprovalAsync(1, approvalDto, "manager-id");

            // Assert
            Assert.Equal("RejectedByManager", deal.Status);
            Assert.Equal("Money", deal.ManagerRejectionReason);
        }

        [Fact]
        public async Task ManagerApproval_ShouldThrowExceptionIfNotPendingManagerApproval()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "PendingSuperAdminApproval", // Wrong status
                ClientId = 1
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var approvalDto = new ApproveDealDTO { Approved = true };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ManagerApprovalAsync(1, approvalDto, "manager-id"));
        }

        #endregion

        #region SuperAdmin Approval Tests

        [Fact]
        public async Task SuperAdminApproval_Approved_ShouldChangeStatusToSentToLegal()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "PendingSuperAdminApproval",
                ClientId = 1,
                SalesmanId = "salesman-id"
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            mockDealRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesDeal>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesDeal d, CancellationToken ct) => d);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = deal.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = deal.SalesmanId, UserName = "salesman" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var approvalDto = new ApproveDealDTO
            {
                Approved = true,
                Comments = "Approved by super admin"
            };

            // Act
            await service.SuperAdminApprovalAsync(1, approvalDto, "superadmin-id");

            // Assert
            Assert.Equal("SentToLegal", deal.Status);
            Assert.NotNull(deal.SentToLegalAt);
            Assert.NotNull(deal.SuperAdminApprovedBy);
        }

        [Fact]
        public async Task SuperAdminApproval_Rejected_ShouldChangeStatusToRejectedBySuperAdmin()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "PendingSuperAdminApproval",
                ClientId = 1,
                SalesmanId = "salesman-id"
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            mockDealRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesDeal>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesDeal d, CancellationToken ct) => d);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = deal.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = deal.SalesmanId, UserName = "salesman" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var approvalDto = new ApproveDealDTO
            {
                Approved = false,
                RejectionReason = "CashFlow",
                Comments = "Rejected due to cash flow"
            };

            // Act
            await service.SuperAdminApprovalAsync(1, approvalDto, "superadmin-id");

            // Assert
            Assert.Equal("RejectedBySuperAdmin", deal.Status);
            Assert.Equal("CashFlow", deal.SuperAdminRejectionReason);
        }

        [Fact]
        public async Task SuperAdminApproval_ShouldThrowExceptionIfNotPendingSuperAdminApproval()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "PendingManagerApproval", // Wrong status
                ClientId = 1
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            var approvalDto = new ApproveDealDTO { Approved = true };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SuperAdminApprovalAsync(1, approvalDto, "superadmin-id"));
        }

        #endregion

        #region Completion Tests

        [Fact]
        public async Task CompleteDeal_ShouldChangeStatusToSuccess()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "SentToLegal",
                ClientId = 1,
                SalesmanId = "salesman-id"
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            mockDealRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesDeal>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesDeal d, CancellationToken ct) => d);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = deal.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = deal.SalesmanId, UserName = "salesman" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            // Act
            await service.MarkDealAsCompletedAsync(1, "Deal completed successfully", "user-id");

            // Assert
            Assert.Equal("Success", deal.Status);
            Assert.NotNull(deal.CompletedAt);
            Assert.Equal("Deal completed successfully", deal.CompletionNotes);
        }

        [Fact]
        public async Task FailDeal_ShouldChangeStatusToFailed()
        {
            // Arrange
            var deal = new SalesDeal
            {
                Id = 1,
                Status = "SentToLegal",
                ClientId = 1,
                SalesmanId = "salesman-id"
            };

            var mockDealRepo = new Mock<ISalesDealRepository>();
            mockDealRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(deal);
            mockDealRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesDeal>(), It.IsAny<CancellationToken>())).ReturnsAsync((SalesDeal d, CancellationToken ct) => d);
            _mockUnitOfWork.Setup(u => u.SalesDeals).Returns(mockDealRepo.Object);
            
            var mockClientRepo = new Mock<IClientRepository>();
            mockClientRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client { Id = deal.ClientId, Name = "Test Client" });
            _mockUnitOfWork.Setup(u => u.Clients).Returns(mockClientRepo.Object);
            
            var mockUserRepo = new Mock<IApplicationUserRepository>();
            mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApplicationUser { Id = deal.SalesmanId, UserName = "salesman" });
            _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new DealService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);

            // Act
            await service.MarkDealAsFailedAsync(1, "Deal failed due to client cancellation", "user-id");

            // Assert
            Assert.Equal("Failed", deal.Status);
            Assert.NotNull(deal.CompletedAt);
            Assert.Equal("Deal failed due to client cancellation", deal.CompletionNotes);
        }

        #endregion
    }
}

