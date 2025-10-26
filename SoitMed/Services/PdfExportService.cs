using iTextSharp.text;
using iTextSharp.text.pdf;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public interface IPdfExportService
    {
        Task<byte[]> GenerateOfferPdfAsync(long offerId, CancellationToken cancellationToken = default);
        byte[] GenerateLetterheadBackground();
    }

    public class PdfExportService : IPdfExportService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PdfExportService> _logger;
        private const float LetterheadOpacity = 0.2f; // 20% opacity

        public PdfExportService(
            IWebHostEnvironment environment,
            IUnitOfWork unitOfWork,
            ILogger<PdfExportService> logger)
        {
            _environment = environment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<byte[]> GenerateOfferPdfAsync(long offerId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get offer with all related data
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                {
                    throw new ArgumentException($"Offer with ID {offerId} not found");
                }

                // Get related data
                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var creator = await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy);
                var salesman = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);
                
                // Load equipment
                var equipment = await _unitOfWork.OfferEquipment.GetByOfferIdAsync(offerId, cancellationToken);
                
                // Load terms
                var terms = await _unitOfWork.OfferTerms.GetByOfferIdAsync(offerId, cancellationToken);
                
                // Load installments
                var installments = await _unitOfWork.InstallmentPlans.GetByOfferIdAsync(offerId, cancellationToken);

                // Create PDF
                using var memoryStream = new MemoryStream();
                using var document = new Document(PageSize.A4, 50, 50, 50, 50);
                using var writer = PdfWriter.GetInstance(document, memoryStream);
                
                document.Open();

                // Add letterhead background
                AddLetterheadBackground(document, writer);

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.BLACK);
                var titleParagraph = new Paragraph("OFFER", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(titleParagraph);

                // Add offer details
                AddOfferDetails(document, offer, client, creator, salesman);

                // Add equipment table
                AddEquipmentTable(document, equipment, offer);

                // Add terms section
                if (terms != null)
                {
                    AddTermsSection(document, terms);
                }

                // Add payment section
                AddPaymentSection(document, offer, installments);

                // Add footer
                AddFooter(document, offer);

                document.Close();

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for offer {OfferId}", offerId);
                throw;
            }
        }

        private void AddLetterheadBackground(Document document, PdfWriter writer)
        {
            try
            {
                var letterheadPath = Path.Combine(_environment.WebRootPath, "templates", "letterhead.png");
                
                if (File.Exists(letterheadPath))
                {
                    var letterheadImg = iTextSharp.text.Image.GetInstance(letterheadPath);
                    letterheadImg.Alignment = Element.ALIGN_UNDEFINED;
                    letterheadImg.ScaleAbsolute(document.PageSize.Width, document.PageSize.Height);
                    letterheadImg.SetAbsolutePosition(0, 0);
                    
                    // Apply opacity
                    var gState = new PdfGState();
                    gState.FillOpacity = LetterheadOpacity;
                    gState.StrokeOpacity = LetterheadOpacity;
                    
                    var cb = writer.DirectContent;
                    cb.SetGState(gState);
                    cb.AddImage(letterheadImg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not add letterhead background, continuing without it");
            }
        }

        private void AddOfferDetails(Document document, SalesOffer offer, Client? client, ApplicationUser? creator, ApplicationUser? salesman)
        {
            var detailsFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            var details = new PdfPTable(2) { WidthPercentage = 100 };
            details.SetWidths(new float[] { 1f, 2f });
            details.SpacingAfter = 15;

            // Client
            details.AddCell(new PdfPCell(new Phrase("Client:", boldFont)) { Border = 0 });
            details.AddCell(new PdfPCell(new Phrase(client?.Name ?? "N/A", detailsFont)) { Border = 0 });

            // Date
            details.AddCell(new PdfPCell(new Phrase("Date:", boldFont)) { Border = 0 });
            details.AddCell(new PdfPCell(new Phrase(offer.CreatedAt.ToString("yyyy-MM-dd"), detailsFont)) { Border = 0 });

            // Valid Until
            details.AddCell(new PdfPCell(new Phrase("Valid Until:", boldFont)) { Border = 0 });
            details.AddCell(new PdfPCell(new Phrase(offer.ValidUntil.ToString("yyyy-MM-dd"), detailsFont)) { Border = 0 });

            // Created By
            details.AddCell(new PdfPCell(new Phrase("Created By:", boldFont)) { Border = 0 });
            details.AddCell(new PdfPCell(new Phrase(creator != null ? $"{creator.FirstName} {creator.LastName}" : "N/A", detailsFont)) { Border = 0 });

            // Assigned To
            details.AddCell(new PdfPCell(new Phrase("Assigned To:", boldFont)) { Border = 0 });
            details.AddCell(new PdfPCell(new Phrase(salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "N/A", detailsFont)) { Border = 0 });

            document.Add(details);
        }

        private void AddEquipmentTable(Document document, IEnumerable<OfferEquipment> equipment, SalesOffer offer)
        {
            if (!equipment.Any()) return;

            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2f, 1.5f, 1.5f, 1.5f, 1.5f });
            table.SpacingBefore = 10;
            table.SpacingAfter = 15;

            // Header
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            var headerCellFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);

            var headers = new[] { "Equipment", "Model", "Provider", "Country", "Price" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = BaseColor.DARK_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(cell);
            }

            // Data rows
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var item in equipment)
            {
                table.AddCell(new PdfPCell(new Phrase(item.Name, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.Model ?? "N/A", dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.Provider ?? "N/A", dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.Country ?? "N/A", dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase($"${item.Price:F2}", dataFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            }

            document.Add(table);
        }

        private void AddTermsSection(Document document, OfferTerms terms)
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            document.Add(new Paragraph("General Terms and Conditions", titleFont) { SpacingBefore = 10, SpacingAfter = 10 });

            if (!string.IsNullOrEmpty(terms.WarrantyPeriod))
            {
                document.Add(new Paragraph($"Warranty: {terms.WarrantyPeriod}", textFont));
            }
            if (!string.IsNullOrEmpty(terms.DeliveryTime))
            {
                document.Add(new Paragraph($"Delivery: {terms.DeliveryTime}", textFont));
            }
            if (!string.IsNullOrEmpty(terms.MaintenanceTerms))
            {
                document.Add(new Paragraph($"Maintenance: {terms.MaintenanceTerms}", textFont));
            }
            if (!string.IsNullOrEmpty(terms.OtherTerms))
            {
                document.Add(new Paragraph($"Other: {terms.OtherTerms}", textFont));
            }

            document.Add(new Paragraph("\n", textFont));
        }

        private void AddPaymentSection(Document document, SalesOffer offer, IEnumerable<InstallmentPlan> installments)
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            document.Add(new Paragraph("Payment Information", titleFont) { SpacingBefore = 10, SpacingAfter = 10 });

            // Payment type
            var paymentType = offer.PaymentType ?? "Not specified";
            document.Add(new Paragraph($"Payment Type: {paymentType}", boldFont));

            // Total and Final Price
            document.Add(new Paragraph($"Total Amount: ${offer.TotalAmount:F2}", boldFont));
            if (offer.FinalPrice.HasValue)
            {
                document.Add(new Paragraph($"Final Price: ${offer.FinalPrice.Value:F2}", boldFont));
            }

            // Installment schedule
            if (installments.Any())
            {
                document.Add(new Paragraph("\nInstallment Schedule:", boldFont) { SpacingAfter = 5 });

                var table = new PdfPTable(4) { WidthPercentage = 80 };
                table.SetWidths(new float[] { 1f, 2f, 2f, 1.5f });

                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                var headers = new[] { "#", "Amount", "Due Date", "Status" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = BaseColor.DARK_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                foreach (var installment in installments.OrderBy(i => i.InstallmentNumber))
                {
                    table.AddCell(new PdfPCell(new Phrase(installment.InstallmentNumber.ToString(), textFont)) { Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase($"${installment.Amount:F2}", textFont)) { Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase(installment.DueDate.ToString("yyyy-MM-dd"), textFont)) { Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase(installment.Status, textFont)) { Padding = 3 });
                }

                document.Add(table);
            }

            document.Add(new Paragraph("\n", textFont));
        }

        private void AddFooter(Document document, SalesOffer offer)
        {
            var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);

            document.Add(new Paragraph($"Offer ID: {offer.Id}", footerFont) { SpacingBefore = 20 });
            document.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), footerFont));
        }

        public byte[] GenerateLetterheadBackground()
        {
            // This can be used to generate a letterhead if we want to create it programmatically
            // For now, we'll use the extracted image
            return Array.Empty<byte>();
        }
    }
}

