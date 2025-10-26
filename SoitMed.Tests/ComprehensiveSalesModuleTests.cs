using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SoitMed.Controllers;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using SoitMed.Services;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SoitMed.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the entire sales module
    /// Tests all controllers, services, and business logic with various scenarios
    /// </summary>
    public class ComprehensiveSalesModuleTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        // Test user credentials
        private const string SUPER_ADMIN_EMAIL = "hemdan@hemdan.com";
        private const string SALES_MANAGER_EMAIL = "salesmanager@soitmed.com";
        private const string SALES_SUPPORT_EMAIL = "salessupport@soitmed.com";
        private const string SALESMAN_EMAIL = "ahmed@soitmed.com";

        public ComprehensiveSalesModuleTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        #region Test Data Setup

        private async Task<string> GetAuthTokenAsync(string email)
        {
            // Return the actual JWT token for the user
            return TestDataHelper.GetTokenForUser(email);
        }

        private CreateClientDTO CreateTestClient()
        {
            return new CreateClientDTO
            {
                Name = "Test Hospital",
                Type = "Hospital",
                Specialization = "General Medicine",
                Location = "Cairo",
                Phone = "01234567890",
                Email = "test@hospital.com",
                Address = "123 Test Street",
                City = "Cairo",
                Governorate = "Cairo",
                Status = "Potential",
                Priority = "High",
                AssignedTo = "ahmed@soitmed.com"
            };
        }

        private CreateWeeklyPlanDTO CreateTestWeeklyPlan()
        {
            return new CreateWeeklyPlanDTO
            {
                WeekStartDate = DateTime.Now.Date,
                WeekEndDate = DateTime.Now.Date.AddDays(6),
                Goals = "Complete client visits and follow-ups",
                Notes = "Focus on high-priority clients"
            };
        }

        private CreateTaskProgressDTO CreateTestTaskProgress()
        {
            return new CreateTaskProgressDTO
            {
                TaskId = 1,
                ProgressDate = DateTime.Now,
                ProgressType = "Visit",
                Description = "Initial client visit",
                Notes = "Client showed interest in our products",
                VisitResult = "Interested",
                NextStep = "NeedsOffer",
                NextFollowUpDate = DateTime.Now.AddDays(7)
            };
        }

        private CreateOfferRequestDTO CreateTestOfferRequest()
        {
            return new CreateOfferRequestDTO
            {
                ClientId = 1,
                RequestedProducts = "Medical Equipment Package A",
                SpecialNotes = "Client needs urgent delivery",
                Priority = "High"
            };
        }

        private CreateDealDTO CreateTestDeal()
        {
            return new CreateDealDTO
            {
                ClientId = 1,
                DealValue = 50000,
                DealDescription = "Medical Equipment Package A",
                ExpectedCloseDate = DateTime.Now.AddDays(30),
                Notes = "High-value deal with good potential"
            };
        }

        #endregion

        #region Client Controller Tests

        [Fact]
        public async Task ClientController_CreateClient_ShouldReturnSuccess()
        {
            // Arrange
            var clientDto = CreateTestClient();
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/client", clientDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Client created successfully", content);
        }

        [Fact]
        public async Task ClientController_GetClientProfile_ShouldReturnCompleteProfile()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/client/1/profile");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<ClientProfileDTO>(content, _jsonOptions);
            
            Assert.NotNull(profile);
            Assert.NotNull(profile.ClientInfo);
            Assert.NotNull(profile.AllVisits);
            Assert.NotNull(profile.AllOffers);
            Assert.NotNull(profile.AllDeals);
            Assert.NotNull(profile.Statistics);
        }

        [Fact]
        public async Task ClientController_GetClientProfile_UnauthorizedUser_ShouldReturnForbidden()
        {
            // Arrange
            var token = await GetAuthTokenAsync("unauthorized@test.com");
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/client/1/profile");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ClientController_UpdateClient_ShouldReturnSuccess()
        {
            // Arrange
            var clientDto = CreateTestClient();
            clientDto.Name = "Updated Hospital Name";
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsJsonAsync("/api/client/1", clientDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ClientController_DeleteClient_ShouldReturnSuccess()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/client/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        #endregion

        #region TaskProgress Controller Tests

        [Fact]
        public async Task TaskProgressController_CreateProgress_ShouldReturnSuccess()
        {
            // Arrange
            var progressDto = CreateTestTaskProgress();
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/taskprogress", progressDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TaskProgressResponseDTO>(content, _jsonOptions);
            
            Assert.NotNull(result);
            Assert.Equal(progressDto.Description, result.Description);
        }

        [Fact]
        public async Task TaskProgressController_CreateProgressWithOfferRequest_ShouldReturnSuccess()
        {
            // Arrange
            var progressWithOfferDto = new CreateTaskProgressWithOfferRequestDTO
            {
                TaskId = 1,
                ClientId = 1,
                ProgressDate = DateTime.Now,
                ProgressType = "Visit",
                Description = "Client visit with offer request",
                VisitResult = "Interested",
                NextStep = "NeedsOffer",
                RequestedProducts = "Medical Equipment Package B",
                SpecialNotes = "Urgent request"
            };
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/taskprogress/with-offer-request", progressWithOfferDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task TaskProgressController_GetProgressesByTask_ShouldReturnList()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/taskprogress/task/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var progresses = JsonSerializer.Deserialize<List<TaskProgressResponseDTO>>(content, _jsonOptions);
            
            Assert.NotNull(progresses);
        }

        [Fact]
        public async Task TaskProgressController_GetProgressesByClient_ShouldReturnList()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/taskprogress/client/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var progresses = JsonSerializer.Deserialize<List<TaskProgressResponseDTO>>(content, _jsonOptions);
            
            Assert.NotNull(progresses);
        }

        [Fact]
        public async Task TaskProgressController_UpdateProgress_ShouldReturnSuccess()
        {
            // Arrange
            var progressDto = CreateTestTaskProgress();
            progressDto.Description = "Updated visit description";
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsJsonAsync("/api/taskprogress/1", progressDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        #endregion

        #region OfferRequest Controller Tests

        [Fact]
        public async Task OfferRequestController_CreateOfferRequest_ShouldReturnSuccess()
        {
            // Arrange
            var offerRequestDto = CreateTestOfferRequest();
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/offerrequest", offerRequestDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OfferRequestResponseDTO>(content, _jsonOptions);
            
            Assert.NotNull(result);
            Assert.Equal(offerRequestDto.RequestedProducts, result.RequestedProducts);
        }

        [Fact]
        public async Task OfferRequestController_GetOfferRequests_ShouldReturnList()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/offerrequest");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var requests = JsonSerializer.Deserialize<List<OfferRequestResponseDTO>>(content, _jsonOptions);
            
            Assert.NotNull(requests);
        }

        [Fact]
        public async Task OfferRequestController_GetOfferRequestById_ShouldReturnSingle()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/offerrequest/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<OfferRequestResponseDTO>(content, _jsonOptions);
            
            Assert.NotNull(request);
        }

        [Fact]
        public async Task OfferRequestController_AssignToSupport_ShouldReturnSuccess()
        {
            // Arrange
            var assignDto = new AssignOfferRequestDTO
            {
                AssignedTo = SALES_SUPPORT_EMAIL
            };
            var token = await GetAuthTokenAsync(SALES_MANAGER_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/offerrequest/1/assign", assignDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task OfferRequestController_UpdateStatus_ShouldReturnSuccess()
        {
            // Arrange
            var statusDto = new UpdateOfferRequestStatusDTO
            {
                Status = "InProgress",
                Notes = "Working on the offer"
            };
            var token = await GetAuthTokenAsync(SALES_SUPPORT_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsJsonAsync("/api/offerrequest/1/status", statusDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        #endregion

        #region Deal Controller Tests

        [Fact]
        public async Task DealController_CreateDeal_ShouldReturnSuccess()
        {
            // Arrange
            var dealDto = CreateTestDeal();
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/deal", dealDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DealResponseDTO>(content, _jsonOptions);
            
            Assert.NotNull(result);
            Assert.Equal(dealDto.DealValue, result.DealValue);
        }

        [Fact]
        public async Task DealController_GetDeals_ShouldReturnList()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/deal");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var deals = JsonSerializer.Deserialize<List<DealResponseDTO>>(content, _jsonOptions);
            
            Assert.NotNull(deals);
        }

        [Fact]
        public async Task DealController_GetDealById_ShouldReturnSingle()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/deal/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var deal = JsonSerializer.Deserialize<DealResponseDTO>(content, _jsonOptions);
            
            Assert.NotNull(deal);
        }

        [Fact]
        public async Task DealController_GetDealsByClient_ShouldReturnList()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/deal/client/1");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var deals = JsonSerializer.Deserialize<List<DealResponseDTO>>(content, _jsonOptions);
            
            Assert.NotNull(deals);
        }

        [Fact]
        public async Task DealController_GetDealsBySalesman_ShouldReturnList()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALES_MANAGER_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/api/deal/salesman/{SALESMAN_EMAIL}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var deals = JsonSerializer.Deserialize<List<DealResponseDTO>>(content, _jsonOptions);
            
            Assert.NotNull(deals);
        }

        [Fact]
        public async Task DealController_ManagerApproval_ShouldReturnSuccess()
        {
            // Arrange
            var approvalDto = new ApproveDealDTO
            {
                IsApproved = true,
                Notes = "Approved by manager"
            };
            var token = await GetAuthTokenAsync(SALES_MANAGER_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/deal/1/manager-approval", approvalDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DealController_SuperAdminApproval_ShouldReturnSuccess()
        {
            // Arrange
            var approvalDto = new ApproveDealDTO
            {
                IsApproved = true,
                Notes = "Approved by super admin"
            };
            var token = await GetAuthTokenAsync(SUPER_ADMIN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/deal/1/superadmin-approval", approvalDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DealController_CompleteDeal_ShouldReturnSuccess()
        {
            // Arrange
            var completeDto = new CompleteDealDTO
            {
                CompletionNotes = "Deal completed successfully"
            };
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/deal/1/complete", completeDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DealController_FailDeal_ShouldReturnSuccess()
        {
            // Arrange
            var failDto = new FailDealDTO
            {
                FailureNotes = "Deal failed due to budget constraints"
            };
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/deal/1/fail", failDto);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        #endregion

        #region Authorization Tests

        [Theory]
        [InlineData(SUPER_ADMIN_EMAIL, "/api/client", "GET", true)]
        [InlineData(SALES_MANAGER_EMAIL, "/api/client", "GET", true)]
        [InlineData(SALESMAN_EMAIL, "/api/client", "GET", true)]
        [InlineData(SALES_SUPPORT_EMAIL, "/api/client", "GET", false)]
        [InlineData(SUPER_ADMIN_EMAIL, "/api/deal/1/manager-approval", "POST", true)]
        [InlineData(SALES_MANAGER_EMAIL, "/api/deal/1/manager-approval", "POST", true)]
        [InlineData(SALESMAN_EMAIL, "/api/deal/1/manager-approval", "POST", false)]
        [InlineData(SALES_SUPPORT_EMAIL, "/api/deal/1/manager-approval", "POST", false)]
        public async Task AuthorizationTests_ShouldEnforceRoleBasedAccess(
            string userEmail, string endpoint, string method, bool shouldSucceed)
        {
            // Arrange
            var token = await GetAuthTokenAsync(userEmail);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response;
            if (method == "GET")
            {
                response = await _client.GetAsync(endpoint);
            }
            else if (method == "POST")
            {
                var approvalDto = new ApproveDealDTO { IsApproved = true, Notes = "Test" };
                response = await _client.PostAsJsonAsync(endpoint, approvalDto);
            }
            else
            {
                throw new ArgumentException("Unsupported HTTP method");
            }

            // Assert
            if (shouldSucceed)
            {
                Assert.True(response.IsSuccessStatusCode);
            }
            else
            {
                Assert.True(response.StatusCode == System.Net.HttpStatusCode.Forbidden || 
                           response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
            }
        }

        #endregion

        #region Validation Tests

        [Fact]
        public async Task ClientController_CreateClient_InvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidClientDto = new CreateClientDTO
            {
                Name = "", // Invalid: empty name
                Type = "InvalidType", // Invalid type
                Email = "invalid-email" // Invalid email format
            };
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/client", invalidClientDto);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TaskProgressController_CreateProgress_InvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidProgressDto = new CreateTaskProgressDTO
            {
                TaskId = 0, // Invalid: zero task ID
                ProgressDate = DateTime.MinValue, // Invalid date
                ProgressType = "InvalidType" // Invalid progress type
            };
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/taskprogress", invalidProgressDto);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DealController_CreateDeal_InvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidDealDto = new CreateDealDTO
            {
                ClientId = 0, // Invalid: zero client ID
                DealValue = -1000, // Invalid: negative value
                DealDescription = "" // Invalid: empty description
            };
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/deal", invalidDealDto);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ClientController_GetClientProfile_NonExistentClient_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/client/99999/profile");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DealController_GetDeal_NonExistentDeal_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/deal/99999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TaskProgressController_UpdateProgress_NonExistentProgress_ShouldReturnNotFound()
        {
            // Arrange
            var progressDto = CreateTestTaskProgress();
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsJsonAsync("/api/taskprogress/99999", progressDto);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task SalesWorkflow_CompleteFlow_ShouldWorkEndToEnd()
        {
            // This test simulates a complete sales workflow from start to finish
            
            // 1. Create a client
            var clientDto = CreateTestClient();
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var clientResponse = await _client.PostAsJsonAsync("/api/client", clientDto);
            Assert.True(clientResponse.IsSuccessStatusCode);

            // 2. Create task progress
            var progressDto = CreateTestTaskProgress();
            var progressResponse = await _client.PostAsJsonAsync("/api/taskprogress", progressDto);
            Assert.True(progressResponse.IsSuccessStatusCode);

            // 3. Create offer request
            var offerRequestDto = CreateTestOfferRequest();
            var offerRequestResponse = await _client.PostAsJsonAsync("/api/offerrequest", offerRequestDto);
            Assert.True(offerRequestResponse.IsSuccessStatusCode);

            // 4. Assign offer request to sales support
            var assignDto = new AssignOfferRequestDTO { AssignedTo = SALES_SUPPORT_EMAIL };
            var managerToken = await GetAuthTokenAsync(SALES_MANAGER_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managerToken);
            
            var assignResponse = await _client.PostAsJsonAsync("/api/offerrequest/1/assign", assignDto);
            Assert.True(assignResponse.IsSuccessStatusCode);

            // 5. Update offer request status
            var statusDto = new UpdateOfferRequestStatusDTO { Status = "Ready", Notes = "Offer prepared" };
            var supportToken = await GetAuthTokenAsync(SALES_SUPPORT_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", supportToken);
            
            var statusResponse = await _client.PutAsJsonAsync("/api/offerrequest/1/status", statusDto);
            Assert.True(statusResponse.IsSuccessStatusCode);

            // 6. Create deal
            var dealDto = CreateTestDeal();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var dealResponse = await _client.PostAsJsonAsync("/api/deal", dealDto);
            Assert.True(dealResponse.IsSuccessStatusCode);

            // 7. Manager approval
            var managerApprovalDto = new ApproveDealDTO { IsApproved = true, Notes = "Approved by manager" };
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managerToken);
            
            var managerApprovalResponse = await _client.PostAsJsonAsync("/api/deal/1/manager-approval", managerApprovalDto);
            Assert.True(managerApprovalResponse.IsSuccessStatusCode);

            // 8. Super admin approval
            var superAdminApprovalDto = new ApproveDealDTO { IsApproved = true, Notes = "Approved by super admin" };
            var superAdminToken = await GetAuthTokenAsync(SUPER_ADMIN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", superAdminToken);
            
            var superAdminApprovalResponse = await _client.PostAsJsonAsync("/api/deal/1/superadmin-approval", superAdminApprovalDto);
            Assert.True(superAdminApprovalResponse.IsSuccessStatusCode);

            // 9. Complete deal
            var completeDto = new CompleteDealDTO { CompletionNotes = "Deal completed successfully" };
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var completeResponse = await _client.PostAsJsonAsync("/api/deal/1/complete", completeDto);
            Assert.True(completeResponse.IsSuccessStatusCode);

            // 10. Verify client profile shows all activities
            var profileResponse = await _client.GetAsync("/api/client/1/profile");
            Assert.True(profileResponse.IsSuccessStatusCode);
            
            var profileContent = await profileResponse.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<ClientProfileDTO>(profileContent, _jsonOptions);
            
            Assert.NotNull(profile);
            Assert.True(profile.AllVisits.Count > 0);
            Assert.True(profile.AllDeals.Count > 0);
            Assert.True(profile.Statistics.TotalVisits > 0);
            Assert.True(profile.Statistics.SuccessfulDeals > 0);
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task ClientController_GetClientProfile_Performance_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _client.GetAsync("/api/client/1/profile");
            stopwatch.Stop();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000); // Should complete within 5 seconds
        }

        [Fact]
        public async Task DealController_GetDeals_Performance_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var token = await GetAuthTokenAsync(SALESMAN_EMAIL);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _client.GetAsync("/api/deal");
            stopwatch.Stop();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(stopwatch.ElapsedMilliseconds < 3000); // Should complete within 3 seconds
        }

        #endregion
    }
}

