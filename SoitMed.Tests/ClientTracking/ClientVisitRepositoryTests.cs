using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SoitMed.Models;
using SoitMed.Repositories;
using SoitMed.Tests.Helpers;

namespace SoitMed.Tests.ClientTracking
{
    public class ClientVisitRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly ClientVisitRepository _repository;

        public ClientVisitRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Context(options);
            _repository = new ClientVisitRepository(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var client = new Client
            {
                Id = 1,
                Name = "Test Client",
                Type = "Doctor",
                Status = "Active",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow
            };

            _context.Clients.Add(client);

            var visits = new List<ClientVisit>
            {
                new ClientVisit
                {
                    Id = 1,
                    ClientId = 1,
                    VisitDate = DateTime.UtcNow.AddDays(-5),
                    VisitType = "Initial",
                    Purpose = "Initial consultation",
                    Status = "Completed",
                    SalesmanId = "salesman-1",
                    CreatedBy = "test-user",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new ClientVisit
                {
                    Id = 2,
                    ClientId = 1,
                    VisitDate = DateTime.UtcNow.AddDays(-2),
                    VisitType = "Follow-up",
                    Purpose = "Follow-up meeting",
                    Status = "Completed",
                    SalesmanId = "salesman-1",
                    CreatedBy = "test-user",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new ClientVisit
                {
                    Id = 3,
                    ClientId = 1,
                    VisitDate = DateTime.UtcNow.AddDays(2),
                    VisitType = "Maintenance",
                    Purpose = "Scheduled maintenance",
                    Status = "Scheduled",
                    SalesmanId = "salesman-1",
                    CreatedBy = "test-user",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.ClientVisits.AddRange(visits);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetClientVisitsAsync_ShouldReturnVisitsForClient()
        {
            // Act
            var visits = await _repository.GetClientVisitsAsync(1);

            // Assert
            Assert.NotNull(visits);
            Assert.Equal(3, visits.Count());
        }

        [Fact]
        public async Task GetClientVisitsAsync_WithDateFilter_ShouldReturnFilteredVisits()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-3);
            var endDate = DateTime.UtcNow.AddDays(1);

            // Act
            var visits = await _repository.GetClientVisitsAsync(1, startDate, endDate);

            // Assert
            Assert.NotNull(visits);
            Assert.Equal(2, visits.Count());
            Assert.All(visits, v => Assert.True(v.VisitDate >= startDate && v.VisitDate <= endDate));
        }

        [Fact]
        public async Task GetClientVisitsAsync_WithStatusFilter_ShouldReturnFilteredVisits()
        {
            // Act
            var visits = await _repository.GetClientVisitsAsync(1, status: "Completed");

            // Assert
            Assert.NotNull(visits);
            Assert.Equal(2, visits.Count());
            Assert.All(visits, v => Assert.Equal("Completed", v.Status));
        }

        [Fact]
        public async Task GetSalesmanVisitsAsync_ShouldReturnVisitsForSalesman()
        {
            // Act
            var visits = await _repository.GetSalesmanVisitsAsync("salesman-1");

            // Assert
            Assert.NotNull(visits);
            Assert.Equal(3, visits.Count());
        }

        [Fact]
        public async Task GetAverageVisitDurationAsync_ShouldReturnZeroForNoVisits()
        {
            // Act
            var average = await _repository.GetAverageVisitDurationAsync(999);

            // Assert
            Assert.Equal(0, average);
        }

        [Fact]
        public async Task GetTotalVisitsCountAsync_ShouldReturnCorrectCount()
        {
            // Act
            var count = await _repository.GetTotalVisitsCountAsync(1);

            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task GetLastVisitDateAsync_ShouldReturnMostRecentVisit()
        {
            // Act
            var lastVisitDate = await _repository.GetLastVisitDateAsync(1);

            // Assert
            Assert.NotNull(lastVisitDate);
            Assert.Equal(DateTime.UtcNow.AddDays(-2).Date, lastVisitDate.Value.Date);
        }

        [Fact]
        public async Task GetNextScheduledVisitAsync_ShouldReturnScheduledVisit()
        {
            // Act
            var nextVisit = await _repository.GetNextScheduledVisitAsync(1);

            // Assert
            Assert.NotNull(nextVisit);
            Assert.Equal(DateTime.UtcNow.AddDays(2).Date, nextVisit.Value.Date);
        }

        [Fact]
        public async Task GetNextScheduledVisitAsync_WithNoScheduledVisits_ShouldReturnNull()
        {
            // Act
            var nextVisit = await _repository.GetNextScheduledVisitAsync(999);

            // Assert
            Assert.Null(nextVisit);
        }

        [Fact]
        public async Task GetClientVisitsAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Act
            var visits = await _repository.GetClientVisitsAsync(1, page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(visits);
            Assert.Equal(2, visits.Count());
        }

        [Fact]
        public async Task GetClientVisitsAsync_WithSalesmanFilter_ShouldReturnFilteredVisits()
        {
            // Act
            var visits = await _repository.GetClientVisitsAsync(1, salesmanId: "salesman-1");

            // Assert
            Assert.NotNull(visits);
            Assert.Equal(3, visits.Count());
            Assert.All(visits, v => Assert.Equal("salesman-1", v.SalesmanId));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}