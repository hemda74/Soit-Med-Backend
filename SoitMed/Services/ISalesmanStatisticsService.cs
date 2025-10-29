using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface ISalesmanStatisticsService
    {
        // Statistics
        Task<SalesmanStatisticsDTO> GetStatisticsAsync(string salesmanId, int year, int? quarter = null);
        Task<List<SalesmanStatisticsDTO>> GetAllSalesmenStatisticsAsync(int year, int? quarter = null);
        
        // Targets
        Task<SalesmanTargetDTO> CreateTargetAsync(CreateSalesmanTargetDTO dto, string managerId);
        Task<SalesmanTargetDTO> UpdateTargetAsync(long targetId, CreateSalesmanTargetDTO dto);
        Task<bool> DeleteTargetAsync(long targetId);
        Task<SalesmanTargetDTO?> GetTargetAsync(long targetId);
        Task<List<SalesmanTargetDTO>> GetTargetsForSalesmanAsync(string salesmanId, int year);
        Task<SalesmanTargetDTO?> GetTeamTargetAsync(int year, int? quarter = null);
        
        // Progress
        Task<SalesmanProgressDTO> GetSalesmanProgressAsync(string salesmanId, int year, int? quarter = null);
    }
}

