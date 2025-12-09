using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Text;
using System.Text.Json;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for generating PDF documents for offers using HTML to PDF conversion (PuppeteerSharp)
    /// This approach: HTML → Render in Headless Chrome → Generate PDF (same as web app)
    /// </summary>
    public class OfferPdfService : IOfferPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<OfferPdfService> _logger;
        private static bool _browserDownloaded = false;
        private static readonly SemaphoreSlim _browserSemaphore = new SemaphoreSlim(1, 1);

        public OfferPdfService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment,
            ILogger<OfferPdfService> logger)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Generates PDF on-demand without saving to disk (returns bytes directly)
        /// </summary>
        public async Task<byte[]> GenerateOfferPdfAsync(long offerId, string language = "en")
        {
            IBrowser? browser = null;
            try
            {
                // Get offer with all related data
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    throw new ArgumentException($"Offer with ID {offerId} not found");

                var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var creator = await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy);
                var salesman = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);
                var equipmentEnumerable = await _unitOfWork.OfferEquipment.GetByOfferIdAsync(offerId);
                var equipment = equipmentEnumerable?.ToList();
                var terms = await _unitOfWork.OfferTerms.GetByOfferIdAsync(offerId);

                // Ensure browser is downloaded (only once)
                await EnsureBrowserDownloadedAsync();

                // Launch browser
                await _browserSemaphore.WaitAsync();
                try
                {
                    browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" } // Required for Linux servers
                    });
                }
                finally
                {
                    _browserSemaphore.Release();
                }

                // Create new page
                var page = await browser.NewPageAsync();

                // Generate HTML content
                var htmlContent = GenerateOfferHtml(offer, client, creator, salesman, equipment, terms, language);

                // Set content and wait for images to load (critical for letterhead)
                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });
                
                // Wait a bit more to ensure background images are fully loaded
                await page.WaitForTimeoutAsync(500);

                // Generate PDF with no margins to ensure letterhead shows fully
                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true, // Critical: must be true for background images
                    MarginOptions = new MarginOptions
                    {
                        Top = "0mm",
                        Right = "0mm",
                        Bottom = "0mm",
                        Left = "0mm"
                    }
                };
                var pdfBytes = await page.PdfDataAsync(pdfOptions);

                _logger.LogInformation("PDF generated on-demand for offer {OfferId} (size: {Size} bytes)", offerId, pdfBytes.Length);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for offer {OfferId}", offerId);
                throw;
            }
            finally
            {
                if (browser != null)
                {
                    try
                    {
                        await browser.CloseAsync();
                    }
                    catch { }
                }
            }
        }

        private async Task EnsureBrowserDownloadedAsync()
        {
            if (_browserDownloaded)
                return;

            try
            {
                await _browserSemaphore.WaitAsync();
                try
                {
                    if (_browserDownloaded)
                        return;

                    _logger.LogInformation("Downloading Chromium browser for PDF generation...");
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();
                    _browserDownloaded = true;
                    _logger.LogInformation("Chromium browser downloaded successfully");
                }
                finally
                {
                    _browserSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading Chromium browser");
                throw;
            }
        }

        /// <summary>
        /// Load letterhead image as base64 (tries JPG first, then PDF conversion)
        /// </summary>
        private string? GetLetterheadImageBase64()
        {
            try
            {
                // Try JPG first (lowercase)
                var jpgPath = Path.Combine(_environment.WebRootPath, "letterhead.jpg");
                if (File.Exists(jpgPath))
                {
                    var imageBytes = File.ReadAllBytes(jpgPath);
                    var base64 = Convert.ToBase64String(imageBytes);
                    _logger.LogInformation("Letterhead loaded successfully from letterhead.jpg ({Size} bytes)", imageBytes.Length);
                    return $"data:image/jpeg;base64,{base64}";
                }

                // Try uppercase JPG
                var jpgPathUpper = Path.Combine(_environment.WebRootPath, "Letterhead.jpg");
                if (File.Exists(jpgPathUpper))
                {
                    var imageBytes = File.ReadAllBytes(jpgPathUpper);
                    var base64 = Convert.ToBase64String(imageBytes);
                    _logger.LogInformation("Letterhead loaded successfully from Letterhead.jpg ({Size} bytes)", imageBytes.Length);
                    return $"data:image/jpeg;base64,{base64}";
                }

                // Try PNG
                var pngPath = Path.Combine(_environment.WebRootPath, "letterhead.png");
                if (File.Exists(pngPath))
                {
                    var imageBytes = File.ReadAllBytes(pngPath);
                    var base64 = Convert.ToBase64String(imageBytes);
                    _logger.LogInformation("Letterhead loaded successfully from letterhead.png ({Size} bytes)", imageBytes.Length);
                    return $"data:image/png;base64,{base64}";
                }

                // Try uppercase PNG
                var pngPathUpper = Path.Combine(_environment.WebRootPath, "Letterhead.png");
                if (File.Exists(pngPathUpper))
                {
                    var imageBytes = File.ReadAllBytes(pngPathUpper);
                    var base64 = Convert.ToBase64String(imageBytes);
                    _logger.LogInformation("Letterhead loaded successfully from Letterhead.png ({Size} bytes)", imageBytes.Length);
                    return $"data:image/png;base64,{base64}";
                }

                // Try PDF (would need PDF to image conversion, but for now just log warning)
                var pdfPath = Path.Combine(_environment.WebRootPath, "Letterhead.pdf");
                if (File.Exists(pdfPath))
                {
                    _logger.LogWarning("Letterhead.pdf found but PDF to image conversion not implemented. Please use letterhead.jpg or letterhead.png instead.");
                }
                else
                {
                    _logger.LogWarning("Letterhead image not found. Expected: letterhead.jpg, Letterhead.jpg, letterhead.png, or Letterhead.png in wwwroot folder.");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading letterhead image");
                return null;
            }
        }

        /// <summary>
        /// Get full image URL for equipment images
        /// </summary>
        private string GetImageUrl(string? imagePath, string baseUrl = "")
        {
            if (string.IsNullOrEmpty(imagePath))
                return "";

            // If already a full URL, return as is
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            // If starts with /, it's already a relative path
            if (imagePath.StartsWith("/"))
                return imagePath;

            // Otherwise, construct relative path
            return $"/{imagePath.Replace('\\', '/')}";
        }

        private string GenerateOfferHtml(
            SalesOffer offer,
            Client? client,
            ApplicationUser? creator,
            ApplicationUser? salesman,
            List<OfferEquipment>? equipment,
            OfferTerms? terms,
            string language)
        {
            var isRTL = language == "ar";
            var dir = isRTL ? "rtl" : "ltr";
            
            // Get letterhead image
            var letterheadImage = GetLetterheadImageBase64();
            var letterheadUrl = letterheadImage ?? "";
            
            // Log letterhead status for debugging
            if (string.IsNullOrEmpty(letterheadUrl))
            {
                _logger.LogWarning("Letterhead image not loaded for offer {OfferId}. PDF will be generated without letterhead background.", offer.Id);
            }
            else
            {
                _logger.LogInformation("Letterhead image loaded successfully for offer {OfferId} (base64 length: {Length})", offer.Id, letterheadUrl.Length);
            }

            // Format helpers
            string FormatCurrency(decimal amount)
            {
                var formatted = amount.ToString("N2");
                return language == "ar" ? $"{formatted} جنيه" : $"{formatted} EGP";
            }

            string FormatDate(DateTime date)
            {
                return date.ToString("dd/MM/yyyy");
            }

            // Translation keys (simplified - you might want to use a proper i18n system)
            var t = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string>
                {
                    ["date"] = "Date",
                    ["dearClient"] = "Dear Client",
                    ["documentTitle"] = "We are pleased to present our offer for the following medical equipment:",
                    ["productsEquipment"] = "Products / Equipment",
                    ["productName"] = "Product Name",
                    ["provider"] = "Provider",
                    ["country"] = "Country",
                    ["price"] = "Price",
                    ["noImage"] = "No Image",
                    ["financialSummary"] = "Financial Summary",
                    ["subtotal"] = "Subtotal",
                    ["discount"] = "Discount",
                    ["totalAmount"] = "Total Amount",
                    ["termsConditions"] = "Terms & Conditions",
                    ["paymentTerms"] = "Payment Terms",
                    ["deliveryTerms"] = "Delivery Terms",
                    ["warranty"] = "Warranty Terms",
                    ["validUntil"] = "Valid Until",
                    ["salesman"] = "Salesman"
                },
                ["ar"] = new Dictionary<string, string>
                {
                    ["date"] = "التاريخ",
                    ["dearClient"] = "عزيزي العميل",
                    ["documentTitle"] = "يسرنا أن نقدم لكم عرضنا للمعدات الطبية التالية:",
                    ["productsEquipment"] = "المنتجات / المعدات",
                    ["productName"] = "اسم المنتج",
                    ["provider"] = "المزود",
                    ["country"] = "البلد",
                    ["price"] = "السعر",
                    ["noImage"] = "لا توجد صورة",
                    ["financialSummary"] = "الملخص المالي",
                    ["subtotal"] = "المجموع الفرعي",
                    ["discount"] = "الخصم",
                    ["totalAmount"] = "المبلغ الإجمالي",
                    ["termsConditions"] = "الشروط والأحكام",
                    ["paymentTerms"] = "شروط الدفع",
                    ["deliveryTerms"] = "شروط التسليم",
                    ["warranty"] = "شروط الضمان",
                    ["validUntil"] = "صالح حتى",
                    ["salesman"] = "مندوب المبيعات"
                }
            };

            var translations = t[language];

            // Calculate discount (if any)
            var discountAmount = 0m;
            if (offer.FinalPrice.HasValue && offer.FinalPrice < offer.TotalAmount)
            {
                discountAmount = offer.TotalAmount - offer.FinalPrice.Value;
            }
            var finalAmount = offer.TotalAmount - discountAmount;

            // Calculate content area matching jsPDF logic
            // A4 page: 210mm x 297mm
            double pageHeight = 297.0; // mm
            double pageWidth = 210.0; // mm
            double topContentMargin = pageHeight * 0.16; // 16% from top = ~47.5mm
            double bottomContentMargin = pageHeight * 0.15; // 15% from bottom = ~44.5mm
            double contentStartY = topContentMargin; // Content starts here
            double contentEndY = pageHeight - bottomContentMargin; // Content ends here
            double availableContentHeight = contentEndY - contentStartY; // ~205mm
            
            // Calculate row heights dynamically (matching jsPDF: 20% of page height per product, but adjusted)
            double baseRowHeight = pageHeight * 0.2; // 20% = ~59.4mm per product
            double minRowHeight = 50.0; // Minimum 50mm per product row
            double rowSpacing = 5.0; // 5mm spacing between products
            
            // Header section heights (approximate, matching jsPDF)
            double headerHeight = 60.0; // Date + greeting + intro + title
            double tableHeaderHeight = 8.0; // Table header row
            double financialSectionHeight = 80.0; // Financial summary
            double termsSectionHeight = 100.0; // Terms & conditions (variable)
            double footerHeight = 40.0; // Footer
            
            // Split equipment into pages based on available space (matching jsPDF logic)
            var productPages = new List<List<OfferEquipment>>();
            
            if (equipment != null && equipment.Any())
            {
                var currentPageProducts = new List<OfferEquipment>();
                double currentPageUsedHeight = headerHeight; // First page has header
                bool isFirstPage = true;
                
                foreach (var product in equipment)
                {
                    // Calculate product row height
                    // Estimate: base height adjusted for content (name, description, image)
                    double productRowHeight = baseRowHeight;
                    
                    // Adjust based on description length (if description is long, add more height)
                    if (!string.IsNullOrEmpty(product.Description) && product.Description.Length > 100)
                    {
                        productRowHeight += 10.0; // Add 10mm for long descriptions
                    }
                    
                    // Ensure minimum height
                    productRowHeight = Math.Max(productRowHeight, minRowHeight);
                    
                    // Check if this product fits on current page
                    // Account for table header if this is the first product on a new page
                    double requiredHeight = productRowHeight;
                    if (currentPageProducts.Count == 0 && !isFirstPage)
                    {
                        requiredHeight += tableHeaderHeight; // Need table header on new product pages
                    }
                    
                    // Check if adding this product would exceed available space
                    // Leave some margin (10mm) for safety
                    if (currentPageUsedHeight + requiredHeight + 10.0 > availableContentHeight)
                    {
                        // Current page is full, start a new page
                        if (currentPageProducts.Any())
                        {
                            productPages.Add(currentPageProducts);
                        }
                        currentPageProducts = new List<OfferEquipment> { product };
                        currentPageUsedHeight = tableHeaderHeight + productRowHeight; // New page: header + first product
                        isFirstPage = false;
                    }
                    else
                    {
                        // Product fits, add to current page
                        if (currentPageProducts.Count == 0 && !isFirstPage)
                        {
                            currentPageUsedHeight += tableHeaderHeight; // Add table header for new page
                        }
                        currentPageProducts.Add(product);
                        currentPageUsedHeight += productRowHeight + rowSpacing; // Add product + spacing
                    }
                }
                
                // Add the last page if it has products
                if (currentPageProducts.Any())
                {
                    productPages.Add(currentPageProducts);
                }
            }
            else
            {
                // If no equipment, create one empty page for header/footer
                productPages.Add(new List<OfferEquipment>());
            }

            // Build product rows HTML
            string BuildProductRows(List<OfferEquipment> pageProducts)
            {
                var rows = new StringBuilder();
                foreach (var eq in pageProducts)
                {
                    var imageUrl = GetImageUrl(eq.ImagePath);
                    var providerImageUrl = GetImageUrl(eq.ProviderImagePath);
                    var description = eq.Description ?? "";
                    
                    rows.AppendLine("<tr class='product-row'>");
                    rows.AppendLine("<td class='col-product'>");
                    rows.AppendLine($"<div class='product-name'>{System.Net.WebUtility.HtmlEncode(eq.Name)}</div>");
                    if (!string.IsNullOrEmpty(description))
                    {
                        rows.AppendLine($"<div class='product-description'>{System.Net.WebUtility.HtmlEncode(description)}</div>");
                    }
                    if (!string.IsNullOrEmpty(eq.Model))
                    {
                        rows.AppendLine($"<div class='product-model'>{System.Net.WebUtility.HtmlEncode(eq.Model)}</div>");
                    }
                    rows.AppendLine("</td>");
                    rows.AppendLine("<td class='col-image'>");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        rows.AppendLine($"<img src='{imageUrl}' alt='{System.Net.WebUtility.HtmlEncode(eq.Name)}' class='product-image' onerror='this.style.display=\"none\"'>");
                    }
                    else
                    {
                        rows.AppendLine($"<div class='no-image-placeholder'><span>{translations["noImage"]}</span></div>");
                    }
                    rows.AppendLine("</td>");
                    rows.AppendLine("<td class='col-provider'>");
                    if (!string.IsNullOrEmpty(providerImageUrl))
                    {
                        rows.AppendLine($"<img src='{providerImageUrl}' alt='{System.Net.WebUtility.HtmlEncode(eq.Provider ?? "")}' class='provider-logo' onerror='this.style.display=\"none\"'>");
                    }
                    rows.AppendLine($"<span class='provider-name'>{System.Net.WebUtility.HtmlEncode(eq.Provider ?? "N/A")}</span>");
                    if (!string.IsNullOrEmpty(eq.Country))
                    {
                        rows.AppendLine($"<div class='country-info'>{System.Net.WebUtility.HtmlEncode(eq.Country)}</div>");
                    }
                    rows.AppendLine("</td>");
                    rows.AppendLine($"<td class='col-price'><span class='price-value'>{FormatCurrency(eq.Price)}</span></td>");
                    rows.AppendLine("</tr>");
                }
                return rows.ToString();
            }

            // Build terms rows
            var termsRows = new StringBuilder();
            if (!string.IsNullOrEmpty(offer.PaymentTerms))
            {
                try
                {
                    var paymentTermsList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(offer.PaymentTerms);
                    if (paymentTermsList != null && paymentTermsList.Any())
                    {
                        termsRows.AppendLine($"<tr><td class='label'>{translations["paymentTerms"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(string.Join(", ", paymentTermsList))}</td></tr>");
                    }
                    else
                    {
                        termsRows.AppendLine($"<tr><td class='label'>{translations["paymentTerms"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(offer.PaymentTerms)}</td></tr>");
                    }
                }
                catch
                {
                    termsRows.AppendLine($"<tr><td class='label'>{translations["paymentTerms"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(offer.PaymentTerms)}</td></tr>");
                }
            }
            if (!string.IsNullOrEmpty(offer.DeliveryTerms))
            {
                try
                {
                    var deliveryTermsList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(offer.DeliveryTerms);
                    if (deliveryTermsList != null && deliveryTermsList.Any())
                    {
                        termsRows.AppendLine($"<tr><td class='label'>{translations["deliveryTerms"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(string.Join(", ", deliveryTermsList))}</td></tr>");
                    }
                    else
                    {
                        termsRows.AppendLine($"<tr><td class='label'>{translations["deliveryTerms"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(offer.DeliveryTerms)}</td></tr>");
                    }
                }
                catch
                {
                    termsRows.AppendLine($"<tr><td class='label'>{translations["deliveryTerms"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(offer.DeliveryTerms)}</td></tr>");
                }
            }
            if (!string.IsNullOrEmpty(offer.WarrantyTerms))
            {
                try
                {
                    var warrantyTermsList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(offer.WarrantyTerms);
                    if (warrantyTermsList != null && warrantyTermsList.Any())
                    {
                        termsRows.AppendLine($"<tr><td class='label'>{translations["warranty"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(string.Join(", ", warrantyTermsList))}</td></tr>");
                    }
                    else
                    {
                        termsRows.AppendLine($"<tr><td class='label'>{translations["warranty"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(offer.WarrantyTerms)}</td></tr>");
                    }
                }
                catch
                {
                    termsRows.AppendLine($"<tr><td class='label'>{translations["warranty"]}</td><td class='value'>{System.Net.WebUtility.HtmlEncode(offer.WarrantyTerms)}</td></tr>");
                }
            }

            // Get valid until date
            string validUntilText = "";
            if (!string.IsNullOrEmpty(offer.ValidUntil))
            {
                try
                {
                    var validUntilDates = System.Text.Json.JsonSerializer.Deserialize<List<string>>(offer.ValidUntil);
                    if (validUntilDates != null && validUntilDates.Any())
                    {
                        validUntilText = FormatDate(DateTime.Parse(validUntilDates[0]));
                    }
                }
                catch
                {
                    try
                    {
                        validUntilText = FormatDate(DateTime.Parse(offer.ValidUntil));
                    }
                    catch { }
                }
            }

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<style>");
            html.AppendLine(GetPrintStyles(isRTL, letterheadUrl));
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            // Ensure letterhead URL is properly formatted and escaped for CSS
            var letterheadStyle = string.IsNullOrEmpty(letterheadUrl) 
                ? "" 
                : $"style=\"--letterhead-url: url('{letterheadUrl.Replace("'", "\\'").Replace("\"", "\\\"")}')\"";
            
            html.AppendLine($"<div class='offer-print-container {(isRTL ? "rtl" : "ltr")}' dir='{dir}' {letterheadStyle}>");
            html.AppendLine("<div class='letterhead-background' aria-hidden='true'></div>");

            // Build pages
            for (int pageIndex = 0; pageIndex < productPages.Count; pageIndex++)
            {
                var pageProducts = productPages[pageIndex];
                html.AppendLine($"<div class='product-page {(pageIndex > 0 ? "page-break" : "")}'>");
                html.AppendLine("<div class='page-content'>");

                // Header only on first page
                if (pageIndex == 0)
                {
                    html.AppendLine("<header class='offer-header'>");
                    html.AppendLine("<div class='date-section'>");
                    html.AppendLine($"<span class='label'>{translations["date"]}:</span>");
                    html.AppendLine($"<span class='value'>{FormatDate(offer.CreatedAt)}</span>");
                    html.AppendLine("</div>");
                    html.AppendLine("<div class='client-greeting'>");
                    html.AppendLine($"<span>{translations["dearClient"]}, {System.Net.WebUtility.HtmlEncode(client?.Name ?? "N/A")}</span>");
                    html.AppendLine("</div>");
                    html.AppendLine($"<p class='document-intro'>{translations["documentTitle"]}</p>");
                    html.AppendLine("</header>");
                }

                // Products section
                if (pageProducts.Any())
                {
                    html.AppendLine("<section class='products-section'>");
                    if (pageIndex == 0)
                    {
                        html.AppendLine($"<h2 class='section-title'>{translations["productsEquipment"]}</h2>");
                    }
                    html.AppendLine("<table class='products-table'>");
                    html.AppendLine("<thead>");
                    html.AppendLine("<tr>");
                    html.AppendLine($"<th class='col-product'>{translations["productName"]}</th>");
                    html.AppendLine("<th class='col-image'></th>");
                    html.AppendLine($"<th class='col-provider'>{translations["provider"]} / {translations["country"]}</th>");
                    html.AppendLine($"<th class='col-price'>{translations["price"]}</th>");
                    html.AppendLine("</tr>");
                    html.AppendLine("</thead>");
                    html.AppendLine("<tbody>");
                    html.AppendLine(BuildProductRows(pageProducts));
                    html.AppendLine("</tbody>");
                    html.AppendLine("</table>");
                    html.AppendLine("</section>");
                }

                // Financial summary and terms only on last page
                // Check if they fit, otherwise add to a new page
                if (pageIndex == productPages.Count - 1)
                {
                    html.AppendLine("<section class='financial-section'>");
                    html.AppendLine($"<h2 class='section-title'>{translations["financialSummary"]}</h2>");
                    html.AppendLine("<table class='financial-table'>");
                    html.AppendLine("<tbody>");
                    html.AppendLine($"<tr><td class='label'>{translations["subtotal"]}</td><td class='value'>{FormatCurrency(offer.TotalAmount)}</td></tr>");
                    if (discountAmount > 0)
                    {
                        html.AppendLine($"<tr><td class='label'>{translations["discount"]}</td><td class='value discount'>- {FormatCurrency(discountAmount)}</td></tr>");
                    }
                    html.AppendLine($"<tr class='total-row'><td class='label'>{translations["totalAmount"]}</td><td class='value'>{FormatCurrency(finalAmount)}</td></tr>");
                    html.AppendLine("</tbody>");
                    html.AppendLine("</table>");
                    html.AppendLine("</section>");

                    if (termsRows.Length > 0)
                    {
                        html.AppendLine("<section class='terms-section'>");
                        html.AppendLine($"<h2 class='section-title'>{translations["termsConditions"]}</h2>");
                        html.AppendLine("<table class='terms-table'>");
                        html.AppendLine("<tbody>");
                        html.AppendLine(termsRows.ToString());
                        html.AppendLine("</tbody>");
                        html.AppendLine("</table>");
                        html.AppendLine("</section>");
                    }

                    html.AppendLine("<footer class='offer-footer'>");
                    if (!string.IsNullOrEmpty(validUntilText))
                    {
                        html.AppendLine("<div class='footer-row'>");
                        html.AppendLine($"<span class='label'>{translations["validUntil"]}:</span>");
                        html.AppendLine($"<span class='value'>{validUntilText}</span>");
                        html.AppendLine("</div>");
                    }
                    if (salesman != null)
                    {
                        html.AppendLine("<div class='footer-row'>");
                        html.AppendLine($"<span class='label'>{translations["salesman"]}:</span>");
                        html.AppendLine($"<span class='value'>{System.Net.WebUtility.HtmlEncode($"{salesman.FirstName} {salesman.LastName}".Trim())}</span>");
                        html.AppendLine("</div>");
                    }
                    html.AppendLine("</footer>");
                }

                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// Get print styles matching web app structure
        /// </summary>
        private string GetPrintStyles(bool isRTL, string letterheadUrl)
        {
            var fontFamily = isRTL ? "'Cairo', 'Amiri', 'Segoe UI', Tahoma, sans-serif" : "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
            
            return $@"
:root {{
    --print-primary: #2980b9;
    --print-secondary: #34495e;
    --print-text: #1e1e1e;
    --print-text-muted: #505050;
    --print-border: #c8c8c8;
    --print-border-light: #dcdcdc;
    --print-bg-header: #f0f0f0;
    --print-white: #ffffff;
    --letterhead-url: {(string.IsNullOrEmpty(letterheadUrl) ? "none" : $"url('{letterheadUrl.Replace("'", "\\'").Replace("\"", "\\\"")}')")};
}}

* {{
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}}

body {{
    font-family: {fontFamily};
    font-size: 10pt;
    color: var(--print-text);
    line-height: 1.4;
    background: white; /* White background for PDF */
    margin: 0;
    padding: 0;
}}

.offer-print-container {{
    position: relative;
    width: 210mm;
    min-height: 297mm;
    margin: 0 auto;
    background: transparent; /* Transparent so letterhead shows */
    box-shadow: none; /* Remove shadow for print */
}}

.offer-print-container.rtl {{
    direction: rtl;
    font-family: {fontFamily};
}}

.offer-print-container.ltr {{
    direction: ltr;
}}

.letterhead-background {{
    display: none;
}}

.page-content {{
    position: relative;
    z-index: 2;
    padding: 47.5mm 15mm 44.5mm 15mm; /* Match jsPDF: 16% top (~47.5mm), 15% bottom (~44.5mm) on A4 (297mm) */
    margin: 0;
    box-sizing: border-box;
    background: transparent !important; /* Ensure transparent so letterhead shows through */
    min-height: 205mm; /* Content area: 297mm - 47.5mm - 44.5mm = 205mm (available for content) */
    max-height: 205mm; /* Limit to available content area */
    width: 100%;
    overflow: visible;
}}

.offer-header {{
    margin-bottom: 12pt;
}}

.date-section {{
    margin-bottom: 8pt;
    font-size: 11pt;
}}

.date-section .label {{
    font-weight: 600;
    color: var(--print-secondary);
}}

.date-section .value {{
    margin-inline-start: 8pt;
    color: var(--print-secondary);
}}

.client-greeting {{
    font-size: 11pt;
    color: var(--print-secondary);
    margin-bottom: 8pt;
}}

.document-intro {{
    font-size: 10pt;
    color: var(--print-secondary);
    line-height: 1.5;
}}

.section-title {{
    font-size: 13pt;
    font-weight: 700;
    color: var(--print-primary);
    margin: 16pt 0 8pt 0;
}}

.products-table {{
    width: 100%;
    border-collapse: collapse;
    border: 1px solid var(--print-border);
    margin-bottom: 12pt;
}}

.products-table th,
.products-table td {{
    border: 1px solid var(--print-border);
    padding: 8pt 6pt;
    vertical-align: top;
    text-align: start;
}}

.products-table thead th {{
    background: var(--print-bg-header);
    font-weight: 600;
    font-size: 9pt;
    color: var(--print-primary);
    text-align: center;
}}

.col-product {{ width: 25%; }}
.col-image {{ width: 30%; text-align: center; }}
.col-provider {{ width: 15%; }}
.col-price {{ width: 15%; text-align: end; }}

.product-page {{
    position: relative;
    width: 210mm;
    min-height: 297mm;
    height: 297mm;
    margin: 0;
    padding: 0;
    background-color: white;
    background-image: var(--letterhead-url);
    background-repeat: no-repeat;
    background-size: 210mm 297mm !important; /* Exact A4 size to match letterhead dimensions */
    background-position: top left !important;
    background-origin: border-box !important;
    background-clip: border-box !important;
    background-attachment: local;
    box-sizing: border-box;
    page-break-after: always;
    page-break-inside: avoid;
    -webkit-print-color-adjust: exact !important;
    print-color-adjust: exact !important;
    image-rendering: -webkit-optimize-contrast;
    image-rendering: crisp-edges;
    overflow: hidden;
}}

.product-page:last-child {{
    page-break-after: auto;
}}

.product-row {{
    page-break-inside: avoid;
    min-height: 50mm; /* Minimum height matching jsPDF */
    height: auto; /* Allow dynamic height based on content */
    vertical-align: top;
    margin-bottom: 5mm; /* 5mm spacing between products (matching jsPDF rowSpacing) */
}}

.product-row:nth-child(even) {{
    background: rgba(240, 240, 240, 0.3);
}}

.product-row td {{
    vertical-align: middle;
    padding: 8pt;
}}

.product-name {{
    font-size: 11pt;
    font-weight: 600;
    color: var(--print-text);
    margin-bottom: 4pt;
}}

.product-description {{
    font-size: 9pt;
    color: var(--print-text-muted);
    line-height: 1.3;
    margin-bottom: 4pt;
}}

.product-model {{
    font-size: 9pt;
    color: var(--print-text-muted);
    font-style: italic;
}}

.product-image {{
    width: 50mm;
    height: 50mm;
    object-fit: contain;
    display: block;
    margin: 0 auto;
    image-rendering: -webkit-optimize-contrast;
    image-rendering: crisp-edges;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
}}

.no-image-placeholder {{
    display: flex;
    align-items: center;
    justify-content: center;
    width: 50mm;
    height: 50mm;
    border: 1px dashed var(--print-border);
    background: #fafafa;
    color: var(--print-text-muted);
    font-size: 8pt;
    margin: 0 auto;
}}

.provider-info {{
    display: flex;
    flex-direction: column;
    gap: 4pt;
    margin-bottom: 6pt;
}}

.provider-logo {{
    width: 30mm;
    height: auto;
    max-height: 15mm;
    object-fit: contain;
    margin-bottom: 4pt;
}}

.provider-name {{
    font-size: 9pt;
    color: var(--print-text);
    font-weight: 500;
}}

.country-info {{
    font-size: 8pt;
    color: var(--print-text-muted);
    margin-top: 2pt;
}}

.price-value {{
    font-size: 10pt;
    font-weight: 600;
    color: var(--print-text);
}}

.financial-table {{
    width: 100%;
    border-collapse: collapse;
    border: 1px solid var(--print-border);
    margin-bottom: 12pt;
}}

.financial-table td {{
    border: 1px solid var(--print-border);
    padding: 6pt 10pt;
    font-size: 9pt;
}}

.financial-table .label {{
    font-weight: 600;
    color: var(--print-secondary);
    width: 50%;
}}

.financial-table .value {{
    text-align: end;
    font-weight: 600;
    color: var(--print-text);
}}

.financial-table .value.discount {{
    color: #c0392b;
}}

.financial-table .total-row {{
    background: var(--print-bg-header);
}}

.financial-table .total-row .label,
.financial-table .total-row .value {{
    font-size: 10pt;
    font-weight: 700;
    color: var(--print-primary);
}}

.terms-table {{
    width: 100%;
    border-collapse: collapse;
    border: 1px solid var(--print-border);
}}

.terms-table td {{
    border: 1px solid var(--print-border);
    padding: 6pt 10pt;
    font-size: 8pt;
}}

.terms-table .label {{
    font-weight: 600;
    color: var(--print-secondary);
    width: 100pt;
}}

.terms-table .value {{
    color: var(--print-text);
}}

.offer-footer {{
    margin-top: 16pt;
    padding-top: 8pt;
    border-top: 1px solid var(--print-border-light);
}}

.footer-row {{
    display: flex;
    gap: 8pt;
    margin-bottom: 4pt;
    font-size: 10pt;
}}

.footer-row .label {{
    font-weight: 600;
    color: var(--print-secondary);
}}

.footer-row .value {{
    color: var(--print-text);
}}

@media print {{
    body {{
        margin: 0 !important;
        padding: 0 !important;
        background: white;
    }}

    @page {{
        size: A4 portrait;
        margin: 0;
    }}

    .offer-print-container {{
        width: 100%;
        min-height: 100vh;
        margin: 0;
        box-shadow: none;
    }}

    .product-page {{
        width: 100%;
        min-height: 100vh;
        height: 100vh;
        page-break-after: always;
        page-break-before: auto;
        page-break-inside: avoid;
        background-image: var(--letterhead-url);
        background-repeat: no-repeat;
        background-size: cover;
        background-position: top left;
        background-origin: padding-box;
        -webkit-print-color-adjust: exact !important;
        print-color-adjust: exact !important;
        margin: 0;
        padding: 0;
        display: block;
        overflow: hidden;
    }}

    .product-page:first-child {{
        page-break-before: avoid;
    }}

    .product-page:last-child {{
        page-break-after: auto;
    }}

    .page-content {{
        position: relative;
        z-index: 2;
        padding: 25% 15mm 25% 15mm;
        margin: 0;
        background: transparent;
    }}

    .product-row,
    .financial-table tr,
    .terms-table tr {{
        page-break-inside: avoid;
    }}
}}";
        }
    }
}

