using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IAccountingService
    {
        Task<IEnumerable<PaymentResponseDTO>> GetPendingPaymentsAsync();
        Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);
        Task<PaymentResponseDTO?> GetPaymentDetailsAsync(int paymentId);
        Task<PaymentResponseDTO> ConfirmPaymentAsync(int paymentId, ConfirmPaymentDTO dto, string accountantId);
        Task<PaymentResponseDTO> RejectPaymentAsync(int paymentId, RejectPaymentDTO dto, string accountantId);
        Task<FinancialReportDTO> GetDailyReportAsync(DateTime date);
        Task<FinancialReportDTO> GetMonthlyReportAsync(int year, int month);
        Task<FinancialReportDTO> GetYearlyReportAsync(int year);
        Task<IEnumerable<PaymentMethodStatisticsDTO>> GetPaymentMethodStatisticsAsync(DateTime from, DateTime to);
        Task<IEnumerable<PaymentResponseDTO>> GetOutstandingPaymentsAsync();
        Task<IEnumerable<PaymentResponseDTO>> GetMaintenancePaymentsAsync(PaymentFilters filters);
        Task<IEnumerable<PaymentResponseDTO>> GetSparePartsPaymentsAsync(PaymentFilters filters);
    }
}

