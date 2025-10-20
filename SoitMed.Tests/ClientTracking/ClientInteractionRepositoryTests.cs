using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SoitMed.Models;
using SoitMed.Repositories;
using SoitMed.Tests.Helpers;

namespace SoitMed.Tests.ClientTracking
{
    public class ClientInteractionRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly ClientInteractionRepository _repository;

        public ClientInteractionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Context(options);
            _repository = new ClientInteractionRepository(_context);

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

            var interactions = new List<ClientInteraction>
            {
                new ClientInteraction
                {
                    Id = 1,
                    ClientId = 1,
                    InteractionDate = DateTime.UtcNow.AddDays(-5),
                    InteractionType = "Call",
                    Subject = "Initial discussion",
                    Description = "Initial phone call to discuss requirements",
                    Status = "Closed",
                    CreatedBy = "test-user",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new ClientInteraction
                {
                    Id = 2,
                    ClientId = 1,
                    InteractionDate = DateTime.UtcNow.AddDays(-3),
                    InteractionType = "Email",
                    Subject = "Proposal follow-up",
                    Description = "Sent proposal via email",
                    Status = "Closed",
                    CreatedBy = "test-user",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new ClientInteraction
                {
                    Id = 3,
                    ClientId = 1,
                    InteractionDate = DateTime.UtcNow.AddDays(-1),
                    InteractionType = "Meeting",
                    Subject = "Contract discussion",
                    Description = "In-person meeting to discuss contract details",
                    FollowUpRequired = true,
                    FollowUpDate = DateTime.UtcNow.AddDays(2),
                    Status = "Open",
                    CreatedBy = "test-user",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _context.ClientInteractions.AddRange(interactions);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetClientInteractionsAsync_ShouldReturnInteractionsForClient()
        {
            // Act
            var interactions = await _repository.GetClientInteractionsAsync(1);

            // Assert
            Assert.NotNull(interactions);
            Assert.Equal(3, interactions.Count());
        }

        [Fact]
        public async Task GetClientInteractionsAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Act
            var interactions = await _repository.GetClientInteractionsAsync(1, page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(interactions);
            Assert.Equal(2, interactions.Count());
        }

        [Fact]
        public async Task GetInteractionsByTypeAsync_ShouldReturnFilteredInteractions()
        {
            // Act
            var interactions = await _repository.GetInteractionsByTypeAsync(1, "Call");

            // Assert
            Assert.NotNull(interactions);
            Assert.Single(interactions);
            Assert.Equal("Call", interactions.First().InteractionType);
        }

        [Fact]
        public async Task GetPendingFollowUpsAsync_ShouldReturnInteractionsNeedingFollowUp()
        {
            // Act
            var interactions = await _repository.GetPendingFollowUpsAsync(1);

            // Assert
            Assert.NotNull(interactions);
            Assert.Single(interactions);
            Assert.True(interactions.First().FollowUpRequired);
            Assert.Equal("Open", interactions.First().Status);
        }

        [Fact]
        public async Task GetPendingFollowUpsAsync_WithNoPendingFollowUps_ShouldReturnEmpty()
        {
            // Act
            var interactions = await _repository.GetPendingFollowUpsAsync(999);

            // Assert
            Assert.NotNull(interactions);
            Assert.Empty(interactions);
        }

        [Fact]
        public async Task GetAverageInteractionDurationAsync_ShouldReturnZeroForNoInteractions()
        {
            // Act
            var average = await _repository.GetAverageInteractionDurationAsync(999);

            // Assert
            Assert.Equal(0, average);
        }

        [Fact]
        public async Task GetTotalInteractionsCountAsync_ShouldReturnCorrectCount()
        {
            // Act
            var count = await _repository.GetTotalInteractionsCountAsync(1);

            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task GetLastInteractionDateAsync_ShouldReturnMostRecentInteraction()
        {
            // Act
            var lastInteractionDate = await _repository.GetLastInteractionDateAsync(1);

            // Assert
            Assert.NotNull(lastInteractionDate);
            Assert.Equal(DateTime.UtcNow.AddDays(-1).Date, lastInteractionDate.Value.Date);
        }

        [Fact]
        public async Task GetLastInteractionDateAsync_WithNoInteractions_ShouldReturnNull()
        {
            // Act
            var lastInteractionDate = await _repository.GetLastInteractionDateAsync(999);

            // Assert
            Assert.Null(lastInteractionDate);
        }

        [Fact]
        public async Task GetClientInteractionsAsync_ShouldOrderByDateDescending()
        {
            // Act
            var interactions = await _repository.GetClientInteractionsAsync(1);

            // Assert
            Assert.NotNull(interactions);
            var interactionsList = interactions.ToList();
            Assert.True(interactionsList[0].InteractionDate >= interactionsList[1].InteractionDate);
            Assert.True(interactionsList[1].InteractionDate >= interactionsList[2].InteractionDate);
        }

        [Fact]
        public async Task GetInteractionsByTypeAsync_WithNonExistentType_ShouldReturnEmpty()
        {
            // Act
            var interactions = await _repository.GetInteractionsByTypeAsync(1, "NonExistent");

            // Assert
            Assert.NotNull(interactions);
            Assert.Empty(interactions);
        }

        [Fact]
        public async Task GetPendingFollowUpsAsync_ShouldOnlyReturnOpenStatus()
        {
            // Arrange
            var closedInteraction = new ClientInteraction
            {
                Id = 4,
                ClientId = 1,
                InteractionDate = DateTime.UtcNow.AddDays(-2),
                InteractionType = "Call",
                Subject = "Closed interaction",
                FollowUpRequired = true,
                FollowUpDate = DateTime.UtcNow.AddDays(1),
                Status = "Closed",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            _context.ClientInteractions.Add(closedInteraction);
            await _context.SaveChangesAsync();

            // Act
            var interactions = await _repository.GetPendingFollowUpsAsync(1);

            // Assert
            Assert.NotNull(interactions);
            Assert.Single(interactions);
            Assert.Equal("Open", interactions.First().Status);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}