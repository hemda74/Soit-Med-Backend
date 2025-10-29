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
                    PaymentType = createOfferDto.PaymentType,
                    FinalPrice = createOfferDto.FinalPrice,
                    OfferDuration = createOfferDto.OfferDuration,
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
                    PaymentType = createOfferDto.PaymentType,
                    FinalPrice = createOfferDto.FinalPrice,
                    OfferDuration = createOfferDto.OfferDuration,
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

        public async Task<object?> GetOfferRequestDetailsAsync(long requestId)
        {
            try
            {
                var request = await _unitOfWork.OfferRequests.GetByIdAsync(requestId);
                if (request == null)
                    return null;

                var requester = await _unitOfWork.Users.GetByIdAsync(request.RequestedBy);
                var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId);
                var assignedSupport = request.AssignedTo != null ? await _unitOfWork.Users.GetByIdAsync(request.AssignedTo) : null;

                return new
                {
                    id = request.Id,
                    requestedBy = request.RequestedBy,
                    requestedByName = requester != null ? $"{requester.FirstName} {requester.LastName}" : "Unknown",
                    clientId = request.ClientId,
                    clientName = client?.Name ?? "Unknown Client",
                    requestedProducts = request.RequestedProducts,
                    specialNotes = request.SpecialNotes,
                    requestDate = request.RequestDate,
                    status = request.Status,
                    assignedTo = request.AssignedTo,
                    assignedToName = assignedSupport != null ? $"{assignedSupport.FirstName} {assignedSupport.LastName}" : null,
                    completedAt = request.CompletedAt,
                    completionNotes = request.CompletionNotes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offer request details for request {RequestId}", requestId);
                throw;
            }
        }

        #endregion

        #region Enhanced Offer Features - Equipment Management

        public async Task<OfferEquipmentDTO> AddEquipmentAsync(long offerId, CreateOfferEquipmentDTO dto)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                var equipment = new OfferEquipment
                {
                    OfferId = offerId,
                    Name = dto.Name,
                    Model = dto.Model,
                    Provider = dto.Provider,
                    Country = dto.Country,
                    Price = dto.Price,
                    Description = dto.Description,
                    InStock = dto.InStock
                };

                await _unitOfWork.OfferEquipment.CreateAsync(equipment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Equipment added to offer. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipment.Id);

                return new OfferEquipmentDTO
                {
                    Id = equipment.Id,
                    OfferId = equipment.OfferId,
                    Name = equipment.Name,
                    Model = equipment.Model,
                    Provider = equipment.Provider,
                    Country = equipment.Country,
                    ImagePath = equipment.ImagePath,
                    Price = equipment.Price,
                    Description = equipment.Description,
                    InStock = equipment.InStock
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding equipment to offer. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<List<OfferEquipmentDTO>> GetEquipmentListAsync(long offerId)
        {
            try
            {
                var equipment = await _unitOfWork.OfferEquipment.GetByOfferIdAsync(offerId);
                return equipment.Select(e => new OfferEquipmentDTO
                {
                    Id = e.Id,
                    OfferId = e.OfferId,
                    Name = e.Name,
                    Model = e.Model,
                    Provider = e.Provider,
                    Country = e.Country,
                    ImagePath = e.ImagePath,
                    Price = e.Price,
                    Description = e.Description,
                    InStock = e.InStock
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment list. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<bool> DeleteEquipmentAsync(long offerId, long equipmentId)
        {
            try
            {
                var equipment = await _unitOfWork.OfferEquipment.GetByIdAsync(equipmentId);
                if (equipment == null || equipment.OfferId != offerId)
                    return false;

                await _unitOfWork.OfferEquipment.DeleteAsync(equipment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Equipment deleted. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                throw;
            }
        }

        #endregion

        #region Enhanced Offer Features - Terms Management

        public async Task<OfferTermsDTO> AddOrUpdateTermsAsync(long offerId, CreateOfferTermsDTO dto)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                var existingTerms = await _unitOfWork.OfferTerms.GetByOfferIdAsync(offerId);

                if (existingTerms != null)
                {
                    existingTerms.WarrantyPeriod = dto.WarrantyPeriod;
                    existingTerms.DeliveryTime = dto.DeliveryTime;
                    existingTerms.MaintenanceTerms = dto.MaintenanceTerms;
                    existingTerms.OtherTerms = dto.OtherTerms;

                    await _unitOfWork.OfferTerms.UpdateAsync(existingTerms);
                    await _unitOfWork.SaveChangesAsync();

                    return new OfferTermsDTO
                    {
                        Id = existingTerms.Id,
                        OfferId = existingTerms.OfferId,
                        WarrantyPeriod = existingTerms.WarrantyPeriod,
                        DeliveryTime = existingTerms.DeliveryTime,
                        MaintenanceTerms = existingTerms.MaintenanceTerms,
                        OtherTerms = existingTerms.OtherTerms
                    };
                }
                else
                {
                    var terms = new OfferTerms
                    {
                        OfferId = offerId,
                        WarrantyPeriod = dto.WarrantyPeriod,
                        DeliveryTime = dto.DeliveryTime,
                        MaintenanceTerms = dto.MaintenanceTerms,
                        OtherTerms = dto.OtherTerms
                    };

                    await _unitOfWork.OfferTerms.CreateAsync(terms);
                    await _unitOfWork.SaveChangesAsync();

                    return new OfferTermsDTO
                    {
                        Id = terms.Id,
                        OfferId = terms.OfferId,
                        WarrantyPeriod = terms.WarrantyPeriod,
                        DeliveryTime = terms.DeliveryTime,
                        MaintenanceTerms = terms.MaintenanceTerms,
                        OtherTerms = terms.OtherTerms
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating terms. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        #endregion

        #region Enhanced Offer Features - Installment Plans

        public async Task<List<InstallmentPlanDTO>> CreateInstallmentPlanAsync(long offerId, CreateInstallmentPlanDTO dto)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                if (!offer.FinalPrice.HasValue || offer.FinalPrice.Value <= 0)
                    throw new InvalidOperationException("Offer must have a valid FinalPrice to create installment plan");

                var existingInstallments = await _unitOfWork.InstallmentPlans.GetByOfferIdAsync(offerId);
                foreach (var existing in existingInstallments)
                {
                    await _unitOfWork.InstallmentPlans.DeleteAsync(existing);
                }

                var installments = new List<InstallmentPlan>();
                decimal installmentAmount = offer.FinalPrice.Value / dto.NumberOfInstallments;
                DateTime dueDate = dto.StartDate;

                for (int i = 0; i < dto.NumberOfInstallments; i++)
                {
                    var installment = new InstallmentPlan
                    {
                        OfferId = offerId,
                        InstallmentNumber = i + 1,
                        Amount = i == dto.NumberOfInstallments - 1 
                            ? offer.FinalPrice.Value - (installmentAmount * (dto.NumberOfInstallments - 1))
                            : installmentAmount,
                        DueDate = dueDate,
                        Status = "Pending"
                    };

                    if (dto.PaymentFrequency == "Monthly")
                        dueDate = dueDate.AddMonths(1);
                    else if (dto.PaymentFrequency == "Quarterly")
                        dueDate = dueDate.AddMonths(3);
                    else if (dto.PaymentFrequency == "Weekly")
                        dueDate = dueDate.AddDays(7);

                    installments.Add(installment);
                    await _unitOfWork.InstallmentPlans.CreateAsync(installment);
                }

                await _unitOfWork.SaveChangesAsync();

                return installments.Select(i => new InstallmentPlanDTO
                {
                    Id = i.Id,
                    OfferId = i.OfferId,
                    InstallmentNumber = i.InstallmentNumber,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status,
                    Notes = i.Notes
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating installment plan. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        #endregion

        #region Enhanced Offer - Complete Details

        public async Task<EnhancedOfferResponseDTO> GetEnhancedOfferAsync(long offerId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    return null;

                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var creator = await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy);
                var salesman = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);

                var equipment = await GetEquipmentListAsync(offerId);
                var terms = await _unitOfWork.OfferTerms.GetByOfferIdAsync(offerId);
                var installments = (await _unitOfWork.InstallmentPlans.GetByOfferIdAsync(offerId)).ToList();

                return new EnhancedOfferResponseDTO
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
                    CreatedAt = offer.CreatedAt,
                    PaymentType = offer.PaymentType,
                    FinalPrice = offer.FinalPrice,
                    OfferDuration = offer.OfferDuration,
                    Equipment = equipment,
                    Terms = terms != null ? new OfferTermsDTO
                    {
                        Id = terms.Id,
                        OfferId = terms.OfferId,
                        WarrantyPeriod = terms.WarrantyPeriod,
                        DeliveryTime = terms.DeliveryTime,
                        MaintenanceTerms = terms.MaintenanceTerms,
                        OtherTerms = terms.OtherTerms
                    } : null,
                    Installments = installments.Select(i => new InstallmentPlanDTO
                    {
                        Id = i.Id,
                        OfferId = i.OfferId,
                        InstallmentNumber = i.InstallmentNumber,
                        Amount = i.Amount,
                        DueDate = i.DueDate,
                        Status = i.Status,
                        Notes = i.Notes
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enhanced offer. OfferId: {OfferId}", offerId);
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
