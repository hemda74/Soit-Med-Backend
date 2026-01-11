using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for retrieving customer machines data from ITIWebApi44
    /// Replaces Media API endpoint that queries TBS database
    /// </summary>
    public interface ICustomerMachinesService
    {
        /// <summary>
        /// Get all machines for a customer by customer ID
        /// </summary>
        /// <param name="customerId">Customer ID (can be new system ID or LegacyCustomerId)</param>
        /// <returns>Customer machines data with visit counts and media files</returns>
        Task<CustomerMachinesDto?> GetMachinesByCustomerIdAsync(long customerId);
    }
}
