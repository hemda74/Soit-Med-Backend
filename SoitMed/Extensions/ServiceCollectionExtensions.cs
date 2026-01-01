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
            
            // Product catalog services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            
            // Maintenance services
            services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
            services.AddScoped<IMaintenanceVisitService, MaintenanceVisitService>();
            services.AddScoped<ISparePartRequestService, SparePartRequestService>();
            services.AddScoped<IMaintenanceAttachmentService, MaintenanceAttachmentService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IVisitStateService, VisitStateService>();
            services.AddScoped<IAuditService, AuditService>();
            
            // Domain events
            services.AddScoped<SoitMed.Common.DomainEvents.IDomainEventDispatcher, SoitMed.Common.DomainEvents.DomainEventDispatcher>();
            services.AddScoped<SoitMed.Common.DomainEvents.IDomainEventHandler<SoitMed.Common.DomainEvents.VisitScheduledEvent>, SoitMed.Common.DomainEvents.VisitScheduledEventHandler>();

            // Payment services
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAccountingService, AccountingService>();
            
            // Payment strategies (Strategy Pattern for future installment support)
            services.AddScoped<SoitMed.Services.Payment.IPaymentStrategy, SoitMed.Services.Payment.CashPaymentStrategy>();
            services.AddScoped<SoitMed.Services.Payment.IPaymentStrategy, SoitMed.Services.Payment.VisaPaymentStrategy>();
            services.AddScoped<SoitMed.Services.Payment.IPaymentStrategy, SoitMed.Services.Payment.InstallmentPaymentStrategy>();
            services.AddScoped<SoitMed.Services.Payment.PaymentStrategyFactory>();

            // Chat services
            services.AddScoped<IChatService, ChatService>();

            // Legacy import service
            services.AddScoped<ILegacyImporterService, LegacyImporterService>();

            // Legacy media service
            services.AddScoped<ILegacyMediaService, LegacyMediaService>();

            return services;
        }
    }
}
