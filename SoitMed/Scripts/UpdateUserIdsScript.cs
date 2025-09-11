using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Scripts
{
    public class UpdateUserIdsScript
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserIdGenerationService _userIdGenerationService;

        public UpdateUserIdsScript(Context context, UserManager<ApplicationUser> userManager, UserIdGenerationService userIdGenerationService)
        {
            _context = context;
            _userManager = userManager;
            _userIdGenerationService = userIdGenerationService;
        }

        /// <summary>
        /// Updates all existing user IDs to the new pattern
        /// WARNING: This is a destructive operation that will change all user IDs
        /// Make sure to backup your database before running this script
        /// </summary>
        public async Task UpdateAllUserIdsAsync()
        {
            Console.WriteLine("Starting user ID migration...");
            Console.WriteLine("WARNING: This will change all existing user IDs!");
            
            // Get all users
            var users = await _context.Users.ToListAsync();
            Console.WriteLine($"Found {users.Count} users to update.");

            var updateCount = 0;
            var errorCount = 0;

            foreach (var user in users)
            {
                try
                {
                    // Skip if user already has the new pattern (contains underscores and ends with a number)
                    if (IsNewIdPattern(user.Id))
                    {
                        Console.WriteLine($"Skipping user {user.Id} - already in new pattern");
                        continue;
                    }

                    // Generate new ID
                    string newId = await _userIdGenerationService.UpdateUserToNewIdPatternAsync(user);
                    
                    if (newId != user.Id)
                    {
                        // Update the user ID
                        await UpdateUserIdAsync(user, newId);
                        updateCount++;
                        Console.WriteLine($"Updated user: {user.FirstName} {user.LastName} - Old ID: {user.Id} -> New ID: {newId}");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"Error updating user {user.Id}: {ex.Message}");
                }
            }

            Console.WriteLine($"Migration completed. Updated: {updateCount}, Errors: {errorCount}");
        }

        /// <summary>
        /// Updates a single user's ID and all related records
        /// </summary>
        private async Task UpdateUserIdAsync(ApplicationUser user, string newId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Store old ID
                string oldId = user.Id;

                // Update user ID in Identity tables
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AspNetUsers SET Id = {0} WHERE Id = {1}", newId, oldId);
                
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AspNetUserRoles SET UserId = {0} WHERE UserId = {1}", newId, oldId);
                
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AspNetUserClaims SET UserId = {0} WHERE UserId = {1}", newId, oldId);
                
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AspNetUserLogins SET UserId = {0} WHERE UserId = {1}", newId, oldId);
                
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AspNetUserTokens SET UserId = {0} WHERE UserId = {1}", newId, oldId);

                // Update related records in custom tables
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Doctors SET UserId = {0} WHERE UserId = {1}", newId, oldId);
                
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Technicians SET UserId = {0} WHERE UserId = {1}", newId, oldId);

                // Update any other tables that reference the user ID
                // Add more UPDATE statements here for other tables that reference UserId

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Checks if an ID follows the new pattern (FirstName_LastName_Organization_Number)
        /// </summary>
        private bool IsNewIdPattern(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            var parts = id.Split('_');
            if (parts.Length < 4)
                return false;

            // Check if the last part is a number
            return int.TryParse(parts[parts.Length - 1], out _);
        }

        /// <summary>
        /// Dry run - shows what would be updated without making changes
        /// </summary>
        public async Task PreviewUpdatesAsync()
        {
            Console.WriteLine("Preview mode - no changes will be made");
            Console.WriteLine("=====================================");

            var users = await _context.Users.ToListAsync();
            Console.WriteLine($"Found {users.Count} users to analyze.");

            foreach (var user in users)
            {
                try
                {
                    if (IsNewIdPattern(user.Id))
                    {
                        Console.WriteLine($"✓ {user.FirstName} {user.LastName} - {user.Id} (already new pattern)");
                    }
                    else
                    {
                        string newId = await _userIdGenerationService.UpdateUserToNewIdPatternAsync(user);
                        Console.WriteLine($"→ {user.FirstName} {user.LastName} - {user.Id} -> {newId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ {user.FirstName} {user.LastName} - {user.Id} (ERROR: {ex.Message})");
                }
            }
        }
    }
}
