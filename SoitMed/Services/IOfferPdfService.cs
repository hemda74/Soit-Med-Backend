namespace SoitMed.Services
{
    /// <summary>
    /// Service for generating PDF documents for offers (on-demand, no storage)
    /// </summary>
    public interface IOfferPdfService
    {
        /// <summary>
        /// Generates a PDF document for an offer and returns it as byte array (no file storage)
        /// </summary>
        /// <param name="offerId">The ID of the offer</param>
        /// <param name="language">Language for PDF (en or ar)</param>
        /// <returns>PDF file bytes</returns>
        Task<byte[]> GenerateOfferPdfAsync(string offerId, string language = "en");
    }
}

