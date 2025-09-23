using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;

namespace SoitMed.Services
{
    public class FinanceSalesReportSeedingService
    {
        private readonly ISalesReportService _salesReportService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;

        public FinanceSalesReportSeedingService(
            ISalesReportService salesReportService,
            UserManager<ApplicationUser> userManager,
            Context context)
        {
            _salesReportService = salesReportService;
            _userManager = userManager;
            _context = context;
        }

        public async Task SeedFinanceSalesReportsAsync()
        {
            // Get all FinanceEmployee users
            var financeEmployees = await _userManager.GetUsersInRoleAsync(UserRoles.FinanceEmployee);
            
            if (!financeEmployees.Any())
            {
                Console.WriteLine("No FinanceEmployee users found. Please create finance employees first.");
                return;
            }

            var reportsCreated = 0;
            var random = new Random();

            // Sample report data
            var reportTemplates = new[]
            {
                new { Type = "daily", Titles = new[] { "Daily Financial Analysis", "Daily Revenue Report", "Daily Expense Tracking", "Daily Budget Review" } },
                new { Type = "weekly", Titles = new[] { "Weekly Financial Summary", "Weekly Revenue Analysis", "Weekly Cost Analysis", "Weekly Budget Report" } },
                new { Type = "monthly", Titles = new[] { "Monthly Financial Review", "Monthly Revenue Report", "Monthly Expense Analysis", "Monthly Budget Summary" } },
                new { Type = "custom", Titles = new[] { "Quarterly Financial Report", "Annual Budget Review", "Special Project Analysis", "Financial Audit Report" } }
            };

            var reportBodies = new[]
            {
                "Financial analysis shows positive trends with increased revenue and controlled expenses. Key metrics indicate healthy growth patterns.",
                "Revenue streams are performing well above projections. Cost management initiatives are showing positive results.",
                "Budget variance analysis reveals minor deviations within acceptable limits. Recommendations for optimization included.",
                "Financial performance exceeds expectations with strong cash flow management and effective cost control measures.",
                "Detailed financial review indicates stable growth with improved profit margins and reduced operational costs.",
                "Revenue analysis shows consistent growth patterns with effective budget allocation and expense management.",
                "Financial metrics demonstrate strong performance with positive ROI and efficient resource utilization.",
                "Budget analysis reveals excellent financial discipline with minimal variance and optimal cost management."
            };

            // Generate reports for the last 3 months
            var startDate = DateTime.Today.AddMonths(-3);
            var endDate = DateTime.Today;

            foreach (var employee in financeEmployees)
            {
                // Generate 2-4 reports per month for each employee
                var reportsPerMonth = random.Next(2, 5);
                
                for (int month = 0; month < 3; month++)
                {
                    var currentMonth = startDate.AddMonths(month);
                    var daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                    
                    for (int i = 0; i < reportsPerMonth; i++)
                    {
                        var template = reportTemplates[random.Next(reportTemplates.Length)];
                        var title = template.Titles[random.Next(template.Titles.Length)];
                        var body = reportBodies[random.Next(reportBodies.Length)];
                        
                        // Generate random date within the month
                        var reportDate = new DateOnly(
                            currentMonth.Year, 
                            currentMonth.Month, 
                            random.Next(1, daysInMonth + 1)
                        );

                        // Skip if date is in the future
                        if (reportDate > DateOnly.FromDateTime(DateTime.Today))
                            continue;

                        var createDto = new CreateSalesReportDto
                        {
                            Title = $"{title} - {reportDate:MMMM yyyy}",
                            Body = $"{body}\n\nReport Date: {reportDate:yyyy-MM-dd}\nEmployee: {employee.FirstName} {employee.LastName}\nDepartment: Finance",
                            Type = template.Type,
                            ReportDate = reportDate
                        };

                        try
                        {
                            var result = await _salesReportService.CreateReportAsync(createDto, employee.Id);
                            if (result != null)
                            {
                                reportsCreated++;
                                Console.WriteLine($"Created finance sales report: {result.Title} for {employee.FirstName} {employee.LastName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating report for {employee.FirstName} {employee.LastName}: {ex.Message}");
                        }
                    }
                }
            }

            Console.WriteLine($"Finance Sales Reports Seeding Completed: {reportsCreated} reports created for {financeEmployees.Count} finance employees.");
        }

        public async Task SeedFinanceManagerRatingsAsync()
        {
            // Get FinanceManager users
            var financeManagers = await _userManager.GetUsersInRoleAsync(UserRoles.FinanceManager);
            
            if (!financeManagers.Any())
            {
                Console.WriteLine("No FinanceManager users found. Skipping rating seeding.");
                return;
            }

            // Get all finance sales reports
            var allReports = await _context.SalesReports
                .Where(sr => sr.IsActive)
                .Include(sr => sr.Employee)
                .Where(sr => sr.Employee != null && 
                           _userManager.IsInRoleAsync(sr.Employee, UserRoles.FinanceEmployee).Result)
                .ToListAsync();

            var random = new Random();
            var ratings = new[] { 3, 4, 5 }; // Good ratings for finance reports
            var comments = new[]
            {
                "Excellent financial analysis with clear insights and recommendations.",
                "Well-structured report with comprehensive data analysis and good conclusions.",
                "Outstanding work on financial reporting with detailed breakdown and actionable insights.",
                "Great job on the financial analysis. The recommendations are very valuable.",
                "Excellent report with thorough analysis and clear presentation of financial data.",
                "Outstanding financial reporting with comprehensive coverage and professional presentation.",
                "Great work on the financial analysis. The insights provided are very helpful.",
                "Excellent report with detailed financial analysis and practical recommendations."
            };

            var ratingsCreated = 0;

            foreach (var report in allReports)
            {
                // 70% chance to rate each report
                if (random.Next(100) < 70)
                {
                    var rating = ratings[random.Next(ratings.Length)];
                    var comment = comments[random.Next(comments.Length)];

                    var rateDto = new RateSalesReportDto
                    {
                        Rating = rating,
                        Comment = comment
                    };

                    try
                    {
                        var result = await _salesReportService.RateReportAsync(report.Id, rateDto);
                        if (result != null)
                        {
                            ratingsCreated++;
                            Console.WriteLine($"Rated finance sales report: {report.Title} with {rating} stars");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error rating report {report.Title}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Finance Manager Ratings Seeding Completed: {ratingsCreated} ratings created.");
        }
    }
}
