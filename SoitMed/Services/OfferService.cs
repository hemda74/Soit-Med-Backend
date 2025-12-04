using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing offers in the sales workflow
    /// </summary>
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IDealService _dealService;
        private readonly ILogger<OfferService> _logger;

        public OfferService(
            IUnitOfWork unitOfWork, 
            UserManager<ApplicationUser> userManager, 
            INotificationService notificationService,
            IDealService dealService,
            ILogger<OfferService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _dealService = dealService;
            _logger = logger;
        }

        #region Offer Management

        public async Task<OfferResponseDTO> CreateOfferFromRequestAsync(CreateOfferDTO createOfferDto, string userId)
        {
            try
            {
                // If OfferRequestId is provided, validate and link it
                OfferRequest? offerRequest = null;
                string assignedTo = createOfferDto.AssignedTo;

                if (createOfferDto.OfferRequestId.HasValue)
                {
                    offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(createOfferDto.OfferRequestId.Value);
                    if (offerRequest == null)
                        throw new ArgumentException("Offer request not found", nameof(createOfferDto.OfferRequestId));

                    // Determine who the offer should be assigned to:
                    // - If AssignedTo is provided in DTO, use it (SalesSupport can assign to specific Salesman/Customer)
                    // - Otherwise, assign to the requester (Salesman or Customer who created the request)
                    if (string.IsNullOrEmpty(assignedTo))
                    {
                        assignedTo = offerRequest.RequestedBy;
                    }
                }
                else
                {
                    // If no OfferRequestId, AssignedTo must be provided
                    if (string.IsNullOrEmpty(createOfferDto.AssignedTo))
                        throw new ArgumentException("AssignedTo is required when OfferRequestId is not provided", nameof(createOfferDto.AssignedTo));
                    
                    assignedTo = createOfferDto.AssignedTo;
                }

                var offer = new SalesOffer
                {
                    OfferRequestId = createOfferDto.OfferRequestId,
                    ClientId = createOfferDto.ClientId,
                    CreatedBy = userId, // SalesSupport/SalesManager who creates the offer
                    AssignedTo = assignedTo, // Salesman or Customer who requested it
                    Products = createOfferDto.Products,
                    TotalAmount = createOfferDto.TotalAmount,
                    PaymentTerms = createOfferDto.PaymentTerms != null && createOfferDto.PaymentTerms.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.PaymentTerms) 
                        : null,
                    DeliveryTerms = createOfferDto.DeliveryTerms != null && createOfferDto.DeliveryTerms.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.DeliveryTerms) 
                        : null,
                    WarrantyTerms = createOfferDto.WarrantyTerms != null && createOfferDto.WarrantyTerms.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.WarrantyTerms) 
                        : null,
                    ValidUntil = createOfferDto.ValidUntil != null && createOfferDto.ValidUntil.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.ValidUntil) 
                        : null,
                    Notes = createOfferDto.Notes,
                    PaymentType = createOfferDto.PaymentType,
                    FinalPrice = createOfferDto.FinalPrice,
                    OfferDuration = createOfferDto.OfferDuration,
                    Status = OfferStatus.PendingSalesManagerApproval // Require SalesManager approval before sending to Salesman
                };

                await _unitOfWork.SalesOffers.CreateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Mark offer as pending SalesManager approval
                offer.MarkAsPendingSalesManagerApproval();
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity for new offer creation (from request)
                await SaveRecentActivityAsync(offer, null);

                // Auto-add equipment based on products (parse from Products field or add default)
                try
                {
                    await AutoAddEquipmentFromProductsAsync(offer.Id, createOfferDto.Products);
                    _logger.LogInformation("Equipment auto-added to offer. OfferId: {OfferId}", offer.Id);
                }
                catch (Exception equipEx)
                {
                    // Log but don't fail offer creation
                    _logger.LogWarning(equipEx, "Failed to auto-add equipment to offer {OfferId}", offer.Id);
                }

                // Update offer request status and link the created offer (if linked to a request)
                if (offerRequest != null)
                {
                    offerRequest.MarkAsCompleted($"Offer created with ID: {offer.Id}", offer.Id);
                    await _unitOfWork.OfferRequests.UpdateAsync(offerRequest);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Send notification to all SalesManager users for approval
                try
                {
                    await SendSalesManagerApprovalNotificationAsync(offer);
                    _logger.LogInformation("SalesManager approval notification sent for offer. OfferId: {OfferId}", offer.Id);
                }
                catch (Exception notifEx)
                {
                    // Log but don't fail offer creation
                    _logger.LogWarning(notifEx, "Failed to send SalesManager approval notification for offer {OfferId}", offer.Id);
                }

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
                // Use CreateOfferFromRequestAsync - it handles both with and without OfferRequestId
                return await CreateOfferFromRequestAsync(createOfferDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer");
                throw;
            }
        }

        public async Task<OfferResponseDTO> CreateOfferWithItemsAsync(CreateOfferWithItemsDTO createOfferDto, string userId)
        {
            try
            {
                // REQUIRE OfferRequestId - offers can only be created from requests
                if (!createOfferDto.OfferRequestId.HasValue)
                {
                    throw new ArgumentException("OfferRequestId is required. Offers can only be created from offer requests initiated by Salesman or Customer.", nameof(createOfferDto.OfferRequestId));
                }

                // Get the offer request to link the offer properly
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(createOfferDto.OfferRequestId.Value);
                if (offerRequest == null)
                    throw new ArgumentException("Offer request not found", nameof(createOfferDto.OfferRequestId));

                // Determine who the offer should be assigned to:
                // - If AssignedTo is provided in DTO, use it (SalesSupport can assign to specific Salesman/Customer)
                // - Otherwise, assign to the requester (Salesman or Customer who created the request)
                var assignedTo = !string.IsNullOrEmpty(createOfferDto.AssignedTo) 
                    ? createOfferDto.AssignedTo 
                    : offerRequest.RequestedBy;

                var offer = new SalesOffer
                {
                    OfferRequestId = createOfferDto.OfferRequestId, // REQUIRED - link to offer request
                    ClientId = createOfferDto.ClientId,
                    CreatedBy = userId, // SalesSupport/SalesManager who creates the offer
                    AssignedTo = assignedTo, // Salesman or Customer who requested it
                    Products = string.Join(", ", createOfferDto.Products.Select(p => p.Name)),
                    TotalAmount = createOfferDto.TotalAmount,
                    PaymentTerms = createOfferDto.PaymentTerms != null && createOfferDto.PaymentTerms.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.PaymentTerms) 
                        : null,
                    DeliveryTerms = createOfferDto.DeliveryTerms != null && createOfferDto.DeliveryTerms.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.DeliveryTerms) 
                        : null,
                    WarrantyTerms = createOfferDto.WarrantyTerms != null && createOfferDto.WarrantyTerms.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.WarrantyTerms) 
                        : null,
                    ValidUntil = createOfferDto.ValidUntil != null && createOfferDto.ValidUntil.Count > 0 
                        ? JsonSerializer.Serialize(createOfferDto.ValidUntil) 
                        : null,
                    Notes = createOfferDto.Notes,
                    PaymentType = createOfferDto.PaymentType,
                    FinalPrice = createOfferDto.FinalPrice,
                    OfferDuration = createOfferDto.OfferDuration,
                    Status = OfferStatus.PendingSalesManagerApproval // Require SalesManager approval before sending to Salesman
                };

                await _unitOfWork.SalesOffers.CreateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Mark offer as pending SalesManager approval
                offer.MarkAsPendingSalesManagerApproval();
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity for new offer creation
                await SaveRecentActivityAsync(offer, null);

                // Map products to equipment
                foreach (var item in createOfferDto.Products)
                {
                    var equipment = new OfferEquipment
                    {
                        OfferId = offer.Id,
                        Name = item.Name,
                        Model = item.Model,
                        Provider = item.Factory,
                        Country = item.Country,
                        Year = item.Year,
                        Price = item.Price,
                        Description = item.Description,
                        InStock = item.InStock,
                        ImagePath = !string.IsNullOrWhiteSpace(item.ImageUrl) ? item.ImageUrl : $"offers/{offer.Id}/equipment-placeholder.png",
                        ProviderImagePath = item.ProviderImagePath
                    };

                    await _unitOfWork.OfferEquipment.CreateAsync(equipment);
                }

                await _unitOfWork.SaveChangesAsync();

                // Update offer request status and link the created offer (REQUIRED - always linked)
                offerRequest.MarkAsCompleted($"Offer created with ID: {offer.Id}", offer.Id);
                await _unitOfWork.OfferRequests.UpdateAsync(offerRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer with items created successfully. OfferId: {OfferId}", offer.Id);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer with items");
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
                // OPTIMIZED: Single method call loads offers + related data in O(1) queries (3 queries total)
                var (offers, clientsDict, usersDict) = await _unitOfWork.SalesOffers
                    .GetOffersByClientIdWithRelatedDataAsync(clientId);

                // Map synchronously using pre-loaded data - O(n) in-memory operation
                return offers.Select(o => MapToOfferResponseDTO(o, clientsDict, usersDict)).ToList();
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
                // OPTIMIZED: Single method call loads offers + related data in O(1) queries (3 queries total)
                var (offers, clientsDict, usersDict) = await _unitOfWork.SalesOffers
                    .GetOffersBySalesmanWithRelatedDataAsync(salesmanId);

                // Map synchronously using pre-loaded data - O(n) in-memory operation
                return offers.Select(o => MapToOfferResponseDTO(o, clientsDict, usersDict)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by salesman. SalesmanId: {SalesmanId}", salesmanId);
                throw;
            }
        }

        public async Task<List<OfferResponseDTO>> GetOffersByCustomerAsync(string customerId)
        {
            try
            {
                if (!long.TryParse(customerId, out var clientId))
                {
                    throw new ArgumentException("CustomerId must be a numeric value that maps to ClientId.", nameof(customerId));
                }

                // OPTIMIZED: Single method call loads offers + related data in O(1) queries (3 queries total)
                var (offers, clientsDict, usersDict) = await _unitOfWork.SalesOffers
                    .GetOffersByClientIdWithRelatedDataAsync(clientId);

                // Map synchronously using pre-loaded data - O(n) in-memory operation
                return offers.Select(o => MapToOfferResponseDTO(o, clientsDict, usersDict)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by customer. CustomerId: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<List<OfferResponseDTO>> GetOffersByStatusAsync(string status)
        {
            try
            {
                // OPTIMIZED: Single method call loads offers + related data in O(1) queries (3 queries total)
                var (offers, clientsDict, usersDict) = await _unitOfWork.SalesOffers
                    .GetOffersByStatusWithRelatedDataAsync(status);

                // Map synchronously using pre-loaded data - O(n) in-memory operation
                return offers.Select(o => MapToOfferResponseDTO(o, clientsDict, usersDict)).ToList();
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
                // OPTIMIZATION: Pre-load all related data in batches, then map synchronously
                return await MapOffersToDTOsAsync(offers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offers by creator. CreatorId: {CreatorId}", creatorId);
                throw;
            }
        }

        public async Task<PaginatedOffersResponseDTO> GetAllOffersWithFiltersAsync(
            string? status,
            string? salesmanId,
            int page,
            int pageSize,
            DateTime? startDate,
            DateTime? endDate)
        {
            try
            {
                var query = _unitOfWork.SalesOffers.GetQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(o => o.Status == status);
                }

                if (!string.IsNullOrEmpty(salesmanId))
                {
                    query = query.Where(o => o.AssignedTo == salesmanId);
                }

                // Apply date filters
                if (startDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= endDate.Value);
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination and ordering (default: last 10 offers)
                var offers = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // OPTIMIZATION: Pre-load all related data in batches, then map synchronously
                var offerDTOs = await MapOffersToDTOsAsync(offers);

                return new PaginatedOffersResponseDTO
                {
                    Offers = offerDTOs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all offers with filters");
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

                // Store old status to track changes
                var oldStatus = offer.Status;

                // Only allow updates if offer can be modified (Draft or NeedsModification)
                if (!offer.CanBeModified())
                    throw new InvalidOperationException("Only draft or needs-modification offers can be updated");

                // If offer was in NeedsModification, automatically change to Draft after update
                // This ensures the workflow: NeedsModification -> (SalesSupport edits) -> Draft -> PendingSalesManagerApproval -> Sent
                bool wasNeedsModification = offer.Status == OfferStatus.NeedsModification;

                offer.Products = updateOfferDto.Products;
                offer.TotalAmount = updateOfferDto.TotalAmount;
                offer.PaymentTerms = updateOfferDto.PaymentTerms != null && updateOfferDto.PaymentTerms.Count > 0 
                    ? JsonSerializer.Serialize(updateOfferDto.PaymentTerms) 
                    : null;
                offer.DeliveryTerms = updateOfferDto.DeliveryTerms != null && updateOfferDto.DeliveryTerms.Count > 0 
                    ? JsonSerializer.Serialize(updateOfferDto.DeliveryTerms) 
                    : null;
                offer.WarrantyTerms = updateOfferDto.WarrantyTerms != null && updateOfferDto.WarrantyTerms.Count > 0 
                    ? JsonSerializer.Serialize(updateOfferDto.WarrantyTerms) 
                    : null;
                offer.ValidUntil = updateOfferDto.ValidUntil != null && updateOfferDto.ValidUntil.Count > 0 
                    ? JsonSerializer.Serialize(updateOfferDto.ValidUntil) 
                    : null;
                offer.Notes = updateOfferDto.Notes;
                offer.UpdatedAt = DateTime.UtcNow;

                // Automatically change from NeedsModification to Draft when SalesSupport updates
                if (wasNeedsModification)
                {
                    offer.Status = OfferStatus.Draft;
                    _logger.LogInformation("Offer automatically changed from NeedsModification to Draft after update. OfferId: {OfferId}", offerId);
                }

                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity if status changed
                if (oldStatus != offer.Status)
                {
                    await SaveRecentActivityAsync(offer, oldStatus);
                }

                _logger.LogInformation("Offer updated successfully. OfferId: {OfferId}", offerId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        /// <summary>
        /// Update offer by SalesManager - allows modification regardless of status
        /// </summary>
        public async Task<OfferResponseDTO> UpdateOfferBySalesManagerAsync(long offerId, UpdateOfferDTO updateDto, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                // Verify user is SalesManager or SuperAdmin
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new UnauthorizedAccessException("User not found");

                var userRoles = await _userManager.GetRolesAsync(user);
                bool isSalesManager = userRoles.Contains("SalesManager") || userRoles.Contains("SuperAdmin");
                
                if (!isSalesManager)
                    throw new UnauthorizedAccessException("Only SalesManager or SuperAdmin can update offers using this method");

                // Store old status to track changes
                var oldStatus = offer.Status;

                // Update only provided fields (PATCH semantics)
                if (updateDto.ClientId.HasValue)
                    offer.ClientId = updateDto.ClientId.Value;

                if (!string.IsNullOrEmpty(updateDto.AssignedTo))
                    offer.AssignedTo = updateDto.AssignedTo;

                if (!string.IsNullOrEmpty(updateDto.Products))
                    offer.Products = updateDto.Products;

                if (updateDto.TotalAmount.HasValue)
                    offer.TotalAmount = updateDto.TotalAmount.Value;

                if (updateDto.PaymentTerms != null)
                    offer.PaymentTerms = updateDto.PaymentTerms.Count > 0 
                        ? JsonSerializer.Serialize(updateDto.PaymentTerms) 
                        : null;

                if (updateDto.DeliveryTerms != null)
                    offer.DeliveryTerms = updateDto.DeliveryTerms.Count > 0 
                        ? JsonSerializer.Serialize(updateDto.DeliveryTerms) 
                        : null;

                if (updateDto.WarrantyTerms != null)
                    offer.WarrantyTerms = updateDto.WarrantyTerms.Count > 0 
                        ? JsonSerializer.Serialize(updateDto.WarrantyTerms) 
                        : null;

                if (updateDto.ValidUntil != null)
                    offer.ValidUntil = updateDto.ValidUntil.Count > 0 
                        ? JsonSerializer.Serialize(updateDto.ValidUntil) 
                        : null;

                if (updateDto.Notes != null)
                    offer.Notes = updateDto.Notes;

                if (!string.IsNullOrEmpty(updateDto.PaymentType))
                    offer.PaymentType = updateDto.PaymentType;

                if (updateDto.FinalPrice.HasValue)
                    offer.FinalPrice = updateDto.FinalPrice.Value;

                if (!string.IsNullOrEmpty(updateDto.OfferDuration))
                    offer.OfferDuration = updateDto.OfferDuration;

                offer.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity
                await SaveRecentActivityAsync(offer, oldStatus);

                _logger.LogInformation("Offer updated by SalesManager. OfferId: {OfferId}, UpdatedBy: {UserId}", offerId, userId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer by SalesManager. OfferId: {OfferId}", offerId);
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

                // Get client info for notification
                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                // Check if offer can be sent to salesman
                // Only allow if offer has been approved by SalesManager (status is "Sent")
                if (offer.Status != OfferStatus.Sent)
                {
                    var errorMessage = offer.Status == OfferStatus.PendingSalesManagerApproval
                        ? "Offer must be approved by SalesManager before it can be sent to salesman. Current status: PendingSalesManagerApproval"
                        : $"Only approved (Sent) offers can be sent to salesman. Current status: {offer.Status}";
                    throw new InvalidOperationException(errorMessage);
                }

                // Check if assigned user is Customer or Salesman
                var assignedUser = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);
                var assignedUserRoles = assignedUser != null ? await _userManager.GetRolesAsync(assignedUser) : new List<string>();
                bool isCustomer = assignedUserRoles.Contains("Customer");
                bool isSalesman = assignedUserRoles.Contains("Salesman");

                // Store old status for activity tracking
                var wasEverSent = offer.SentToClientAt.HasValue;
                var previousStatusForActivity = wasEverSent
                    ? offer.Status
                    : OfferStatus.PendingSalesManagerApproval;

                    offer.SentToClientAt = DateTime.UtcNow;
                    offer.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SalesOffers.UpdateAsync(offer);
                
                // Update OfferRequest status to Sent (if linked to a request)
                if (offer.OfferRequestId.HasValue)
                {
                    var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(offer.OfferRequestId.Value);
                    if (offerRequest != null && offerRequest.Status != "Sent")
                    {
                        offerRequest.MarkAsSent();
                        await _unitOfWork.OfferRequests.UpdateAsync(offerRequest);
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity (status changed to Sent, or resend if already Sent)
                await SaveRecentActivityAsync(offer, previousStatusForActivity);

                if (wasEverSent)
                {
                    _logger.LogInformation("Offer {OfferId} notification resent to salesman. Previous status was already Sent.", offerId);
                }

                _logger.LogInformation("Offer sent to salesman successfully. OfferId: {OfferId}", offerId);

                // Send notification to assigned user (Salesman or Customer)
                if (!string.IsNullOrEmpty(offer.AssignedTo))
                {
                    try
                    {
                        // Reuse the assignedUser variable from above
                        if (assignedUser != null && assignedUser.IsActive)
                        {
                            var creatorInfo = await _unitOfWork.Users.GetByIdAsync(userId);
                            var creatorName = creatorInfo != null ? $"{creatorInfo.FirstName} {creatorInfo.LastName}" : "Sales Support";

                            var notificationTitle = isCustomer ? "New Offer Received" : "New Offer Available";
                            var notificationMessage = isCustomer
                                ? $"You have received a new offer for {clientName}. Total: {offer.TotalAmount:C}"
                                : $"{creatorName} has sent you an offer for {clientName}. Total: {offer.TotalAmount:C}";

                            // Add offerId to metadata for easy navigation
                            var metadata = new Dictionary<string, object>
                            {
                                ["offerId"] = offerId,
                                ["clientName"] = clientName,
                                ["totalAmount"] = offer.TotalAmount
                            };

                            await _notificationService.CreateNotificationAsync(
                                offer.AssignedTo,
                                notificationTitle,
                                notificationMessage,
                                "Offer",
                                "High",
                                null,
                                null,
                                true, // Mobile push notification
                                metadata, // Pass metadata with offerId
                                CancellationToken.None
                            );

                            var userType = isCustomer ? "Customer" : "Salesman";
                            _logger.LogInformation("  Notification sent to {UserType}: {UserId} for Offer: {OfferId}", userType, offer.AssignedTo, offerId);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Assigned user {UserId} not found or inactive. Notification not sent for Offer: {OfferId}", offer.AssignedTo, offerId);
                        }
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "❌ Failed to send notification to assigned user {UserId} for Offer: {OfferId}", offer.AssignedTo, offerId);
                        // Don't fail the whole operation if notification fails
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Offer {OfferId} has no assigned user. Notification not sent.", offerId);
                }

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending offer to salesman. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> SalesManagerApprovalAsync(long offerId, ApproveOfferDTO approvalDto, string salesManagerId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                if (offer.Status != OfferStatus.PendingSalesManagerApproval)
                    throw new InvalidOperationException("Offer is not pending SalesManager approval");

                if (approvalDto.Approved)
                {
                    // Approve and mark as Sent (ready to send to salesman)
                    offer.ApproveBySalesManager(salesManagerId, approvalDto.Comments);
                }
                else
                {
                    if (string.IsNullOrEmpty(approvalDto.RejectionReason))
                        throw new ArgumentException("Rejection reason is required when rejecting an offer");

                    offer.RejectBySalesManager(salesManagerId, approvalDto.RejectionReason, approvalDto.Comments);
                }

                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save activity
                await SaveRecentActivityAsync(offer, OfferStatus.PendingSalesManagerApproval);

                _logger.LogInformation("SalesManager approval processed for offer. OfferId: {OfferId}, Approved: {Approved}", 
                    offerId, approvalDto.Approved);

                // FIX 1: Auto-send notification to customer after approval
                if (approvalDto.Approved && offer.Status == OfferStatus.Sent)
                {
                    try
                    {
                        var assignedUser = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);
                        if (assignedUser != null && assignedUser.IsActive)
                        {
                            var assignedUserRoles = await _userManager.GetRolesAsync(assignedUser);
                            bool isCustomer = assignedUserRoles.Contains("Customer");
                            
                            var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                            var clientName = client?.Name ?? "Unknown Client";
                            
                            var notificationTitle = isCustomer ? "New Offer Received" : "New Offer Available";
                            var notificationMessage = isCustomer
                                ? $"You have received a new offer for {clientName}. Total: {offer.TotalAmount:C}"
                                : $"A new offer for {clientName} has been approved. Total: {offer.TotalAmount:C}";
                            
                            var metadata = new Dictionary<string, object>
                            {
                                ["offerId"] = offerId,
                                ["clientName"] = clientName,
                                ["totalAmount"] = offer.TotalAmount
                            };
                            
                            await _notificationService.CreateNotificationAsync(
                                offer.AssignedTo,
                                notificationTitle,
                                notificationMessage,
                                "Offer",
                                "High",
                                null,
                                null,
                                true, // Mobile push
                                metadata,
                                CancellationToken.None
                            );
                            
                            // Update SentToClientAt timestamp
                            offer.SentToClientAt = DateTime.UtcNow;
                            await _unitOfWork.SalesOffers.UpdateAsync(offer);
                            await _unitOfWork.SaveChangesAsync();
                            
                            _logger.LogInformation("✅ Offer automatically sent to customer after approval. OfferId: {OfferId}", offerId);
                        }
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "❌ Failed to send notification after approval. OfferId: {OfferId}", offerId);
                    }
                }
                
                // FIX 4: Notify customer of rejection
                if (!approvalDto.Approved && offer.Status == OfferStatus.Rejected)
                {
                    try
                    {
                        var assignedUser = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);
                        if (assignedUser != null && assignedUser.IsActive)
                        {
                            var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                            var clientName = client?.Name ?? "Unknown Client";
                            
                            await _notificationService.CreateNotificationAsync(
                                offer.AssignedTo,
                                "Offer Rejected",
                                $"Your offer request for {clientName} has been rejected. Reason: {approvalDto.RejectionReason}",
                                "Offer",
                                "High",
                                null,
                                null,
                                true,
                                new Dictionary<string, object>
                                {
                                    ["offerId"] = offerId,
                                    ["clientName"] = clientName,
                                    ["rejectionReason"] = approvalDto.RejectionReason ?? ""
                                },
                                CancellationToken.None
                            );
                            
                            _logger.LogInformation("✅ Customer notified of offer rejection. OfferId: {OfferId}", offerId);
                        }
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "❌ Failed to notify customer of rejection. OfferId: {OfferId}", offerId);
                    }
                }

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SalesManager approval. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<List<OfferResponseDTO>> GetPendingSalesManagerApprovalsAsync()
        {
            try
            {
                var offers = await _unitOfWork.SalesOffers.GetOffersByStatusAsync(OfferStatus.PendingSalesManagerApproval);
                var result = new List<OfferResponseDTO>();

                foreach (var offer in offers)
                {
                    result.Add(await MapToOfferResponseDTO(offer));
                }

                return result.OrderByDescending(o => o.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending SalesManager approvals");
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

                // Verify user can respond to this offer (must be assigned to them if Customer or Salesman)
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    bool isCustomerOrSalesman = userRoles.Contains("Customer") || userRoles.Contains("Salesman");
                    bool isManagerOrAdmin = userRoles.Contains("SalesManager") || userRoles.Contains("SuperAdmin") || userRoles.Contains("SalesSupport");

                    // If user is Customer or Salesman (and not a Manager/Admin), verify they are assigned to this offer
                    if (isCustomerOrSalesman && !isManagerOrAdmin)
                    {
                        if (string.IsNullOrEmpty(offer.AssignedTo) || offer.AssignedTo != userId)
                        {
                            throw new UnauthorizedAccessException("You can only respond to offers assigned to you");
                        }
                    }
                }

                var oldStatus = offer.Status;
                offer.RecordClientResponse(response, accepted);
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity (status changed to Accepted or Rejected)
                await SaveRecentActivityAsync(offer, oldStatus);

                // If client accepted the offer, automatically create a deal and send for approval
                if (accepted && offer.Status == "Accepted")
                {
                    try
                    {
                        // Check if deal already exists for this offer
                        var dealsQuery = _unitOfWork.SalesDeals.GetQueryable()
                            .Where(d => d.OfferId == offerId);
                        var existingDeals = dealsQuery.ToList();
                        var existingDeal = existingDeals.FirstOrDefault();

                        if (existingDeal == null)
                        {
                            // Create deal automatically from accepted offer
                            var createDealDto = new CreateDealDTO
                            {
                                OfferId = offer.Id,
                                ClientId = offer.ClientId,
                                DealValue = offer.TotalAmount,
                                PaymentTerms = offer.PaymentTerms,
                                DeliveryTerms = offer.DeliveryTerms,
                                ExpectedDeliveryDate = ParseDeliveryTermsToDate(offer.DeliveryTerms),
                                Notes = $"Auto-created from accepted Offer #{offer.Id}. Client response: {response}"
                            };

                            await _dealService.CreateDealAsync(createDealDto, userId);
                            _logger.LogInformation("Deal automatically created from accepted offer. OfferId: {OfferId}", offerId);
                            
                            // FIX 3: Notify SuperAdmin of new deal
                            try
                            {
                                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                                var clientName = client?.Name ?? "Unknown Client";
                                
                                foreach (var admin in superAdmins.Where(a => a.IsActive))
                                {
                                    await _notificationService.CreateNotificationAsync(
                                        admin.Id,
                                        "New Deal Pending Approval",
                                        $"A new deal has been created from accepted offer #{offerId} for {clientName}. Value: {offer.TotalAmount:C}",
                                        "Deal",
                                        "High",
                                        null,
                                        null,
                                        true,
                                        new Dictionary<string, object>
                                        {
                                            ["offerId"] = offerId,
                                            ["clientName"] = clientName,
                                            ["dealValue"] = offer.TotalAmount
                                        },
                                        CancellationToken.None
                                    );
                                }
                                
                                _logger.LogInformation("✅ SuperAdmin notified of new deal from accepted offer. OfferId: {OfferId}", offerId);
                            }
                            catch (Exception notifEx)
                            {
                                _logger.LogError(notifEx, "❌ Failed to notify SuperAdmin of new deal. OfferId: {OfferId}", offerId);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Deal already exists for offer. OfferId: {OfferId}, DealId: {DealId}", offerId, existingDeal.Id);
                        }
                    }
                    catch (Exception dealEx)
                    {
                        // Log error but don't fail the offer update
                        _logger.LogError(dealEx, "Failed to auto-create deal for accepted offer. OfferId: {OfferId}", offerId);
                    }
                }

                // FIX 2: Notify SalesSupport and original requester after customer response
                try
                {
                    var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                    var clientName = client?.Name ?? "Unknown Client";
                    
                    var responseStatus = accepted ? "accepted" : "rejected";
                    var notificationTitle = $"Offer {responseStatus.ToUpper()}";
                    var notificationMessage = $"Customer has {responseStatus} offer #{offerId} for {clientName}. Response: {response}";
                    
                    var metadata = new Dictionary<string, object>
                    {
                        ["offerId"] = offerId,
                        ["clientName"] = clientName,
                        ["accepted"] = accepted,
                        ["response"] = response ?? ""
                    };
                    
                    // Notify SalesSupport users
                    var salesSupportUsers = await _userManager.GetUsersInRoleAsync("SalesSupport");
                    foreach (var supportUser in salesSupportUsers.Where(u => u.IsActive))
                    {
                        await _notificationService.CreateNotificationAsync(
                            supportUser.Id,
                            notificationTitle,
                            notificationMessage,
                            "Offer",
                            "High",
                            null,
                            null,
                            true,
                            metadata,
                            CancellationToken.None
                        );
                    }
                    
                    // Notify original requester if different from customer
                    if (offer.OfferRequestId.HasValue)
                    {
                        var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(offer.OfferRequestId.Value);
                        if (offerRequest != null && offerRequest.RequestedBy != userId)
                        {
                            await _notificationService.CreateNotificationAsync(
                                offerRequest.RequestedBy,
                                notificationTitle,
                                notificationMessage,
                                "Offer",
                                "High",
                                null,
                                null,
                                true,
                                metadata,
                                CancellationToken.None
                            );
                        }
                    }
                    
                    _logger.LogInformation("✅ Customer response notifications sent. OfferId: {OfferId}", offerId);
                }
                catch (Exception notifEx)
                {
                    _logger.LogError(notifEx, "❌ Failed to send customer response notifications. OfferId: {OfferId}", offerId);
                }

                _logger.LogInformation("Client response recorded for offer. OfferId: {OfferId}, Accepted: {Accepted}", offerId, accepted);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording client response. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> CompleteOfferAsync(long offerId, string? completionNotes, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                // Only allow completion if offer is Accepted
                if (offer.Status != OfferStatus.Accepted)
                    throw new InvalidOperationException("Only accepted offers can be completed");

                var oldStatus = offer.Status;
                offer.Status = OfferStatus.Completed;
                offer.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(completionNotes))
                {
                    offer.Notes = string.IsNullOrEmpty(offer.Notes) 
                        ? completionNotes 
                        : $"{offer.Notes}\n\nCompletion Notes: {completionNotes}";
                }

                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity (status changed to Completed)
                await SaveRecentActivityAsync(offer, oldStatus);

                _logger.LogInformation("Offer marked as completed. OfferId: {OfferId}", offerId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing offer. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> MarkAsNeedsModificationAsync(long offerId, string? reason, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                // Only allow if offer is in Draft or Sent status
                if (offer.Status != OfferStatus.Draft && offer.Status != OfferStatus.Sent)
                    throw new InvalidOperationException("Only draft or sent offers can be marked as needing modification");

                // Check if user is a Salesman or Customer - if so, verify they are assigned to this offer
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    bool isSalesmanOrCustomer = userRoles.Contains("Salesman") || userRoles.Contains("Customer");
                    bool isManagerOrAdmin = userRoles.Contains("SalesManager") || userRoles.Contains("SuperAdmin") || userRoles.Contains("SalesSupport");

                    // If user is a Salesman or Customer (and not a Manager/Admin), verify they are assigned to this offer
                    if (isSalesmanOrCustomer && !isManagerOrAdmin)
                    {
                        if (string.IsNullOrEmpty(offer.AssignedTo) || offer.AssignedTo != userId)
                        {
                            throw new UnauthorizedAccessException("You can only request modifications for offers assigned to you");
                        }
                    }
                }

                var oldStatus = offer.Status;
                offer.MarkAsNeedsModification(reason);
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity
                await SaveRecentActivityAsync(offer, oldStatus);

                // If requested by Salesman, send notification to SalesSupport/SalesManager
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Contains("Salesman") && !userRoles.Contains("SalesManager") && !userRoles.Contains("SuperAdmin"))
                    {
                        // Get client info for notification
                        var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                        var clientName = client?.Name ?? "Unknown Client";
                        var salesmanName = $"{user.FirstName} {user.LastName}".Trim();

                        var notificationTitle = "Offer Modification Requested";
                        var notificationMessage = $"Salesman {salesmanName} has requested modifications for Offer #{offerId} (Client: {clientName}). Reason: {reason ?? "No reason provided"}";

                        var metadata = new Dictionary<string, object>
                        {
                            ["offerId"] = offerId,
                            ["clientName"] = clientName,
                            ["salesmanName"] = salesmanName,
                            ["reason"] = reason ?? ""
                        };

                        // Send notification to all SalesSupport and SalesManager users
                        var supportUsers = await _userManager.GetUsersInRoleAsync("SalesSupport");
                        var managerUsers = await _userManager.GetUsersInRoleAsync("SalesManager");
                        var allRecipients = supportUsers.Concat(managerUsers).Where(u => u.IsActive).Distinct().ToList();

                        foreach (var recipient in allRecipients)
                        {
                            try
                            {
                                await _notificationService.CreateNotificationAsync(
                                    recipient.Id,
                                    notificationTitle,
                                    notificationMessage,
                                    "Offer",
                                    "High",
                                    null,
                                    null,
                                    true,
                                    metadata,
                                    CancellationToken.None
                                );
                            }
                            catch (Exception notifEx)
                            {
                                _logger.LogWarning(notifEx, "Failed to send notification to {UserId} for modification request", recipient.Id);
                            }
                        }

                        _logger.LogInformation("Modification request notification sent to SalesSupport/Managers for Offer: {OfferId}", offerId);
                    }
                }

                _logger.LogInformation("Offer marked as needing modification. OfferId: {OfferId}, RequestedBy: {UserId}", offerId, userId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking offer as needing modification. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> MarkAsUnderReviewAsync(long offerId, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                // Only allow if offer is in Sent status
                if (offer.Status != OfferStatus.Sent)
                    throw new InvalidOperationException("Only sent offers can be marked as under review");

                var oldStatus = offer.Status;
                offer.MarkAsUnderReview();
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity
                await SaveRecentActivityAsync(offer, oldStatus);

                _logger.LogInformation("Offer marked as under review. OfferId: {OfferId}", offerId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking offer as under review. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        /// <summary>
        /// Resume offer from UnderReview back to Sent status
        /// </summary>
        public async Task<OfferResponseDTO> ResumeFromUnderReviewAsync(long offerId, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                // Only allow if offer is in UnderReview status
                if (offer.Status != OfferStatus.UnderReview)
                    throw new InvalidOperationException("Only offers under review can be resumed to sent status");

                var oldStatus = offer.Status;
                offer.MarkAsSent();
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                // Save recent activity
                await SaveRecentActivityAsync(offer, oldStatus);

                _logger.LogInformation("Offer resumed from under review to sent. OfferId: {OfferId}", offerId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming offer from under review. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<int> UpdateExpiredOffersAsync()
        {
            try
            {
                var offers = await _unitOfWork.SalesOffers.GetQueryable()
                    .Where(o => o.Status != OfferStatus.Expired
                             && o.Status != OfferStatus.Accepted
                             && o.Status != OfferStatus.Completed
                             && o.Status != OfferStatus.Rejected)
                    .ToListAsync();

                int expiredCount = 0;
                foreach (var offer in offers)
                {
                    if (offer.IsExpired())
                    {
                        var oldStatus = offer.Status;
                        offer.MarkAsExpired();
                        await _unitOfWork.SalesOffers.UpdateAsync(offer);
                        await SaveRecentActivityAsync(offer, oldStatus);
                        expiredCount++;
                    }
                }

                if (expiredCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Updated {Count} offers to expired status", expiredCount);
                }

                return expiredCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expired offers");
                throw;
            }
        }

        /// <summary>
        /// Saves a recent activity and maintains only the last 20 activities in the database
        /// Tracks ALL offer status changes, not just specific operations
        /// </summary>
        private async Task SaveRecentActivityAsync(SalesOffer offer, string? oldStatus = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentStatus = offer.Status;
                
                // Skip if status hasn't changed (unless it's a new offer)
                if (oldStatus != null && oldStatus == currentStatus)
                    return;

                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var salesman = !string.IsNullOrEmpty(offer.AssignedTo) 
                    ? await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo) 
                    : null;
                var creator = !string.IsNullOrEmpty(offer.CreatedBy)
                    ? await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy)
                    : null;

                var salesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}".Trim() : null;
                var clientName = client?.Name ?? "Unknown Client";
                var creatorName = creator != null ? $"{creator.FirstName} {creator.LastName}".Trim() : null;

                // Generate description based on status
                string description = GenerateActivityDescription(currentStatus, offer.Id, clientName, salesmanName, creatorName, oldStatus);

                var activity = new RecentOfferActivity
                {
                    OfferId = offer.Id,
                    Type = currentStatus,
                    Description = description,
                    ClientName = clientName,
                    SalesmanName = salesmanName,
                    TotalAmount = offer.TotalAmount,
                    ActivityTimestamp = DateTime.UtcNow
                };

                await _unitOfWork.RecentOfferActivities.CreateAsync(activity, cancellationToken);
                
                // Maintain only the last 20 activities
                await _unitOfWork.RecentOfferActivities.MaintainMaxActivitiesAsync(20, cancellationToken);
                
                _logger.LogInformation("Recent activity saved. OfferId: {OfferId}, Status: {Status}", offer.Id, currentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving recent activity for OfferId: {OfferId}", offer.Id);
                // Don't throw - activity saving failure shouldn't break the main operation
            }
        }

        /// <summary>
        /// Generates activity description based on offer status
        /// </summary>
        private string GenerateActivityDescription(string status, long offerId, string clientName, string? salesmanName, string? creatorName, string? oldStatus)
        {
            return status switch
            {
                "Draft" => creatorName != null
                    ? $"{creatorName} created Offer #{offerId} for client {clientName}"
                    : $"Offer #{offerId} was created for client {clientName}",
                
                "Sent" => creatorName != null
                    ? $"{creatorName} sent Offer #{offerId} to client {clientName}"
                    : $"Offer #{offerId} was sent to client {clientName}",
                
                "Accepted" => salesmanName != null
                    ? $"{salesmanName} reported that client {clientName} accepted Offer #{offerId}"
                    : $"Client {clientName} accepted Offer #{offerId}",
                
                "Rejected" => salesmanName != null
                    ? $"{salesmanName} reported that client {clientName} rejected Offer #{offerId}"
                    : $"Client {clientName} rejected Offer #{offerId}",
                
                "Completed" => salesmanName != null
                    ? $"{salesmanName} completed Offer #{offerId} for client {clientName}"
                    : $"Offer #{offerId} for client {clientName} was completed",
                
                "UnderReview" => $"Offer #{offerId} for client {clientName} is under review",
                
                "NeedsModification" => $"Offer #{offerId} for client {clientName} needs modification",
                
                "Expired" => $"Offer #{offerId} for client {clientName} has expired",
                
                "Cancelled" => creatorName != null
                    ? $"{creatorName} cancelled Offer #{offerId} for client {clientName}"
                    : $"Offer #{offerId} for client {clientName} was cancelled",
                
                "Ready" => creatorName != null
                    ? $"{creatorName} marked Offer #{offerId} as ready for client {clientName}"
                    : $"Offer #{offerId} for client {clientName} is ready",
                
                _ => oldStatus != null
                    ? $"Offer #{offerId} for client {clientName} status changed from {oldStatus} to {status}"
                    : $"Offer #{offerId} for client {clientName} status changed to {status}"
            };
        }

        public async Task<List<OfferActivityDTO>> GetRecentActivityAsync(int limit = 20)
        {
            try
            {
                // Get activities from database
                var activities = await _unitOfWork.RecentOfferActivities.GetRecentActivitiesAsync(limit);
                
                return activities.Select(a => new OfferActivityDTO
                {
                    OfferId = a.OfferId,
                    Type = a.Type,
                    Description = a.Description,
                    ClientName = a.ClientName,
                    SalesmanName = a.SalesmanName,
                    TotalAmount = a.TotalAmount,
                    Timestamp = a.ActivityTimestamp
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity");
                throw;
            }
        }

        /// <summary>
        /// Get full history of all actions performed on a specific offer
        /// </summary>
        public async Task<List<OfferActivityDTO>> GetOfferHistoryAsync(long offerId)
        {
            try
            {
                // Get all activities for this specific offer
                var activities = await _unitOfWork.RecentOfferActivities.GetActivitiesByOfferIdAsync(offerId);
                
                return activities.Select(a => new OfferActivityDTO
                {
                    OfferId = a.OfferId,
                    Type = a.Type,
                    Description = a.Description,
                    ClientName = a.ClientName,
                    SalesmanName = a.SalesmanName,
                    TotalAmount = a.TotalAmount,
                    Timestamp = a.ActivityTimestamp
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offer history. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        public async Task<OfferResponseDTO> AssignOfferToSalesmanAsync(long offerId, string salesmanId, string userId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                // Validate salesman user exists and has Salesman role
                var salesman = await _unitOfWork.Users.GetByIdAsync(salesmanId);
                if (salesman == null)
                    throw new ArgumentException("Salesman user not found", nameof(salesmanId));

                // Verify user has Salesman role
                var userRoles = await _userManager.GetRolesAsync(salesman);
                if (!userRoles.Contains("Salesman"))
                    throw new ArgumentException("User must have Salesman role", nameof(salesmanId));

                // Update assignment
                offer.AssignedTo = salesmanId;
                await _unitOfWork.SalesOffers.UpdateAsync(offer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer assigned to salesman successfully. OfferId: {OfferId}, SalesmanId: {SalesmanId}", offerId, salesmanId);

                return await MapToOfferResponseDTO(offer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning offer to salesman. OfferId: {OfferId}", offerId);
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

            // Validate ValidUntil dates (check if all dates are in the past)
            if (offerDto.ValidUntil != null && offerDto.ValidUntil.Count > 0)
            {
                var now = DateTime.UtcNow;
                var allExpired = offerDto.ValidUntil.All(dateStr => 
                    DateTime.TryParse(dateStr, out var date) && now > date);
                if (allExpired)
                    return false;
            }

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
                // OPTIMIZED: Single method call loads offers + related data in O(1) queries (3 queries total)
                var (offers, clientsDict, usersDict) = await _unitOfWork.SalesOffers
                    .GetExpiredOffersWithRelatedDataAsync();

                // Map synchronously using pre-loaded data - O(n) in-memory operation
                return offers.Select(o => MapToOfferResponseDTO(o, clientsDict, usersDict)).ToList();
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
                    ProviderImagePath = dto.ProviderImagePath,
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
                    ProviderImagePath = equipment.ProviderImagePath,
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
                
                // Only fetch products if equipment needs enrichment (has missing data)
                var needsEnrichment = equipment.Any(e => 
                    string.IsNullOrWhiteSpace(e.Model) || 
                    string.IsNullOrWhiteSpace(e.Provider) || 
                    string.IsNullOrWhiteSpace(e.ImagePath) ||
                    e.Price <= 0);
                
                Dictionary<string, Product>? productLookup = null;
                if (needsEnrichment)
                {
                    // OPTIMIZATION: Only query products that match equipment names instead of loading all products
                    // This is much faster when there are many products in the database
                    var equipmentNames = equipment
                        .Where(e => !string.IsNullOrWhiteSpace(e.Name))
                        .Select(e => e.Name.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (equipmentNames.Any())
                    {
                        // Get only products that might match equipment names (optimized query)
                        var matchingProducts = (await _unitOfWork.Products.GetByNamesAsync(equipmentNames)).ToList();
                        
                        // Create lookup dictionary for faster matching (O(1) instead of O(n))
                        productLookup = matchingProducts
                            .GroupBy(p => p.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
                        
                        _logger.LogInformation(
                            "Optimized product query: Found {ProductCount} matching products for {EquipmentCount} equipment items. OfferId: {OfferId}",
                            matchingProducts.Count, equipment.Count(), offerId);
                    }
                }
                
                return equipment.Select(e =>
                {
                    Product? matchedProduct = null;
                    
                    // Only try to match if we have products and equipment needs enrichment
                    if (productLookup != null)
                    {
                        var normalizedEquipmentName = e.Name.Trim();
                        
                        // Try exact match first (O(1) lookup)
                        if (productLookup.TryGetValue(normalizedEquipmentName, out var exactMatch))
                        {
                            matchedProduct = exactMatch;
                        }
                        else
                        {
                            // Fallback to partial matching only if equipment needs data
                            if (string.IsNullOrWhiteSpace(e.Model) || 
                                string.IsNullOrWhiteSpace(e.Provider) || 
                                string.IsNullOrWhiteSpace(e.ImagePath) ||
                                e.Price <= 0)
                            {
                                matchedProduct = productLookup.Values.FirstOrDefault(p =>
                                    normalizedEquipmentName.Contains(p.Name, StringComparison.OrdinalIgnoreCase) ||
                                    p.Name.Contains(normalizedEquipmentName, StringComparison.OrdinalIgnoreCase))
                                    ?? productLookup.Values.FirstOrDefault(p =>
                                        // Try to match by extracting key words (first 3 words)
                                        normalizedEquipmentName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3 &&
                                        p.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3 &&
                                        normalizedEquipmentName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                            .Take(3)
                                            .Any(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)));
                            }
                        }
                    }

                    return new OfferEquipmentDTO
                    {
                        Id = e.Id,
                        OfferId = e.OfferId,
                        Name = e.Name,
                        // Use equipment data if available, otherwise fall back to matched product data
                        Model = e.Model ?? matchedProduct?.Model,
                        Provider = e.Provider ?? matchedProduct?.Provider,
                        Country = e.Country ?? matchedProduct?.Country,
                        Year = e.Year ?? matchedProduct?.Year,
                        ImagePath = e.ImagePath ?? matchedProduct?.ImagePath,
                        ProviderImagePath = e.ProviderImagePath ?? matchedProduct?.ProviderImagePath,
                        // Use equipment price if set (> 0), otherwise use product base price
                        Price = e.Price > 0 ? e.Price : (matchedProduct?.BasePrice ?? 0),
                        Description = !string.IsNullOrWhiteSpace(e.Description) && !e.Description.StartsWith("Equipment item:") 
                            ? e.Description 
                            : (matchedProduct?.Description ?? e.Description),
                        InStock = e.InStock
                    };
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

        public async Task<OfferEquipmentDTO> UpdateEquipmentAsync(long offerId, long equipmentId, UpdateOfferEquipmentDTO dto)
        {
            try
            {
                var equipment = await _unitOfWork.OfferEquipment.GetByIdAsync(equipmentId);
                if (equipment == null || equipment.OfferId != offerId)
                    throw new ArgumentException("Equipment not found or doesn't belong to this offer");

                // Update all fields - these changes are saved to OfferEquipment, NOT Product table
                equipment.Name = dto.Name;
                equipment.Model = dto.Model;
                equipment.Provider = dto.Provider;
                if (!string.IsNullOrWhiteSpace(dto.ProviderImagePath))
                    equipment.ProviderImagePath = dto.ProviderImagePath;
                equipment.Country = dto.Country;
                equipment.Year = dto.Year;
                equipment.Price = dto.Price;
                equipment.Description = dto.Description;
                equipment.InStock = dto.InStock;
                
                // Update image path if provided
                if (!string.IsNullOrWhiteSpace(dto.ImagePath))
                {
                    equipment.ImagePath = dto.ImagePath;
                }

                await _unitOfWork.OfferEquipment.UpdateAsync(equipment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Equipment updated. OfferId: {OfferId}, EquipmentId: {EquipmentId}. Changes saved to OfferEquipment only, not Product table.", offerId, equipmentId);

                return new OfferEquipmentDTO
                {
                    Id = equipment.Id,
                    OfferId = equipment.OfferId,
                    Name = equipment.Name,
                    Model = equipment.Model,
                    Provider = equipment.Provider,
                    Country = equipment.Country,
                    Year = equipment.Year,
                    ImagePath = equipment.ImagePath,
                    ProviderImagePath = equipment.ProviderImagePath,
                    Price = equipment.Price,
                    Description = equipment.Description,
                    InStock = equipment.InStock
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                throw;
            }
        }

        public async Task<OfferEquipmentDTO> UpdateEquipmentImagePathAsync(long offerId, long equipmentId, string imagePath)
        {
            try
            {
                var equipment = await _unitOfWork.OfferEquipment.GetByIdAsync(equipmentId);
                if (equipment == null || equipment.OfferId != offerId)
                    throw new ArgumentException("Equipment not found or doesn't belong to this offer");

                equipment.ImagePath = imagePath;
                await _unitOfWork.OfferEquipment.UpdateAsync(equipment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Equipment image path updated. OfferId: {OfferId}, EquipmentId: {EquipmentId}, ImagePath: {ImagePath}", offerId, equipmentId, imagePath);

                return new OfferEquipmentDTO
                {
                    Id = equipment.Id,
                    OfferId = equipment.OfferId,
                    Name = equipment.Name,
                    Model = equipment.Model,
                    Provider = equipment.Provider,
                    Country = equipment.Country,
                    ImagePath = equipment.ImagePath,
                    ProviderImagePath = equipment.ProviderImagePath,
                    Price = equipment.Price,
                    Description = equipment.Description,
                    InStock = equipment.InStock
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment image path. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                throw;
            }
        }

        public async Task<string?> GetEquipmentImagePathAsync(long offerId, long equipmentId)
        {
            try
            {
                var equipment = await _unitOfWork.OfferEquipment.GetByIdAsync(equipmentId);
                if (equipment == null || equipment.OfferId != offerId)
                {
                    _logger.LogWarning("Equipment not found. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                    return null;
                }

                // If equipment has image path, return it immediately (fast path)
                if (!string.IsNullOrWhiteSpace(equipment.ImagePath) && !equipment.ImagePath.Contains("equipment-placeholder.png"))
                {
                    _logger.LogInformation("Equipment has valid image path. OfferId: {OfferId}, EquipmentId: {EquipmentId}, ImagePath: {ImagePath}", offerId, equipmentId, equipment.ImagePath);
                    return equipment.ImagePath;
                }

                // If no image path or placeholder, try to find matching product
                var normalizedEquipmentName = equipment.Name.Trim();
                _logger.LogInformation("Searching for product match. EquipmentName: {EquipmentName}, EquipmentId: {EquipmentId}, Model: {Model}", 
                    normalizedEquipmentName, equipmentId, equipment.Model);

                Product? matchedProduct = null;

                // Strategy 1: Try exact name match first (fast)
                // Limit search to first 20 results for performance
                var exactMatchProducts = (await _unitOfWork.Products.SearchAsync(normalizedEquipmentName)).Take(20).ToList();
                matchedProduct = exactMatchProducts.FirstOrDefault(p => 
                    p.Name.Equals(normalizedEquipmentName, StringComparison.OrdinalIgnoreCase));
                
                if (matchedProduct != null)
                {
                    _logger.LogInformation("Found exact name match. ProductId: {ProductId}, ProductName: {ProductName}", 
                        matchedProduct.Id, matchedProduct.Name);
                }

                // Strategy 2: If no exact match, try partial matching with first few words
                if (matchedProduct == null)
                {
                    // Extract first 3-4 meaningful words from equipment name for better matching
                    string[] words = normalizedEquipmentName
                        .Split(new[] { ' ', '(', '-', ')', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w.Length > 2 && !w.All(char.IsDigit)) // Filter out very short words and pure numbers
                        .Take(4)
                        .ToArray();
                    
                    if (words.Length > 0)
                    {
                        var searchTerm = string.Join(" ", words);
                        _logger.LogInformation("Trying partial match with search term: {SearchTerm}, Words: [{Words}]", 
                            searchTerm, string.Join(", ", words));
                        
                        // Try searching with the full search term first (limit to 20 results for performance)
                        var partialMatchProducts = (await _unitOfWork.Products.SearchAsync(searchTerm)).Take(20).ToList();
                        
                        // Try to find best match by comparing name similarity
                        matchedProduct = partialMatchProducts
                            .OrderByDescending(p => {
                                var productNameLower = p.Name.ToLower();
                                // Calculate similarity: check if product name contains key words from equipment name
                                int matchCount = words.Count(w => productNameLower.Contains(w.ToLower()));
                                // Bonus if product name starts with the first word
                                if (words.Length > 0 && productNameLower.StartsWith(words[0].ToLower()))
                                    matchCount += 2;
                                return matchCount;
                            })
                            .FirstOrDefault();
                        
                        if (matchedProduct != null)
                        {
                            _logger.LogInformation("Found partial match. ProductId: {ProductId}, ProductName: {ProductName}, SearchTerm: {SearchTerm}", 
                                matchedProduct.Id, matchedProduct.Name, searchTerm);
                        }
                        else
                        {
                            // If still no match, try searching with just the first 2-3 words (limit to 20 results)
                            if (words.Length >= 2)
                            {
                                var shorterSearchTerm = string.Join(" ", words.Take(2));
                                _logger.LogInformation("Trying shorter search term: {SearchTerm}", shorterSearchTerm);
                                var shorterMatchProducts = (await _unitOfWork.Products.SearchAsync(shorterSearchTerm)).Take(20).ToList();
                                string[] wordsSubset = words.Take(2).ToArray();
                                matchedProduct = shorterMatchProducts
                                    .OrderByDescending(p => {
                                        var productNameLower = p.Name.ToLower();
                                        int matchCount = wordsSubset.Count(w => productNameLower.Contains(w.ToLower()));
                                        if (wordsSubset.Length > 0 && productNameLower.StartsWith(wordsSubset[0].ToLower()))
                                            matchCount += 2;
                                        return matchCount;
                                    })
                                    .FirstOrDefault();
                                
                                if (matchedProduct != null)
                                {
                                    _logger.LogInformation("Found match with shorter term. ProductId: {ProductId}, ProductName: {ProductName}", 
                                        matchedProduct.Id, matchedProduct.Name);
                                }
                            }
                        }
                    }
                }

                // Strategy 3: If still no match, try matching by model if available (limit to 10 results)
                if (matchedProduct == null && !string.IsNullOrWhiteSpace(equipment.Model))
                {
                    _logger.LogInformation("Trying model match. Model: {Model}", equipment.Model);
                    var modelMatchProducts = (await _unitOfWork.Products.SearchAsync(equipment.Model)).Take(10).ToList();
                    matchedProduct = modelMatchProducts.FirstOrDefault();
                    
                    if (matchedProduct != null)
                    {
                        _logger.LogInformation("Found model match. ProductId: {ProductId}, ProductName: {ProductName}, Model: {Model}", 
                            matchedProduct.Id, matchedProduct.Name, equipment.Model);
                    }
                }

                // Strategy 4: Last resort - try searching with individual key words (optimized - no full product load)
                if (matchedProduct == null)
                {
                    // Extract key words from equipment name
                    var keyWords = normalizedEquipmentName
                        .Split(new[] { ' ', '(', '-', ')', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w.Length > 3 && !w.All(char.IsDigit)) // Only words longer than 3 chars
                        .Take(3)
                        .Select(w => w.ToLower())
                        .ToList();
                    
                    if (keyWords.Any())
                    {
                        _logger.LogInformation("Trying last resort: searching with individual key words: {KeyWords}", string.Join(", ", keyWords));
                        
                        // Try searching with the longest key word first (most specific)
                        var longestWord = keyWords.OrderByDescending(w => w.Length).First();
                        var lastResortProducts = await _unitOfWork.Products.SearchAsync(longestWord);
                        
                        if (lastResortProducts.Any())
                        {
                            matchedProduct = lastResortProducts
                                .OrderByDescending(p => {
                                    var productNameLower = p.Name.ToLower();
                                    int matchCount = keyWords.Count(w => productNameLower.Contains(w));
                                    // Bonus if product name starts with first key word
                                    if (keyWords.Any() && productNameLower.StartsWith(keyWords[0]))
                                        matchCount += 3;
                                    // Bonus if product name contains all key words
                                    if (keyWords.All(w => productNameLower.Contains(w)))
                                        matchCount += 5;
                                    return matchCount;
                                })
                                .FirstOrDefault(p => {
                                    var productNameLower = p.Name.ToLower();
                                    return keyWords.Any(w => productNameLower.Contains(w));
                                });
                            
                            if (matchedProduct != null)
                            {
                                _logger.LogInformation("Found match in last resort search. ProductId: {ProductId}, ProductName: {ProductName}", 
                                    matchedProduct.Id, matchedProduct.Name);
                            }
                        }
                    }
                }

                if (matchedProduct != null && !string.IsNullOrWhiteSpace(matchedProduct.ImagePath))
                {
                    _logger.LogInformation("Found matching product. ProductId: {ProductId}, ProductName: {ProductName}, ImagePath: {ImagePath}", 
                        matchedProduct.Id, matchedProduct.Name, matchedProduct.ImagePath);
                    return matchedProduct.ImagePath;
                }

                _logger.LogWarning("No matching product found for equipment. EquipmentName: {EquipmentName}, EquipmentId: {EquipmentId}", normalizedEquipmentName, equipmentId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment image path. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
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

                // OPTIMIZATION: Use bulk delete instead of sequential deletes
                var existingInstallments = await _unitOfWork.InstallmentPlans.GetByOfferIdAsync(offerId);
                var existingList = existingInstallments.ToList();
                if (existingList.Any())
                {
                    await _unitOfWork.InstallmentPlans.DeleteRangeAsync(existingList);
                }

                // OPTIMIZATION: Build all installments first, then bulk create
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
                }

                // OPTIMIZATION: Bulk create all installments at once
                await _unitOfWork.InstallmentPlans.CreateRangeAsync(installments);
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
                    PaymentTerms = DeserializeStringList(offer.PaymentTerms),
                    DeliveryTerms = DeserializeStringList(offer.DeliveryTerms),
                    WarrantyTerms = DeserializeStringList(offer.WarrantyTerms),
                    ValidUntil = DeserializeStringList(offer.ValidUntil),
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

        // Helper method to pre-load related data and map offers synchronously
        private async Task<List<OfferResponseDTO>> MapOffersToDTOsAsync(IEnumerable<SalesOffer> offers)
        {
            var offersList = offers.ToList();
            if (!offersList.Any())
                return new List<OfferResponseDTO>();

            // Safety limit to prevent memory issues and connection pool exhaustion
            const int maxOffersToProcess = 1000;
            if (offersList.Count > maxOffersToProcess)
            {
                _logger.LogWarning("Attempting to map {Count} offers, limiting to {Max} to prevent memory issues", 
                    offersList.Count, maxOffersToProcess);
                offersList = offersList.Take(maxOffersToProcess).ToList();
            }

            // Pre-load all related data in batches to avoid DbContext concurrency issues
            var clientIds = offersList.Where(o => o.ClientId > 0).Select(o => o.ClientId).Distinct().ToList();
            var userIds = offersList
                .SelectMany(o => new[] { o.CreatedBy, o.AssignedTo })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            // Load sequentially to avoid DbContext concurrency issues (same DbContext instance)
            var clientsList = clientIds.Any() ? await _unitOfWork.Clients.GetByIdsAsync(clientIds) : new List<Client>();
            var usersList = userIds.Any() ? await _unitOfWork.Users.GetByIdsAsync(userIds) : new List<ApplicationUser>();

            var clientsDict = clientsList.ToDictionary(c => c.Id);
            var usersDict = usersList.ToDictionary(u => u.Id);

            // Map synchronously using pre-loaded data
            return offersList.Select(o => MapToOfferResponseDTO(o, clientsDict, usersDict)).ToList();
        }

        private async Task<OfferResponseDTO> MapToOfferResponseDTO(SalesOffer offer)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
            var creator = await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy);
            var salesman = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);

            // Get OfferRequest information if linked
            string? requesterId = null;
            string? requesterName = null;
            if (offer.OfferRequestId.HasValue)
            {
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(offer.OfferRequestId.Value);
                if (offerRequest != null)
                {
                    requesterId = offerRequest.RequestedBy;
                    var requester = await _unitOfWork.Users.GetByIdAsync(offerRequest.RequestedBy);
                    if (requester != null)
                    {
                        requesterName = $"{requester.FirstName} {requester.LastName}".Trim();
                    }
                }
            }

            return new OfferResponseDTO
            {
                Id = offer.Id,
                OfferRequestId = offer.OfferRequestId,
                OfferRequestRequesterId = requesterId,
                OfferRequestRequesterName = requesterName,
                ClientId = offer.ClientId,
                ClientName = client?.Name ?? "Unknown Client",
                CreatedBy = offer.CreatedBy,
                CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown Creator",
                AssignedTo = offer.AssignedTo,
                AssignedToName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Unknown Salesman",
                Products = offer.Products,
                TotalAmount = offer.TotalAmount,
                PaymentTerms = DeserializeStringList(offer.PaymentTerms),
                DeliveryTerms = DeserializeStringList(offer.DeliveryTerms),
                WarrantyTerms = DeserializeStringList(offer.WarrantyTerms),
                ValidUntil = DeserializeStringList(offer.ValidUntil),
                Status = offer.Status,
                SentToClientAt = offer.SentToClientAt,
                ClientResponse = offer.ClientResponse,
                SalesManagerApprovedBy = offer.SalesManagerApprovedBy,
                SalesManagerApprovedAt = offer.SalesManagerApprovedAt,
                SalesManagerComments = offer.SalesManagerComments,
                SalesManagerRejectionReason = offer.SalesManagerRejectionReason,
                CreatedAt = offer.CreatedAt,
                IsSalesManagerApproved = offer.Status == OfferStatus.Sent && !string.IsNullOrEmpty(offer.SalesManagerApprovedBy),
                CanSendToSalesman = offer.Status == OfferStatus.Sent
            };
        }

        // Synchronous overload that uses pre-loaded data to avoid DbContext concurrency issues
        private OfferResponseDTO MapToOfferResponseDTO(SalesOffer offer, Dictionary<long, Client> clientsDict, Dictionary<string, ApplicationUser> usersDict)
        {
            clientsDict.TryGetValue(offer.ClientId, out var client);
            usersDict.TryGetValue(offer.CreatedBy ?? string.Empty, out var creator);
            usersDict.TryGetValue(offer.AssignedTo ?? string.Empty, out var salesman);

            // Note: OfferRequest information should be loaded separately if needed in batch operations
            // For now, we'll set it to null in batch operations to avoid N+1 queries
            // Individual offer retrieval will use the async method that loads OfferRequest

            return new OfferResponseDTO
            {
                Id = offer.Id,
                OfferRequestId = offer.OfferRequestId,
                OfferRequestRequesterId = null, // Will be populated in individual retrieval
                OfferRequestRequesterName = null, // Will be populated in individual retrieval
                ClientId = offer.ClientId,
                ClientName = client?.Name ?? "Unknown Client",
                CreatedBy = offer.CreatedBy,
                CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown Creator",
                AssignedTo = offer.AssignedTo,
                AssignedToName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Unknown Salesman",
                Products = offer.Products,
                TotalAmount = offer.TotalAmount,
                PaymentTerms = DeserializeStringList(offer.PaymentTerms),
                DeliveryTerms = DeserializeStringList(offer.DeliveryTerms),
                WarrantyTerms = DeserializeStringList(offer.WarrantyTerms),
                ValidUntil = DeserializeStringList(offer.ValidUntil),
                Status = offer.Status,
                SentToClientAt = offer.SentToClientAt,
                ClientResponse = offer.ClientResponse,
                SalesManagerApprovedBy = offer.SalesManagerApprovedBy,
                SalesManagerApprovedAt = offer.SalesManagerApprovedAt,
                SalesManagerComments = offer.SalesManagerComments,
                SalesManagerRejectionReason = offer.SalesManagerRejectionReason,
                CreatedAt = offer.CreatedAt,
                IsSalesManagerApproved = offer.Status == OfferStatus.Sent && !string.IsNullOrEmpty(offer.SalesManagerApprovedBy),
                CanSendToSalesman = offer.Status == OfferStatus.Sent
            };
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Automatically adds equipment items to offer based on products description
        /// Creates equipment with default image paths
        /// </summary>
        private async Task AutoAddEquipmentFromProductsAsync(long offerId, string productsDescription)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productsDescription))
                    return;

                // Parse products - try to split by comma or newline
                var productNames = productsDescription
                    .Split(new[] { ',', '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();

                if (!productNames.Any())
                {
                    // If no products parsed, create one default equipment item
                    productNames = new List<string> { "Equipment" };
                }

                // Get all active products to match against
                var allProducts = (await _unitOfWork.Products.GetAllActiveAsync()).ToList();

                foreach (var productName in productNames)
                {
                    // Try to find matching product in Products table
                    // Match by name (case-insensitive, partial match)
                    // First try exact match, then try if product name contains the search term or vice versa
                    var normalizedProductName = productName.Trim();
                    var matchedProduct = allProducts.FirstOrDefault(p =>
                        p.Name.Equals(normalizedProductName, StringComparison.OrdinalIgnoreCase)) 
                        ?? allProducts.FirstOrDefault(p =>
                            normalizedProductName.Contains(p.Name, StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains(normalizedProductName, StringComparison.OrdinalIgnoreCase))
                        ?? allProducts.FirstOrDefault(p =>
                            // Try to match by extracting key words (first 3-5 words)
                            normalizedProductName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3 &&
                            p.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3 &&
                            normalizedProductName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Take(3)
                                .Any(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)));

                    var equipment = new OfferEquipment
                    {
                        OfferId = offerId,
                        Name = productName.Length > 200 ? productName.Substring(0, 200) : productName,
                        // Populate from Products table if match found
                        Model = matchedProduct?.Model,
                        Provider = matchedProduct?.Provider,
                        Country = matchedProduct?.Country,
                        Year = matchedProduct?.Year,
                        Price = matchedProduct?.BasePrice ?? 0,
                        Description = matchedProduct?.Description ?? $"Equipment item: {productName}",
                        InStock = matchedProduct?.InStock ?? true,
                        ImagePath = matchedProduct?.ImagePath ?? $"offers/{offerId}/equipment-placeholder.png"
                    };

                    await _unitOfWork.OfferEquipment.CreateAsync(equipment);
                    
                    if (matchedProduct != null)
                    {
                        _logger.LogInformation("Matched product '{ProductName}' with catalog product ID {ProductId} for offer {OfferId}", 
                            productName, matchedProduct.Id, offerId);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Auto-added {Count} equipment items to offer {OfferId}", productNames.Count, offerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-adding equipment to offer {OfferId}", offerId);
                throw;
            }
        }

        /// <summary>
        /// Sends notification to all SalesManager users when an offer needs approval
        /// </summary>
        private async Task SendSalesManagerApprovalNotificationAsync(SalesOffer offer)
        {
            try
            {
                // Get client info for notification
                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                // Get creator info
                var creator = await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy);
                var creatorName = creator != null ? $"{creator.FirstName} {creator.LastName}".Trim() : "SalesSupport";

                // Get all SalesManagers
                var salesManagers = await _userManager.GetUsersInRoleAsync("SalesManager");
                var activeSalesManagers = salesManagers.Where(sm => sm.IsActive).ToList();

                if (!activeSalesManagers.Any())
                {
                    _logger.LogWarning("No active SalesManagers found to notify for Offer: {OfferId}", offer.Id);
                    return;
                }

                var notificationTitle = "Offer Pending Approval";
                var notificationMessage = $"New offer #{offer.Id} created by {creatorName} for {clientName} (Amount: {offer.TotalAmount:C}) requires your approval before sending to salesman.";

                var metadata = new Dictionary<string, object>
                {
                    ["offerId"] = offer.Id,
                    ["clientName"] = clientName,
                    ["creatorName"] = creatorName,
                    ["totalAmount"] = offer.TotalAmount,
                    ["status"] = offer.Status
                };

                // Send notification to all SalesManagers
                _logger.LogInformation("📢 Sending notifications to {Count} active SalesManagers for Offer: {OfferId}", 
                    activeSalesManagers.Count, offer.Id);

                // Also send a broadcast to the role group as backup
                try
                {
                    await _notificationService.SendNotificationToRoleGroupAsync(
                        "SalesManager",
                        notificationTitle,
                        notificationMessage,
                        metadata,
                        CancellationToken.None
                    );
                    _logger.LogInformation("  Broadcast notification sent to Role_SalesManager group for Offer: {OfferId}", offer.Id);
                }
                catch (Exception broadcastEx)
                {
                    _logger.LogWarning(broadcastEx, "⚠️ Failed to send broadcast to role group (continuing with individual notifications)");
                }

                foreach (var salesManager in activeSalesManagers)
                {
                    try
                    {
                        _logger.LogInformation("📤 Creating notification for SalesManager: {SalesManagerId} ({SalesManagerName}) for Offer: {OfferId}", 
                            salesManager.Id, salesManager.UserName, offer.Id);

                        var notification = await _notificationService.CreateNotificationAsync(
                            salesManager.Id,
                            notificationTitle,
                            notificationMessage,
                            "Offer",
                            "High",
                            null,
                            null,
                            true, // Mobile push notification
                            metadata,
                            CancellationToken.None
                        );

                        _logger.LogInformation("  Notification created successfully for SalesManager: {SalesManagerId}, NotificationId: {NotificationId}", 
                            salesManager.Id, notification?.Id);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogWarning(notifEx, "⚠️ Failed to create notification for SalesManager: {SalesManagerId} for Offer: {OfferId}", 
                            salesManager.Id, offer.Id);
                    }
                }

                _logger.LogInformation("  Completed sending SalesManager approval notifications for Offer: {OfferId}", offer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending SalesManager approval notifications for Offer: {OfferId}", offer.Id);
                throw;
            }
        }

        /// <summary>
        /// Helper method to deserialize JSON array or return single value as array (backward compatibility)
        /// </summary>
        private List<string>? DeserializeStringList(string? jsonOrString)
        {
            if (string.IsNullOrWhiteSpace(jsonOrString))
                return null;

            // Try to deserialize as JSON array first
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(jsonOrString);
                if (list != null && list.Count > 0)
                    return list;
            }
            catch
            {
                // Not JSON, try as single string (backward compatibility)
            }

            // Fallback: treat as single string value (backward compatibility)
            return new List<string> { jsonOrString };
        }

        /// <summary>
        /// Helper method to parse DeliveryTerms JSON array and extract delivery date from descriptive text
        /// DeliveryTerms contains descriptive text like "the equipment will be delivered within a week"
        /// This method tries to extract actual dates or calculate dates from relative terms
        /// </summary>
        private DateTime? ParseDeliveryTermsToDate(string? deliveryTerms)
        {
            if (string.IsNullOrWhiteSpace(deliveryTerms))
                return null;

            try
            {
                // Parse the JSON array of delivery term strings
                var terms = DeserializeStringList(deliveryTerms);
                if (terms == null || terms.Count == 0)
                    return null;

                // Look through all delivery terms for dates
                foreach (var term in terms)
                {
                    if (string.IsNullOrWhiteSpace(term))
                        continue;

                    // Try to find explicit dates in the text (e.g., "delivered on 10/11/2020" or "by December 15, 2025")
                    var dateMatch = Regex.Match(
                        term,
                        @"(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})|(\d{1,2}\s+(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{2,4})",
                        RegexOptions.IgnoreCase
                    );

                    if (dateMatch.Success && DateTime.TryParse(dateMatch.Value, out var explicitDate))
                    {
                        return explicitDate;
                    }

                    // Try to parse relative terms like "within a week", "in 2 weeks", "within 30 days"
                    var relativeMatch = Regex.Match(
                        term,
                        @"(within|in)\s+(\d+)\s+(day|days|week|weeks|month|months)",
                        RegexOptions.IgnoreCase
                    );

                    if (relativeMatch.Success)
                    {
                        var number = int.Parse(relativeMatch.Groups[2].Value);
                        var unit = relativeMatch.Groups[3].Value.ToLower();
                        
                        var deliveryDate = DateTime.UtcNow;
                        if (unit.StartsWith("day"))
                            deliveryDate = deliveryDate.AddDays(number);
                        else if (unit.StartsWith("week"))
                            deliveryDate = deliveryDate.AddDays(number * 7);
                        else if (unit.StartsWith("month"))
                            deliveryDate = deliveryDate.AddMonths(number);
                        
                        return deliveryDate;
                    }

                    // Try parsing the whole term as a date (in case it's just a date string)
                    if (DateTime.TryParse(term, out var directDate))
                    {
                        return directDate;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse DeliveryTerms to date: {DeliveryTerms}", deliveryTerms);
            }

            // If no date can be extracted from delivery terms, return null
            // The deal can still be created without an expected delivery date
            return null;
        }

        #endregion
    }
}
