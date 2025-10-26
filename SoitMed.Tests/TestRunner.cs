using System;
using System.Threading.Tasks;

namespace SoitMed.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await SimpleApiTest.RunAllTestsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test execution failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}

