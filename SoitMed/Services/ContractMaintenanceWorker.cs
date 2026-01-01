using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SoitMed.Models.Enums;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Background worker that runs daily to auto-generate maintenance visits
    /// based on LegalManager contracts with maintenance schedules
    /// </summary>
    public class ContractMaintenanceWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ContractMaintenanceWorker> _logger;
        private readonly ContractMaintenanceOptions _options;

        public ContractMaintenanceWorker(
            IServiceProvider serviceProvider,
            IOptions<ContractMaintenanceOptions> options,
            ILogger<ContractMaintenanceWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ContractMaintenanceWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessContractMaintenanceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ContractMaintenanceWorker execution");
                }

                // Run daily at configured time (default: 2 AM)
                var nextRun = DateTime.Today.AddDays(1).AddHours(_options.RunHour);
                if (nextRun <= DateTime.Now)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - DateTime.Now;
                _logger.LogInformation("ContractMaintenanceWorker will run again at {NextRun}", nextRun);

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task ProcessContractMaintenanceAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SoitMed.Models.Identity.ApplicationUser>>();

            try
            {
                _logger.LogInformation("Starting contract maintenance processing. ScheduleDaysAhead: {Days}", 
                    _options.ScheduleDaysAhead);

                // Get date range for upcoming visits
                var startDate = DateTime.UtcNow.Date;
                var endDate = startDate.AddDays(_options.ScheduleDaysAhead);

                // TODO: When Contract model is available, query contracts with maintenance schedules
                // For now, this is a placeholder that can be extended
                // Example query would be:
                // var contracts = await _unitOfWork.Contracts.GetContractsWithMaintenanceScheduleAsync(startDate, endDate);

                _logger.LogInformation("Contract maintenance processing completed. No contracts model available yet.");

                // Placeholder: When contracts are available, iterate and create visits:
                /*
                foreach (var contract in contracts)
                {
                    try
                    {
                        // Get equipment from contract
                        var equipment = await unitOfWork.Equipment.GetByIdAsync(contract.EquipmentId);
                        if (equipment == null) continue;

                        // Get customer from contract
                        var customer = await userManager.FindByIdAsync(contract.CustomerId);
                        if (customer == null) continue;

                        // Create maintenance request if needed
                        var existingRequest = await unitOfWork.MaintenanceRequests
                            .GetFirstOrDefaultAsync(mr => mr.EquipmentId == equipment.Id && 
                                                       mr.CustomerId == contract.CustomerId &&
                                                       mr.Status == MaintenanceRequestStatus.Pending);

                        int requestId;
                        if (existingRequest == null)
                        {
                            var request = new MaintenanceRequest
                            {
                                CustomerId = contract.CustomerId,
                                EquipmentId = equipment.Id,
                                Description = $"Scheduled maintenance per contract {contract.ContractNumber}",
                                Status = MaintenanceRequestStatus.Pending
                            };
                            await unitOfWork.MaintenanceRequests.CreateAsync(request);
                            await unitOfWork.SaveChangesAsync();
                            requestId = request.Id;
                        }
                        else
                        {
                            requestId = existingRequest.Id;
                        }

                        // Create visit for each scheduled date
                        var visitDto = new CreateVisitDTO
                        {
                            MaintenanceRequestId = requestId,
                            DeviceId = equipment.Id,
                            ScheduledDate = contract.NextMaintenanceDate,
                            Origin = VisitOrigin.AutoContract,
                            IsPaidVisit = contract.IncludesMaintenance,
                            Cost = contract.MaintenanceCost
                        };

                        await maintenanceService.CreateVisitAsync(visitDto, contract.LegalManagerId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating visit for contract {ContractId}", contract.Id);
                    }
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contract maintenance");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ContractMaintenanceWorker is stopping");
            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Configuration options for ContractMaintenanceWorker
    /// </summary>
    public class ContractMaintenanceOptions
    {
        public const string SectionName = "ContractMaintenance";

        /// <summary>
        /// Number of days ahead to schedule visits
        /// </summary>
        public int ScheduleDaysAhead { get; set; } = 7;

        /// <summary>
        /// Hour of day to run the worker (0-23)
        /// </summary>
        public int RunHour { get; set; } = 2;
    }
}

