using SoitMed.DTO;
using SoitMed.Models.Equipment;

namespace SoitMed.Services
{
    public interface IEquipmentService
    {
        /// <summary>
        /// Get all equipment for a client using comprehensive relationship checking
        /// Similar to soitmed_data_backend's GetMachinesByCustomerIdAsync approach
        /// </summary>
        /// <param name="clientId">Client ID (long)</param>
        /// <returns>List of equipment associated with the client through any relationship</returns>
        Task<IEnumerable<EquipmentResponseDTO>> GetEquipmentByClientIdAsync(long clientId);
        
        /// <summary>
        /// Get all equipment for a customer (ApplicationUser) using comprehensive relationship checking
        /// </summary>
        /// <param name="customerId">Customer ID (ApplicationUser Id)</param>
        /// <returns>List of equipment associated with the customer through any relationship</returns>
        Task<IEnumerable<EquipmentResponseDTO>> GetEquipmentByCustomerIdAsync(string customerId);
    }
}

