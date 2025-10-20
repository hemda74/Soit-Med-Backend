using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SoitMed.Models;
using SoitMed.Repositories;
using SoitMed.Tests.Helpers;

namespace SoitMed.Tests.ClientTracking
{
    public class ClientAnalyticsRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly ClientAnalyticsRepository _repository;

        public ClientAnalyticsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Context(options);
            _repository = new ClientAnalyticsRepository(_context);

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

            var analytics = new ClientAnalytics
            {
                Id = 1,
                ClientId = 1,
                Period = "monthly",
                PeriodStart = DateTime.UtcNow.AddDays(-30),
                PeriodEnd = DateTime.UtcNow,
                TotalVisits = 5,
                TotalInteractions = 8,
                TotalSales = 2,
                AverageVisitDuration = 120.5m,
                LastVisitDate = DateTime.UtcNow.AddDays(-2),
                NextScheduledVisit = DateTime.UtcNow.AddDays(3),
                ClientSatisfactionScore = 4.5m,
                ConversionRate = 0.4m,
                Revenue = 50000m,
                GrowthRate = 0.15m,
                TopProducts = "[\"Product A\", \"Product B\"]",
                KeyMetrics = "{\"visitFrequency\": 2.5, \"responseTime\": 24, \"issueResolutionRate\": 0.9}",
                CreatedAt = DateTime.UtcNow
            };

            _context.ClientAnalytics.Add(analytics);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetClientAnalyticsAsync_ShouldReturnAnalyticsForClient()
        {
            // Act
            var analytics = await _repository.GetClientAnalyticsAsync(1, "monthly");

            // Assert
            Assert.NotNull(analytics);
            Assert.Equal(1, analytics.ClientId);
            Assert.Equal("monthly", analytics.Period);
            Assert.Equal(5, analytics.TotalVisits);
            Assert.Equal(8, analytics.TotalInteractions);
        }

        [Fact]
        public async Task GetClientAnalyticsAsync_WithNonExistentClient_ShouldReturnNull()
        {
            // Act
            var analytics = await _repository.GetClientAnalyticsAsync(999, "monthly");

            // Assert
            Assert.Null(analytics);
        }

        [Fact]
        public async Task GetClientAnalyticsAsync_WithNonExistentPeriod_ShouldReturnNull()
        {
            // Act
            var analytics = await _repository.GetClientAnalyticsAsync(1, "yearly");

            // Assert
            Assert.Null(analytics);
        }

        [Fact]
        public async Task GetClientAnalyticsHistoryAsync_ShouldReturnAnalyticsHistory()
        {
            // Act
            var analyticsHistory = await _repository.GetClientAnalyticsHistoryAsync(1);

            // Assert
            Assert.NotNull(analyticsHistory);
            Assert.Single(analyticsHistory);
        }

        [Fact]
        public async Task GetClientAnalyticsHistoryAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Act
            var analyticsHistory = await _repository.GetClientAnalyticsHistoryAsync(1, page: 1, pageSize: 1);

            // Assert
            Assert.NotNull(analyticsHistory);
            Assert.Single(analyticsHistory);
        }

        [Fact]
        public async Task GetClientSummaryAsync_ShouldReturnClientSummary()
        {
            // Act
            var summary = await _repository.GetClientSummaryAsync(1);

            // Assert
            Assert.NotNull(summary);
            
            // Use reflection to check properties since we're returning anonymous object
            var summaryType = summary.GetType();
            var clientIdProperty = summaryType.GetProperty("ClientId");
            var clientNameProperty = summaryType.GetProperty("ClientName");
            var totalVisitsProperty = summaryType.GetProperty("TotalVisits");

            Assert.NotNull(clientIdProperty);
            Assert.NotNull(clientNameProperty);
            Assert.NotNull(totalVisitsProperty);
        }

        [Fact]
        public async Task GetClientSummaryAsync_WithNonExistentClient_ShouldReturnNull()
        {
            // Act
            var summary = await _repository.GetClientSummaryAsync(999);

            // Assert
            Assert.Null(summary);
        }

        [Fact]
        public async Task GetClientTimelineAsync_ShouldReturnTimeline()
        {
            // Arrange
            var visit = new ClientVisit
            {
                Id = 1,
                ClientId = 1,
                VisitDate = DateTime.UtcNow.AddDays(-2),
                VisitType = "Initial",
                Purpose = "Test visit",
                Status = "Completed",
                SalesmanId = "test-salesman",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var interaction = new ClientInteraction
            {
                Id = 1,
                ClientId = 1,
                InteractionDate = DateTime.UtcNow.AddDays(-1),
                InteractionType = "Call",
                Subject = "Test call",
                Status = "Closed",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.ClientVisits.Add(visit);
            _context.ClientInteractions.Add(interaction);
            await _context.SaveChangesAsync();

            // Act
            var timeline = await _repository.GetClientTimelineAsync(1);

            // Assert
            Assert.NotNull(timeline);
            
            // Use reflection to check the timeline structure
            var timelineType = timeline.GetType();
            Assert.True(timelineType.IsGenericType);
        }

        [Fact]
        public async Task GetClientTimelineAsync_WithLimit_ShouldRespectLimit()
        {
            // Act
            var timeline = await _repository.GetClientTimelineAsync(1, limit: 1);

            // Assert
            Assert.NotNull(timeline);
        }

        [Fact]
        public async Task UpdateClientAnalyticsAsync_ShouldCreateNewAnalytics()
        {
            // Arrange
            var clientId = 2;
            var client = new Client
            {
                Id = clientId,
                Name = "Test Client 2",
                Type = "Hospital",
                Status = "Active",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Act
            await _repository.UpdateClientAnalyticsAsync(clientId, "monthly");

            // Assert
            var analytics = await _repository.GetClientAnalyticsAsync(clientId, "monthly");
            Assert.NotNull(analytics);
            Assert.Equal(clientId, analytics.ClientId);
            Assert.Equal("monthly", analytics.Period);
        }

        [Fact]
        public async Task UpdateClientAnalyticsAsync_ShouldUpdateExistingAnalytics()
        {
            // Arrange
            var clientId = 1;
            var period = "monthly";

            // Act
            await _repository.UpdateClientAnalyticsAsync(clientId, period);

            // Assert
            var analytics = await _repository.GetClientAnalyticsAsync(clientId, period);
            Assert.NotNull(analytics);
            Assert.Equal(clientId, analytics.ClientId);
        }

        [Fact]
        public async Task GetClientAnalyticsHistoryAsync_ShouldOrderByPeriodStartDescending()
        {
            // Arrange
            var additionalAnalytics = new ClientAnalytics
            {
                Id = 2,
                ClientId = 1,
                Period = "weekly",
                PeriodStart = DateTime.UtcNow.AddDays(-7),
                PeriodEnd = DateTime.UtcNow,
                TotalVisits = 2,
                TotalInteractions = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            };

            _context.ClientAnalytics.Add(additionalAnalytics);
            await _context.SaveChangesAsync();

            // Act
            var analyticsHistory = await _repository.GetClientAnalyticsHistoryAsync(1);

            // Assert
            Assert.NotNull(analyticsHistory);
            var analyticsList = analyticsHistory.ToList();
            Assert.True(analyticsList[0].PeriodStart >= analyticsList[1].PeriodStart);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}