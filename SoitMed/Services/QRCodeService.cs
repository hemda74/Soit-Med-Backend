using QRCoder;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SoitMed.Models.Equipment;

namespace SoitMed.Services
{
    public interface IQRCodeService
    {
        Task<QRCodeGenerationResult> GenerateQRCodeAsync(Equipment equipment);
        string GenerateUniqueQRCode(string equipmentName, string hospitalId);
        Task<bool> SaveQRCodePdfAsync(string qrCodeData, string equipmentName, string filePath);
    }

    public class QRCodeService : IQRCodeService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(IWebHostEnvironment environment, ILogger<QRCodeService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<QRCodeGenerationResult> GenerateQRCodeAsync(Equipment equipment)
        {
            try
            {
                // Generate unique QR code if not provided
                if (string.IsNullOrEmpty(equipment.QRCode))
                {
                    equipment.QRCode = GenerateUniqueQRCode(equipment.Name, equipment.HospitalId);
                }

                // Create QR code data with equipment information
                var qrData = CreateQRCodeData(equipment);

                // Generate QR code image
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                
                var pngByteQRCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = pngByteQRCode.GetGraphic(20);

                // Convert to base64 string for database storage
                var base64String = Convert.ToBase64String(qrCodeBytes);

                // Create QRs directory if it doesn't exist
                var qrDirectory = Path.Combine(_environment.WebRootPath, "qrs");
                Directory.CreateDirectory(qrDirectory);

                // Generate PDF file path
                var fileName = $"{equipment.QRCode}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var pdfPath = Path.Combine(qrDirectory, fileName);

                // Save as PDF
                await SaveQRCodePdfAsync(qrData, equipment.Name, pdfPath);

                return new QRCodeGenerationResult
                {
                    QRCode = equipment.QRCode,
                    QRCodeImageData = base64String,
                    QRCodePdfPath = $"qrs/{fileName}",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for equipment {EquipmentName}", equipment.Name);
                return new QRCodeGenerationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public string GenerateUniqueQRCode(string equipmentName, string hospitalId)
        {
            // Create a unique identifier combining timestamp, equipment name, and hospital
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var cleanName = new string(equipmentName.Where(char.IsLetterOrDigit).ToArray());
            var cleanHospitalId = new string(hospitalId.Where(char.IsLetterOrDigit).ToArray());
            
            return $"EQ-{cleanHospitalId}-{cleanName}-{timestamp}".ToUpper();
        }

        public Task<bool> SaveQRCodePdfAsync(string qrCodeData, string equipmentName, string filePath)
        {
            try
            {
                using var document = new Document(PageSize.A4, 50, 50, 50, 50);
                using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                
                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph($"Equipment QR Code - {equipmentName}", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                // Generate QR code image for PDF
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeDataObj = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
                var pngByteQRCode = new PngByteQRCode(qrCodeDataObj);
                var imageBytes = pngByteQRCode.GetGraphic(10);

                var qrImage = iTextSharp.text.Image.GetInstance(imageBytes);
                qrImage.Alignment = Element.ALIGN_CENTER;
                qrImage.ScaleToFit(300f, 300f);
                document.Add(qrImage);

                // Add QR code data as text
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var dataText = new Paragraph($"\nQR Code: {qrCodeData}", dataFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20
                };
                document.Add(dataText);

                // Add generation timestamp
                var timestampText = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", dataFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 10
                };
                document.Add(timestampText);

                document.Close();
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving QR code PDF for equipment {EquipmentName}", equipmentName);
                return Task.FromResult(false);
            }
        }

        private string CreateQRCodeData(Equipment equipment)
        {
            // Create JSON-like data for the QR code
            return $@"{{
    ""equipmentId"": ""{equipment.Id}"",
    ""qrCode"": ""{equipment.QRCode}"",
    ""name"": ""{equipment.Name}"",
    ""hospitalId"": ""{equipment.HospitalId}"",
    ""model"": ""{equipment.Model ?? "N/A"}"",
    ""manufacturer"": ""{equipment.Manufacturer ?? "N/A"}"",
    ""createdAt"": ""{equipment.CreatedAt:yyyy-MM-dd}""
}}";
        }

    }

    public class QRCodeGenerationResult
    {
        public string QRCode { get; set; } = string.Empty;
        public string? QRCodeImageData { get; set; }
        public string? QRCodePdfPath { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
