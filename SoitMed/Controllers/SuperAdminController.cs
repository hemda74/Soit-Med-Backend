using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Scripts;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly UpdateSuperAdminScript _updateSuperAdminScript;
        private readonly CleanSuperAdminScript _cleanSuperAdminScript;
        private readonly CleanAndCreateTestUsersScript _cleanAndCreateTestUsersScript;
        private readonly UpdateExistingUserIdsScript _updateExistingUserIdsScript;

        public SuperAdminController(UpdateSuperAdminScript updateSuperAdminScript, CleanSuperAdminScript cleanSuperAdminScript, CleanAndCreateTestUsersScript cleanAndCreateTestUsersScript, UpdateExistingUserIdsScript updateExistingUserIdsScript)
        {
            _updateSuperAdminScript = updateSuperAdminScript;
            _cleanSuperAdminScript = cleanSuperAdminScript;
            _cleanAndCreateTestUsersScript = cleanAndCreateTestUsersScript;
            _updateExistingUserIdsScript = updateExistingUserIdsScript;
        }

        [HttpPost("update-superadmin")]
        public async Task<IActionResult> UpdateSuperAdmin()
        {
            try
            {
                var result = await _updateSuperAdminScript.UpdateSuperAdminAsync();
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("clean-superadmin")]
        public async Task<IActionResult> CleanSuperAdmin()
        {
            try
            {
                var result = await _cleanSuperAdminScript.CleanSuperAdminAsync();
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("clean-and-create-test-users")]
        public async Task<IActionResult> CleanAndCreateTestUsers()
        {
            try
            {
                var result = await _cleanAndCreateTestUsersScript.CleanAndCreateTestUsersAsync();
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("update-existing-user-ids")]
        public async Task<IActionResult> UpdateExistingUserIds()
        {
            try
            {
                var result = await _updateExistingUserIdsScript.UpdateAllUserIdsAsync();
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


    }
}
