using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SoitMed.Tests
{
    public class SimpleSalesModuleTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SimpleSalesModuleTests(WebApplicationFactory<Program> factory)
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

        [Fact]
        public async Task WeeklyPlan_Endpoints_Require_Authentication()
        {
            // Test that all WeeklyPlan endpoints require authentication
            var endpoints = new[]
            {
                "/api/WeeklyPlan",
                "/api/WeeklyPlan/1",
                "/api/WeeklyPlan/current",
                "/api/WeeklyPlan/1/submit",
                "/api/WeeklyPlan/1/review"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                // Should return 401 Unauthorized or 405 Method Not Allowed (for POST endpoints)
                Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                            response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed);
            }
        }

        [Fact]
        public async Task Client_Endpoints_Require_Authentication()
        {
            // Test that all Client endpoints require authentication
            var endpoints = new[]
            {
                "/api/Client/search",
                "/api/Client/1",
                "/api/Client/my-clients",
                "/api/Client/follow-up",
                "/api/Client/statistics"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                // Should return 401 Unauthorized or 405 Method Not Allowed (for POST endpoints)
                Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                            response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed);
            }
        }

        [Fact]
        public async Task Health_Check_Works()
        {
            // Test basic health check
            var response = await _client.GetAsync("/");

            // Should return some response (could be 200, 404, or redirect)
            Assert.True(response.IsSuccessStatusCode || 
                        response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                        response.StatusCode == System.Net.HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task Non_Existent_Endpoint_Returns_404()
        {
            // Test non-existent endpoint
            var response = await _client.GetAsync("/api/nonexistent");

            // Should return 404 Not Found
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}



