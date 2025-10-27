using iTextSharp.text;
using iTextSharp.text.pdf;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for generating PDF documents with letterhead
    /// </summary>
    public class PdfExportService : IPdfExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PdfExportService> _logger;
        private readonly string _letterheadPath;

        public PdfExportService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment,
            ILogger<PdfExportService> logger)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
            _logger = logger;
            _letterheadPath = Path.Combine(_environment.WebRootPath, "templates", "letterhead.png");
        }

        public async Task<byte[]> GenerateOfferPdfAsync(long offerId)
        {
            try
            {
                // Get offer details
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(offerId));

                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var equipment = (await _unitOfWork.OfferEquipment.GetByOfferIdAsync(offerId)).ToList();
                var terms = await _unitOfWork.OfferTerms.GetByOfferIdAsync(offerId);
                var installments = (await _unitOfWork.InstallmentPlans.GetByOfferIdAsync(offerId)).ToList();

                // Create PDF document
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Page size A4
                    Document document = new Document(PageSize.A4, 50, 50, 80, 50);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                    // Add header/footer handler if needed
                    writer.PageEvent = new PageEventHandler(_letterheadPath);

                    document.Open();

                    // Add title
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                    var titleParagraph = new Paragraph("OFFER DOCUMENT", titleFont);
                    titleParagraph.Alignment = Element.ALIGN_CENTER;
                    titleParagraph.SpacingAfter = 20f;
                    document.Add(titleParagraph);

                    // Add offer details
                    AddOfferHeader(document, offer, client);
                    
                    // Add equipment list if exists
                    if (equipment != null && equipment.Any())
                    {
                        AddEquipmentTable(document, equipment);
                    }

                    // Add terms if exists
                    if (terms != null)
                    {
                        AddTermsSection(document, terms);
                    }

                    // Add installment plan if exists
                    if (installments != null && installments.Any())
                    {
                        AddInstallmentPlan(document, installments);
                    }

                    // Add payment summary
                    AddPaymentSummary(document, offer);

                    document.Close();

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for offer {OfferId}", offerId);
                throw;
            }
        }

        private void AddOfferHeader(Document document, SalesOffer offer, Client? client)
        {
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
            
            document.Add(new Paragraph($"Client: {client?.Name ?? "N/A"}", normalFont));
            document.Add(new Paragraph($"Offer Date: {offer.CreatedAt:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph($"Valid Until: {offer.ValidUntil:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph($"Status: {offer.Status}", normalFont));
            
            document.Add(new Paragraph(" ")); // Empty line
        }

        private void AddEquipmentTable(Document document, List<OfferEquipment> equipment)
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);

            // Title
            var title = new Paragraph("Equipment List", titleFont);
            title.SpacingBefore = 10f;
            title.SpacingAfter = 10f;
            document.Add(title);

            // Create table
            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 2, 1.5f, 1.5f, 1.5f, 1.5f, 1f });

            // Header row
            AddCell(table, "Name", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Model", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Provider", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Country", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Status", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Price", headerFont, BaseColor.DARK_GRAY);

            // Data rows
            foreach (var item in equipment)
            {
                AddCell(table, item.Name, cellFont, BaseColor.WHITE);
                AddCell(table, item.Model ?? "N/A", cellFont, BaseColor.LIGHT_GRAY);
                AddCell(table, item.Provider ?? "N/A", cellFont, BaseColor.WHITE);
                AddCell(table, item.Country ?? "N/A", cellFont, BaseColor.LIGHT_GRAY);
                AddCell(table, item.InStock ? "In Stock" : "Out of Stock", cellFont, 
                    item.InStock ? BaseColor.GREEN : BaseColor.RED);
                AddCell(table, $"{item.Price:C}", cellFont, BaseColor.WHITE);
            }

            document.Add(table);
            document.Add(new Paragraph(" ")); // Empty line
        }

        private void AddTermsSection(Document document, OfferTerms terms)
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            var title = new Paragraph("Terms and Conditions", titleFont);
            title.SpacingBefore = 10f;
            title.SpacingAfter = 10f;
            document.Add(title);

            if (!string.IsNullOrEmpty(terms.WarrantyPeriod))
            {
                document.Add(new Paragraph($"Warranty: {terms.WarrantyPeriod}", normalFont));
            }
            if (!string.IsNullOrEmpty(terms.DeliveryTime))
            {
                document.Add(new Paragraph($"Delivery: {terms.DeliveryTime}", normalFont));
            }
            if (!string.IsNullOrEmpty(terms.MaintenanceTerms))
            {
                document.Add(new Paragraph($"Maintenance: {terms.MaintenanceTerms}", normalFont));
            }
            if (!string.IsNullOrEmpty(terms.OtherTerms))
            {
                document.Add(new Paragraph($"Other Terms: {terms.OtherTerms}", normalFont));
            }

            document.Add(new Paragraph(" ")); // Empty line
        }

        private void AddInstallmentPlan(Document document, List<InstallmentPlan> installments)
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);

            var title = new Paragraph("Installment Plan", titleFont);
            title.SpacingBefore = 10f;
            title.SpacingAfter = 10f;
            document.Add(title);

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100;

            AddCell(table, "Installment", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Amount", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Due Date", headerFont, BaseColor.DARK_GRAY);
            AddCell(table, "Status", headerFont, BaseColor.DARK_GRAY);

            foreach (var installment in installments.OrderBy(i => i.InstallmentNumber))
            {
                AddCell(table, installment.InstallmentNumber.ToString(), cellFont, BaseColor.WHITE);
                AddCell(table, $"{installment.Amount:C}", cellFont, BaseColor.LIGHT_GRAY);
                AddCell(table, installment.DueDate.ToString("dd/MM/yyyy"), cellFont, BaseColor.WHITE);
                AddCell(table, installment.Status, cellFont, BaseColor.LIGHT_GRAY);
            }

            document.Add(table);
            document.Add(new Paragraph(" ")); // Empty line
        }

        private void AddPaymentSummary(Document document, SalesOffer offer)
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            var title = new Paragraph("Payment Summary", titleFont);
            title.SpacingBefore = 10f;
            title.SpacingAfter = 10f;
            document.Add(title);

            document.Add(new Paragraph($"Total Amount: {offer.TotalAmount:C}", normalFont));
            
            if (offer.FinalPrice.HasValue && offer.FinalPrice != offer.TotalAmount)
            {
                document.Add(new Paragraph($"Final Price: {offer.FinalPrice:C}", normalFont));
            }

            if (!string.IsNullOrEmpty(offer.PaymentTerms))
            {
                document.Add(new Paragraph($"Payment Terms: {offer.PaymentTerms}", normalFont));
            }

            if (!string.IsNullOrEmpty(offer.PaymentType))
            {
                document.Add(new Paragraph($"Payment Type: {offer.PaymentType}", normalFont));
            }
        }

        private void AddCell(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = backgroundColor;
            cell.Padding = 5f;
            cell.Border = Rectangle.BOX;
            cell.BorderWidth = 1f;
            table.AddCell(cell);
        }

        private class PageEventHandler : PdfPageEventHelper
        {
            private readonly string _letterheadPath;

            public PageEventHandler(string letterheadPath)
            {
                _letterheadPath = letterheadPath;
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);

                // Add letterhead if file exists
                if (File.Exists(_letterheadPath))
                {
                    try
                    {
                        Image letterhead = Image.GetInstance(_letterheadPath);
                        letterhead.SetAbsolutePosition(0, 0);
                        letterhead.ScaleAbsolute(document.PageSize.Width, document.PageSize.Height);
                        letterhead.SetAbsolutePosition(0, document.PageSize.Height - letterhead.ScaledHeight);
                        writer.DirectContent.AddImage(letterhead);
                    }
                    catch
                    {
                        // Silently fail if letterhead cannot be added
                    }
                }
            }
        }
    }
}
