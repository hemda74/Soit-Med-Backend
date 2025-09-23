using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DataSeedingController : ControllerBase
    {
        private readonly FinanceSalesReportSeedingService _financeSeedingService;

        public DataSeedingController(FinanceSalesReportSeedingService financeSeedingService)
        {
            _financeSeedingService = financeSeedingService;
        }

        /// <summary>
        /// Seed finance sales reports with dummy data
        /// </summary>
        [HttpPost("finance-sales-reports")]
        public async Task<IActionResult> SeedFinanceSalesReports()
        {
            try
            {
                await _financeSeedingService.SeedFinanceSalesReportsAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Finance sales reports seeded successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error seeding finance sales reports",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Seed finance manager ratings for existing reports
        /// </summary>
        [HttpPost("finance-manager-ratings")]
        public async Task<IActionResult> SeedFinanceManagerRatings()
        {
            try
            {
                await _financeSeedingService.SeedFinanceManagerRatingsAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Finance manager ratings seeded successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error seeding finance manager ratings",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Seed all finance-related data
        /// </summary>
        [HttpPost("finance-all")]
        public async Task<IActionResult> SeedAllFinanceData()
        {
            try
            {
                Console.WriteLine("Starting finance data seeding...");
                
                // First seed the reports
                Console.WriteLine("Seeding finance sales reports...");
                await _financeSeedingService.SeedFinanceSalesReportsAsync();
                
                // Then seed the ratings
                Console.WriteLine("Seeding finance manager ratings...");
                await _financeSeedingService.SeedFinanceManagerRatingsAsync();
                
                Console.WriteLine("Finance data seeding completed successfully.");
                
                return Ok(new
                {
                    success = true,
                    message = "All finance data seeded successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in finance data seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return BadRequest(new
                {
                    success = false,
                    message = "Error seeding finance data",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
