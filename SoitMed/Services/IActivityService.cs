using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IActivityService
    {
        Task<ActivityResponseDto> CreateActivityAsync(int taskId, string userId, CreateActivityRequestDto request, CancellationToken cancellationToken = default);
        Task<DealResponseDto?> UpdateDealAsync(long dealId, string userId, UpdateDealDto updateDto, CancellationToken cancellationToken = default);
        Task<OfferResponseDto?> UpdateOfferAsync(long offerId, string userId, UpdateOfferDto updateDto, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityResponseDto>> GetActivitiesByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<ActivityResponseDto?> GetActivityByIdAsync(long activityId, string userId, CancellationToken cancellationToken = default);
    }
}
