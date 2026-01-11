using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Simple test controller for debugging
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                message = "Test API is working!", 
                timestamp = DateTime.UtcNow,
                server = "SoitMed Backend"
            });
        }

        [HttpGet("search")]
        public IActionResult Search()
        {
            var mockCustomers = new[]
            {
                new { id = "1", name = "جمعية دار الاورمان بالجيزة", phone = "01226369240", source = "Legacy" },
                new { id = "2", name = "Cairo Medical Center", phone = "02234567890", source = "New" }
            };

            return Ok(new { 
                items = mockCustomers,
                totalCount = mockCustomers.Length,
                pageNumber = 1,
                pageSize = 20
            });
        }
    }
}
