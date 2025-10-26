using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SoitMed.Tests
{
    public class SalesModuleIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SalesModuleIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Application_Starts_Successfully()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/swagger");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Swagger_UI_Is_Accessible()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/swagger");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task WeeklyPlan_Controller_Exists()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/WeeklyPlan");

            // Assert
            // Should return 401 Unauthorized (not 404 Not Found) because controller exists but requires authentication
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                       response.StatusCode == System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Client_Controller_Exists()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/Client");

            // Assert
            // Should return 401 Unauthorized (not 404 Not Found) because controller exists but requires authentication
            // Or 200 OK if no authentication is required in test environment
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                       response.StatusCode == System.Net.HttpStatusCode.OK ||
                       response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed);
        }
    }
}
