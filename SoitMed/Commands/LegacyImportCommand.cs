using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoitMed.Services;

namespace SoitMed.Commands
{
    /// <summary>
    /// Console command to run legacy data import
    /// Usage: dotnet run -- legacy-import [--clients-only] [--equipment-only] [--visits-only]
    /// </summary>
    public class LegacyImportCommand
    {
        public static async Task<int> ExecuteAsync(string[] args, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<LegacyImportCommand>>();
            var importer = serviceProvider.GetRequiredService<ILegacyImporterService>();

            try
            {
                logger.LogInformation("Starting Legacy Data Import Command");

                ImportResult result;

                // Check command line arguments
                if (args.Contains("--clients-only"))
                {
                    logger.LogInformation("Importing clients only...");
                    result = await importer.ImportClientsAsync();
                }
                else if (args.Contains("--equipment-only"))
                {
                    logger.LogInformation("Importing equipment only...");
                    result = await importer.ImportEquipmentAsync();
                }
                else if (args.Contains("--visits-only"))
                {
                    logger.LogInformation("Importing maintenance visits only...");
                    result = await importer.ImportMaintenanceVisitsAsync();
                }
                else
                {
                    logger.LogInformation("Importing all legacy data...");
                    result = await importer.ImportAllAsync();
                }

                // Print results
                Console.WriteLine("\n" + "=".PadRight(80, '='));
                Console.WriteLine("LEGACY IMPORT RESULTS");
                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine($"Success: {result.SuccessCount}");
                Console.WriteLine($"Failures: {result.FailureCount}");
                Console.WriteLine($"Skipped: {result.SkippedCount}");
                Console.WriteLine($"Log File: {result.LogFilePath}");
                
                if (result.Errors.Any())
                {
                    Console.WriteLine($"\nErrors ({result.Errors.Count}):");
                    foreach (var error in result.Errors.Take(10)) // Show first 10 errors
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    if (result.Errors.Count > 10)
                    {
                        Console.WriteLine($"  ... and {result.Errors.Count - 10} more errors (see log file)");
                    }
                }

                Console.WriteLine("\n" + "=".PadRight(80, '='));
                logger.LogInformation("Legacy import command completed successfully");

                return result.FailureCount > 0 ? 1 : 0; // Return 0 for success, 1 for failures
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing legacy import command");
                Console.WriteLine($"\nFATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return 1;
            }
        }
    }
}

