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
            // Core services
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IMappingService, MappingService>();
            services.AddScoped<IWeeklyPlanService, WeeklyPlanService>();
            services.AddScoped<IWeeklyPlanTaskService, WeeklyPlanTaskService>();

            // Sales workflow services
            services.AddScoped<ITaskProgressService, TaskProgressService>();
            services.AddScoped<IOfferRequestService, OfferRequestService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IDealService, DealService>();
            
            // Enhanced offer services
            services.AddScoped<IOfferEquipmentImageService, OfferEquipmentImageService>();
            
            // Product catalog service
            services.AddScoped<IProductService, ProductService>();

            // Maintenance services
            services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
            services.AddScoped<IMaintenanceVisitService, MaintenanceVisitService>();
            services.AddScoped<ISparePartRequestService, SparePartRequestService>();
            services.AddScoped<IMaintenanceAttachmentService, MaintenanceAttachmentService>();

            // Payment services
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAccountingService, AccountingService>();
            
            // Mobile notification service (IHttpClientFactory registered in Program.cs)
            services.AddScoped<IMobileNotificationService, MobileNotificationService>();

            return services;
        }
    }
}
