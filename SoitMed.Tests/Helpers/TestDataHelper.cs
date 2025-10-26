using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Security.Claims;

namespace SoitMed.Tests.Helpers
{
    /// <summary>
    /// Helper class for setting up test data and authentication
    /// </summary>
    public static class TestDataHelper
    {
        // Test user credentials
        public const string SUPER_ADMIN_EMAIL = "hemdan@hemdan.com";
        public const string SALES_MANAGER_EMAIL = "salesmanager@soitmed.com";
        public const string SALES_SUPPORT_EMAIL = "salessupport@soitmed.com";
        public const string SALESMAN_EMAIL = "ahmed@soitmed.com";

        // JWT Tokens for testing
        public const string SUPER_ADMIN_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaGVtZGFuQGhlbWRhbi5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IkhlbWRhbl9UZXN0X0FkbWluaXN0cmF0aW9uXzAwMiIsImp0aSI6IjgyZjFiOGRiLWY3MWQtNDQxZi04NTc5LTUyZGI5N2Y0NzhmNSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN1cGVyQWRtaW4iLCJleHAiOjE5MTg5ODk3OTcsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTExNyIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDIwMCJ9.t8vWVolbjc7EgKiv1fd6kujXEPAh5hMMVAk92bOvXCg";
        
        public const string SALES_MANAGER_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2FsZXNtYW5hZ2VyQHNvaXRtZWQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJBaG1lZF9IZW1kYW5fU2FsZXNfMDAyIiwianRpIjoiNmZhMzU2ZjQtZWQ0My00YmJhLWJlM2UtNjcxZWEyNmYyMWJmIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU2FsZXNNYW5hZ2VyIiwiZXhwIjoxOTE4OTg5NjQ3LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUxMTciLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDAifQ.8SlY9_eVlX6EenY_nmiTWcfvq0QfR-iyT2zhNQHqxws";
        
        public const string SALES_SUPPORT_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2FsZXNzdXBwb3J0QHNvaXRtZWQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJBaG1lZF9IZW1kYW5fRW5naW5lZXJpbmdfMDAxIiwianRpIjoiMzU2NGZkNWMtMmIwZC00MGE5LWFkZTUtNTEwZWY3YzA1OGZmIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU2FsZXNTdXBwb3J0IiwiZXhwIjoxOTE4OTg5ODQxLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUxMTciLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDAifQ.wppWw1Zm2qEJ6Ozg2z1ceDbcbGUR1IUeQVwSZQP0lZM";
        
        public const string SALESMAN_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWhtZWRAc29pdG1lZC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IkFobWVkX0FzaHJhZl9TYWxlc18wMDEiLCJqdGkiOiJmYjU5Nzc3Ny05MzVlLTRjNzctOGFlMi05MWJmMTAxN2M2NWEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTYWxlc21hbiIsImV4cCI6MTkxODk4OTg2OSwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MTE3IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo0MjAwIn0.wAFrUdA2SH5Elk3VGW7DfJA70D1geFGX9EJjx4az9Do";

        public static async Task<Context> CreateTestContextAsync()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new Context(options);
            await SeedTestDataAsync(context);
            return context;
        }

        public static async Task SeedTestDataAsync(Context context)
        {
            // Create test users
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    UserName = SUPER_ADMIN_EMAIL,
                    Email = SUPER_ADMIN_EMAIL,
                    FirstName = "Hemdan",
                    LastName = "Test",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    Id = "2",
                    UserName = SALES_MANAGER_EMAIL,
                    Email = SALES_MANAGER_EMAIL,
                    FirstName = "Sales",
                    LastName = "Manager",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    Id = "3",
                    UserName = SALES_SUPPORT_EMAIL,
                    Email = SALES_SUPPORT_EMAIL,
                    FirstName = "Sales",
                    LastName = "Support",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    Id = "4",
                    UserName = SALESMAN_EMAIL,
                    Email = SALESMAN_EMAIL,
                    FirstName = "Ahmed",
                    LastName = "Salesman",
                    EmailConfirmed = true
                }
            };

            context.Users.AddRange(users);

            // Create test clients
            var clients = new List<Client>
            {
                new Client
                {
                    Id = 1,
                    Name = "Test Hospital 1",
                    Type = "Hospital",
                    Specialization = "General Medicine",
                    Location = "Cairo",
                    Phone = "01234567890",
                    Email = "test1@hospital.com",
                    Status = "Potential",
                    Priority = "High",
                    AssignedTo = SALESMAN_EMAIL,
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Client
                {
                    Id = 2,
                    Name = "Test Hospital 2",
                    Type = "Hospital",
                    Specialization = "Cardiology",
                    Location = "Alexandria",
                    Phone = "01234567891",
                    Email = "test2@hospital.com",
                    Status = "Active",
                    Priority = "Medium",
                    AssignedTo = SALESMAN_EMAIL,
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.Clients.AddRange(clients);

            // Create test weekly plans
            var weeklyPlans = new List<WeeklyPlan>
            {
                new WeeklyPlan
                {
                    Id = 1,
                    WeekStartDate = DateTime.Now.Date,
                    WeekEndDate = DateTime.Now.Date.AddDays(6),
                    Goals = "Complete client visits",
                    Notes = "Focus on high-priority clients",
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.WeeklyPlans.AddRange(weeklyPlans);

            // Create test weekly plan tasks
            var tasks = new List<WeeklyPlanTask>
            {
                new WeeklyPlanTask
                {
                    Id = 1,
                    WeeklyPlanId = 1,
                    TaskName = "Visit Test Hospital 1",
                    Description = "Initial client visit",
                    Priority = "High",
                    Status = "Pending",
                    DueDate = DateTime.Now.AddDays(1),
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new WeeklyPlanTask
                {
                    Id = 2,
                    WeeklyPlanId = 1,
                    TaskName = "Follow up with Test Hospital 2",
                    Description = "Follow up on previous discussion",
                    Priority = "Medium",
                    Status = "Pending",
                    DueDate = DateTime.Now.AddDays(2),
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.WeeklyPlanTasks.AddRange(tasks);

            // Create test task progresses
            var taskProgresses = new List<TaskProgress>
            {
                new TaskProgress
                {
                    Id = 1,
                    TaskId = 1,
                    EmployeeId = SALESMAN_EMAIL,
                    ProgressDate = DateTime.Now,
                    ProgressType = "Visit",
                    Description = "Initial client visit completed",
                    Notes = "Client showed interest",
                    VisitResult = "Interested",
                    NextStep = "NeedsOffer",
                    NextFollowUpDate = DateTime.Now.AddDays(7),
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.TaskProgresses.AddRange(taskProgresses);

            // Create test offer requests
            var offerRequests = new List<OfferRequest>
            {
                new OfferRequest
                {
                    Id = 1,
                    ClientId = 1,
                    RequestedBy = SALESMAN_EMAIL,
                    RequestedProducts = "Medical Equipment Package A",
                    SpecialNotes = "Urgent request",
                    Priority = "High",
                    Status = "Requested",
                    RequestDate = DateTime.Now,
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.OfferRequests.AddRange(offerRequests);

            // Create test sales offers
            var salesOffers = new List<SalesOffer>
            {
                new SalesOffer
                {
                    Id = 1,
                    ClientId = 1,
                    OfferRequestId = 1,
                    OfferTitle = "Medical Equipment Package A",
                    OfferDescription = "Complete medical equipment package",
                    OfferValue = 45000,
                    ValidUntil = DateTime.Now.AddDays(30),
                    Status = "Active",
                    CreatedBy = SALES_SUPPORT_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.SalesOffers.AddRange(salesOffers);

            // Create test sales deals
            var salesDeals = new List<SalesDeal>
            {
                new SalesDeal
                {
                    Id = 1,
                    ClientId = 1,
                    SalesmanId = SALESMAN_EMAIL,
                    DealValue = 50000,
                    DealDescription = "Medical Equipment Package A",
                    ExpectedCloseDate = DateTime.Now.AddDays(30),
                    Status = "PendingManagerApproval",
                    Notes = "High-value deal",
                    CreatedBy = SALESMAN_EMAIL,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.SalesDeals.AddRange(salesDeals);

            await context.SaveChangesAsync();
        }

        public static ClaimsPrincipal CreateTestUser(string email, string role = "Salesman")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(identity);
        }

        public static string GetTestUserRole(string email)
        {
            return email switch
            {
                SUPER_ADMIN_EMAIL => "SuperAdmin",
                SALES_MANAGER_EMAIL => "SalesManager",
                SALES_SUPPORT_EMAIL => "SalesSupport",
                SALESMAN_EMAIL => "Salesman",
                _ => "Salesman"
            };
        }

        public static string GetTokenForUser(string userEmail)
        {
            return userEmail switch
            {
                SUPER_ADMIN_EMAIL => SUPER_ADMIN_TOKEN,
                SALES_MANAGER_EMAIL => SALES_MANAGER_TOKEN,
                SALES_SUPPORT_EMAIL => SALES_SUPPORT_TOKEN,
                SALESMAN_EMAIL => SALESMAN_TOKEN,
                _ => SALESMAN_TOKEN // Default to salesman token
            };
        }

        public static bool HasPermission(string userEmail, string permission)
        {
            var role = GetTestUserRole(userEmail);
            
            return permission switch
            {
                "ViewAllClients" => role is "SuperAdmin" or "SalesManager",
                "ViewOwnClients" => role is "Salesman" or "SalesManager" or "SuperAdmin",
                "CreateClient" => role is "Salesman" or "SalesManager" or "SuperAdmin",
                "EditClient" => role is "Salesman" or "SalesManager" or "SuperAdmin",
                "ViewAllDeals" => role is "SuperAdmin" or "SalesManager",
                "ViewOwnDeals" => role is "Salesman" or "SalesManager" or "SuperAdmin",
                "CreateDeal" => role is "Salesman" or "SalesManager" or "SuperAdmin",
                "ApproveManagerDeal" => role is "SalesManager" or "SuperAdmin",
                "ApproveSuperAdminDeal" => role is "SuperAdmin",
                "ViewAllOffers" => role is "SuperAdmin" or "SalesManager",
                "ViewOwnOffers" => role is "Salesman" or "SalesManager" or "SuperAdmin",
                "CreateOffer" => role is "SalesSupport" or "SalesManager" or "SuperAdmin",
                "RequestOffer" => role is "Salesman" or "SalesManager" or "SuperAdmin",
                _ => false
            };
        }
    }
}


