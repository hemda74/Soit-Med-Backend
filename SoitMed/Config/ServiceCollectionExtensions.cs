using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Services;
using SoitMed.Repositories;

namespace SoitMed.Config
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSoitMedServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<Context>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Core Services
            services.AddScoped<IComprehensiveMaintenanceService, ComprehensiveMaintenanceService>();
            
            return services;
        }
    }
}
