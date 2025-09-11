using SoitMed.Scripts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can run migrations
    public class MigrationController : ControllerBase
    {
        private readonly UpdateUserIdsScript _updateUserIdsScript;

        public MigrationController(UpdateUserIdsScript updateUserIdsScript)
        {
            _updateUserIdsScript = updateUserIdsScript;
        }

        /// <summary>
        /// Preview what user ID changes would be made (dry run)
        /// </summary>
        [HttpGet("preview-user-id-updates")]
        public async Task<IActionResult> PreviewUserIdUpdates()
        {
            try
            {
                await _updateUserIdsScript.PreviewUpdatesAsync();
                return Ok("Preview completed. Check console output for details.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during preview: {ex.Message}");
            }
        }

        /// <summary>
        /// Update all user IDs to the new pattern
        /// WARNING: This is a destructive operation
        /// </summary>
        [HttpPost("update-user-ids")]
        public async Task<IActionResult> UpdateUserIds()
        {
            try
            {
                await _updateUserIdsScript.UpdateAllUserIdsAsync();
                return Ok("User ID migration completed successfully. Check console output for details.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during migration: {ex.Message}");
            }
        }
    }
}
