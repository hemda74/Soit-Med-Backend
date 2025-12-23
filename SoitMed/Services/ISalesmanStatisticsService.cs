using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface ISalesManStatisticsService
    {
        // Statistics
        Task<SalesManStatisticsDTO> GetStatisticsAsync(string salesmanId, int year, int? quarter = null);
        Task<List<SalesManStatisticsDTO>> GetAllSalesmenStatisticsAsync(int year, int? quarter = null);
        
        // Targets
        Task<SalesManTargetDTO> CreateTargetAsync(CreateSalesManTargetDTO dto, string? managerId, string? salesmanId = null);
        Task<SalesManTargetDTO> UpdateTargetAsync(long targetId, CreateSalesManTargetDTO dto, string? currentUserId = null);
        Task<bool> DeleteTargetAsync(long targetId);
        Task<SalesManTargetDTO?> GetTargetAsync(long targetId);
        Task<List<SalesManTargetDTO>> GetTargetsForSalesManAsync(string salesmanId, int year);
        Task<SalesManTargetDTO?> GetTeamTargetAsync(int year, int? quarter = null);
        
        // Progress
        Task<SalesManProgressDTO> GetSalesManProgressAsync(string salesmanId, int year, int? quarter = null);
    }
}

