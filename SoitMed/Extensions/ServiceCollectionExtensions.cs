using SoitMed.Services;

namespace SoitMed.Extensions
{
    /// <summary>
    /// Extension methods for service registration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all application services
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
        // Register services
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IMappingService, MappingService>();
        services.AddScoped<IWeeklyPlanService, WeeklyPlanService>();
        services.AddScoped<IWeeklyPlanItemService, WeeklyPlanItemService>();

            return services;
        }
    }
}
