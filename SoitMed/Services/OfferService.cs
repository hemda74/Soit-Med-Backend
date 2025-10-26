using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing offers in the sales workflow
    /// </summary>
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OfferService> _logger;

        public OfferService(IUnitOfWork unitOfWork, ILogger<OfferService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Offer Management

        public async Task<OfferResponseDTO> CreateOfferFromRequestAsync(CreateOfferDTO createOfferDto, string userId)
        {
            try
            {
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(createOfferDto.OfferRequestId);
                if (offerRequest == null)
                    throw new ArgumentException("Offer request not found", nameof(createOfferDto.OfferRequestId));

                var offer = new SalesOffer
                {
                    OfferRequestId = createOfferDto.OfferRequestId,
                    ClientId = createOfferDto.ClientId,
                    CreatedBy = userId,
                    AssignedTo = createOfferDto.AssignedTo,
                    Products = createOfferDto.Products,
                    TotalAmount = createOfferDto.TotalAmount,
                    PaymentTerms = createOfferDto.PaymentTerms,
                    DeliveryTerms = createOfferDto.DeliveryTerms,
                    ValidUntil = createOfferDto.ValidUntil,
                    Notes = createOfferDto.Notes,
                    Status = "Draft"
                };

                await _unitOfWork.SalesOffers.CreateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Update offer request status
                offerRequest.MarkAsCompleted($"Offer created with ID: {offer.Id}");
                await _unitOfWork.OfferRequests.UpdateAsync(offerRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer created from request successfully. OfferId: {OfferId}, RequestId: {RequestId}", 
                    offer.Id, createOfferDto.OfferRequestId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer from request. RequestId: {RequestId}", createOfferDto.OfferRequestId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> CreateOfferAsync(CreateOfferDTO createOfferDto, string userId)
        {
            try
            {
                var offer = new SalesOffer
                {
                    OfferRequestId = createOfferDto.OfferRequestId,
                    ClientId = createOfferDto.ClientId,
                    CreatedBy = userId,
                    AssignedTo = createOfferDto.AssignedTo,
                    Products = createOfferDto.Products,
                    TotalAmount = createOfferDto.TotalAmount,
                    PaymentTerms = createOfferDto.PaymentTerms,
                    DeliveryTerms = createOfferDto.DeliveryTerms,
                    ValidUntil = createOfferDto.ValidUntil,
                    Notes = createOfferDto.Notes,
                    Status = "Draft"
                };

                await _unitOfWork.SalesOffers.CreateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer created successfully. OfferId: {OfferId}", offer.Id);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer");
                throw;
            }
        }

        public async Task<OfferResponseDTO?> GetOfferAsync(long offerId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    return null;

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offer. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<List<OfferResponseDTO>> GetOffersByClientAsync(long clientId)
        {
            try
            {
                var offers = await _unitOfWork.SalesOffers.GetOffersByClientIdAsync(clientId);
                var result = new List<OfferResponseDTO>();

                foreach (var offer in offers)
                {
                    result.Add(await MapToOfferResponseDTO(offer));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by client. ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<OfferResponseDTO>> GetOffersBySalesmanAsync(string salesmanId)
        {
            try
            {
                var offers = await _unitOfWork.SalesOffers.GetOffersBySalesmanAsync(salesmanId);
                var result = new List<OfferResponseDTO>();

                foreach (var offer in offers)
                {
                    result.Add(await MapToOfferResponseDTO(offer));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by salesman. SalesmanId: {SalesmanId}", salesmanId);
                throw;
            }
        }

        public async Task<List<OfferResponseDTO>> GetOffersByStatusAsync(string status)
        {
            try
            {
                IEnumerable<SalesOffer> offers;
                if (string.IsNullOrEmpty(status))
                {
                    offers = await _unitOfWork.SalesOffers.GetAllAsync();
                }
                else
                {
                    offers = await _unitOfWork.SalesOffers.GetOffersByStatusAsync(status);
                }
                
                var result = new List<OfferResponseDTO>();

                foreach (var offer in offers)
                {
                    result.Add(await MapToOfferResponseDTO(offer));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by status. Status: {Status}", status);
                throw;
            }
        }

        public async Task<List<OfferResponseDTO>> GetOffersByCreatorAsync(string creatorId)
        {
            try
            {
                var offers = await _unitOfWork.SalesOffers.GetOffersByCreatorAsync(creatorId);
                var result = new List<OfferResponseDTO>();

                foreach (var offer in offers)
                {
                    result.Add(await MapToOfferResponseDTO(offer));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by creator. CreatorId: {CreatorId}", creatorId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> UpdateOfferAsync(long offerId, CreateOfferDTO updateOfferDto, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                // Only allow updates if offer is in draft status
                if (offer.Status != "Draft")
                    throw new InvalidOperationException("Only draft offers can be updated");

                offer.Products = updateOfferDto.Products;
                offer.TotalAmount = updateOfferDto.TotalAmount;
                offer.PaymentTerms = updateOfferDto.PaymentTerms;
                offer.DeliveryTerms = updateOfferDto.DeliveryTerms;
                offer.ValidUntil = updateOfferDto.ValidUntil;
                offer.Notes = updateOfferDto.Notes;
                offer.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer updated successfully. OfferId: {OfferId}", offerId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> SendToSalesmanAsync(long offerId, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                offer.MarkAsSent();
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer sent to salesman successfully. OfferId: {OfferId}", offerId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending offer to salesman. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> RecordClientResponseAsync(long offerId, string response, bool accepted, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                offer.RecordClientResponse(response, accepted);
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client response recorded for offer. OfferId: {OfferId}, Accepted: {Accepted}", offerId, accepted);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording client response. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<bool> DeleteOfferAsync(long offerId, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    return false;

                // Only allow deletion if offer is in draft status
                if (offer.Status != "Draft")
                    throw new InvalidOperationException("Only draft offers can be deleted");

                await _unitOfWork.SalesOffers.DeleteAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer deleted successfully. OfferId: {OfferId}", offerId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting offer. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        #endregion

        #region Business Logic Methods

        public async Task<bool> ValidateOfferAsync(CreateOfferDTO offerDto)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(offerDto.Products))
                return false;

            if (offerDto.TotalAmount <= 0)
                return false;

            if (offerDto.ValidUntil <= DateTime.UtcNow)
                return false;

            // Validate client exists
            var client = await _unitOfWork.Clients.GetByIdAsync(offerDto.ClientId);
            if (client == null)
                return false;

            return true;
        }

        public async Task<List<OfferResponseDTO>> GetExpiredOffersAsync()
        {
            try
            {
                var offers = await _unitOfWork.SalesOffers.GetExpiredOffersAsync();
                var result = new List<OfferResponseDTO>();

                foreach (var offer in offers)
                {
                    result.Add(await MapToOfferResponseDTO(offer));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired offers");
                throw;
            }
        }

        #endregion

        #region Mapping Methods

        private async Task<OfferResponseDTO> MapToOfferResponseDTO(SalesOffer offer)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
            var creator = await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy);
            var salesman = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);

            return new OfferResponseDTO
            {
                Id = offer.Id,
                OfferRequestId = offer.OfferRequestId,
                ClientId = offer.ClientId,
                ClientName = client?.Name ?? "Unknown Client",
                CreatedBy = offer.CreatedBy,
                CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown Creator",
                AssignedTo = offer.AssignedTo,
                AssignedToName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Unknown Salesman",
                Products = offer.Products,
                TotalAmount = offer.TotalAmount,
                PaymentTerms = offer.PaymentTerms,
                DeliveryTerms = offer.DeliveryTerms,
                ValidUntil = offer.ValidUntil,
                Status = offer.Status,
                SentToClientAt = offer.SentToClientAt,
                ClientResponse = offer.ClientResponse,
                CreatedAt = offer.CreatedAt
            };
        }

        #endregion
    }
}
