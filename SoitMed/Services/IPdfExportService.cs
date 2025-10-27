namespace SoitMed.Services
{
    /// <summary>
    /// Interface for PDF export service
    /// </summary>
    public interface IPdfExportService
    {
        /// <summary>
        /// Generates a PDF document for an offer with letterhead
        /// </summary>
        Task<byte[]> GenerateOfferPdfAsync(long offerId);
    }
}

