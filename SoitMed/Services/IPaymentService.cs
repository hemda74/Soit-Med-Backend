using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentAsync(CreatePaymentDTO dto, string customerId);
        Task<PaymentResponseDTO?> GetPaymentAsync(int id);
        Task<IEnumerable<PaymentResponseDTO>> GetCustomerPaymentsAsync(string customerId);
        Task<PaymentResponseDTO> ProcessStripePaymentAsync(int paymentId, StripePaymentDTO dto);
        Task<PaymentResponseDTO> ProcessPayPalPaymentAsync(int paymentId, PayPalPaymentDTO dto);
        Task<PaymentResponseDTO> ProcessLocalGatewayPaymentAsync(int paymentId, LocalGatewayPaymentDTO dto);
        Task<PaymentResponseDTO> RecordCashPaymentAsync(int paymentId, CashPaymentDTO dto);
        Task<PaymentResponseDTO> RecordBankTransferAsync(int paymentId, BankTransferDTO dto);
        Task<PaymentResponseDTO> ProcessRefundAsync(int paymentId, RefundDTO dto, string userId);
    }
}

