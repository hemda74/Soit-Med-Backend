using Microsoft.AspNetCore.Identity;
using SoitMed.DTO;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Collections.Generic;

namespace SoitMed.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AccountingService> _logger;

        public AccountingService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<AccountingService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetPendingPaymentsAsync()
        {
            var payments = await _unitOfWork.Payments.GetPendingPaymentsAsync();
            var result = new List<PaymentResponseDTO>();

            foreach (var payment in payments)
            {
                result.Add(await MapToResponseDTO(payment));
            }

            return result;
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
        {
            var payments = await _unitOfWork.Payments.GetByDateRangeAsync(from, to);
            var result = new List<PaymentResponseDTO>();

            foreach (var payment in payments)
            {
                result.Add(await MapToResponseDTO(payment));
            }

            return result;
        }

        public async Task<PaymentResponseDTO?> GetPaymentDetailsAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetWithDetailsAsync(paymentId);
            if (payment == null)
                return null;

            return await MapToResponseDTO(payment);
        }

        public async Task<PaymentResponseDTO> ConfirmPaymentAsync(int paymentId, ConfirmPaymentDTO dto, string accountantId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            payment.Status = PaymentStatus.Completed;
            payment.ProcessedByAccountantId = accountantId;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.ConfirmedAt = DateTime.UtcNow;
            payment.AccountingNotes = dto.Notes;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment confirmed. PaymentId: {PaymentId}, AccountantId: {AccountantId}", 
                paymentId, accountantId);

            // Notify customer
            await _notificationService.CreateNotificationAsync(
                payment.CustomerId,
                "Payment confirmed",
                $"Your payment of {payment.Amount} EGP has been confirmed",
                "Payment",
                "Medium",
                null,
                null,
                true,
                new Dictionary<string, object> { { "PaymentId", paymentId } }
            );

            return await MapToResponseDTO(payment);
        }

        public async Task<PaymentResponseDTO> RejectPaymentAsync(int paymentId, RejectPaymentDTO dto, string accountantId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            payment.Status = PaymentStatus.Failed;
            payment.ProcessedByAccountantId = accountantId;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.AccountingNotes = dto.Reason;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Notify customer
            await _notificationService.CreateNotificationAsync(
                payment.CustomerId,
                "Payment rejected",
                $"Your payment was rejected: {dto.Reason}",
                "Payment",
                "High",
                null,
                null,
                true,
                new Dictionary<string, object> { { "PaymentId", paymentId }, { "Reason", dto.Reason } }
            );

            return await MapToResponseDTO(payment);
        }

        public async Task<FinancialReportDTO> GetDailyReportAsync(DateTime date)
        {
            var from = date.Date;
            var to = date.Date.AddDays(1).AddTicks(-1);

            var payments = await _unitOfWork.Payments.GetByDateRangeAsync(from, to);
            var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed);

            var report = new FinancialReportDTO
            {
                ReportDate = date,
                TotalRevenue = completedPayments.Sum(p => p.Amount),
                TotalPayments = payments.Count(),
                OutstandingPayments = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                TotalTransactions = payments.Count(),
                RevenueByPaymentMethod = completedPayments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                CountByPaymentMethod = payments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return report;
        }

        public async Task<FinancialReportDTO> GetMonthlyReportAsync(int year, int month)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1).AddTicks(-1);

            var payments = await _unitOfWork.Payments.GetByDateRangeAsync(from, to);
            var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed);

            var report = new FinancialReportDTO
            {
                ReportDate = from,
                TotalRevenue = completedPayments.Sum(p => p.Amount),
                TotalPayments = payments.Count(),
                OutstandingPayments = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                TotalTransactions = payments.Count(),
                RevenueByPaymentMethod = completedPayments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                CountByPaymentMethod = payments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return report;
        }

        public async Task<FinancialReportDTO> GetYearlyReportAsync(int year)
        {
            var from = new DateTime(year, 1, 1);
            var to = new DateTime(year, 12, 31, 23, 59, 59);

            var payments = await _unitOfWork.Payments.GetByDateRangeAsync(from, to);
            var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed);

            var report = new FinancialReportDTO
            {
                ReportDate = from,
                TotalRevenue = completedPayments.Sum(p => p.Amount),
                TotalPayments = payments.Count(),
                OutstandingPayments = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                TotalTransactions = payments.Count(),
                RevenueByPaymentMethod = completedPayments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                CountByPaymentMethod = payments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return report;
        }

        public async Task<IEnumerable<PaymentMethodStatisticsDTO>> GetPaymentMethodStatisticsAsync(DateTime from, DateTime to)
        {
            var payments = await _unitOfWork.Payments.GetByDateRangeAsync(from, to);
            var statistics = payments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodStatisticsDTO
                {
                    PaymentMethod = g.Key,
                    PaymentMethodName = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount),
                    AverageAmount = g.Average(p => p.Amount),
                    SuccessCount = g.Count(p => p.Status == PaymentStatus.Completed),
                    FailedCount = g.Count(p => p.Status == PaymentStatus.Failed),
                    SuccessRate = g.Count() > 0 
                        ? (decimal)g.Count(p => p.Status == PaymentStatus.Completed) / g.Count() * 100 
                        : 0
                })
                .ToList();

            return statistics;
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetOutstandingPaymentsAsync()
        {
            var payments = await _unitOfWork.Payments.GetByStatusAsync(PaymentStatus.Pending);
            var result = new List<PaymentResponseDTO>();

            foreach (var payment in payments)
            {
                result.Add(await MapToResponseDTO(payment));
            }

            return result;
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetMaintenancePaymentsAsync(PaymentFilters filters)
        {
            var payments = await _unitOfWork.Payments.GetByMaintenanceRequestIdAsync(
                filters.MaintenanceRequestId ?? 0);
            
            // Apply additional filters
            if (filters.Status.HasValue)
                payments = payments.Where(p => p.Status == filters.Status.Value);
            if (filters.PaymentMethod.HasValue)
                payments = payments.Where(p => p.PaymentMethod == filters.PaymentMethod.Value);

            var result = new List<PaymentResponseDTO>();
            foreach (var payment in payments)
            {
                result.Add(await MapToResponseDTO(payment));
            }

            return result;
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetSparePartsPaymentsAsync(PaymentFilters filters)
        {
            var payments = await _unitOfWork.Payments.GetBySparePartRequestIdAsync(
                filters.SparePartRequestId ?? 0);
            
            // Apply additional filters
            if (filters.Status.HasValue)
                payments = payments.Where(p => p.Status == filters.Status.Value);
            if (filters.PaymentMethod.HasValue)
                payments = payments.Where(p => p.PaymentMethod == filters.PaymentMethod.Value);

            var result = new List<PaymentResponseDTO>();
            foreach (var payment in payments)
            {
                result.Add(await MapToResponseDTO(payment));
            }

            return result;
        }

        private async Task<PaymentResponseDTO> MapToResponseDTO(Models.Payment.Payment payment)
        {
            var customer = await _userManager.FindByIdAsync(payment.CustomerId);
            var accountant = payment.ProcessedByAccountantId != null
                ? await _userManager.FindByIdAsync(payment.ProcessedByAccountantId)
                : null;

            return new PaymentResponseDTO
            {
                Id = payment.Id,
                MaintenanceRequestId = payment.MaintenanceRequestId,
                SparePartRequestId = payment.SparePartRequestId,
                CustomerId = payment.CustomerId,
                CustomerName = customer?.UserName ?? "",
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentMethodName = payment.PaymentMethod.ToString(),
                Status = payment.Status,
                StatusName = payment.Status.ToString(),
                TransactionId = payment.TransactionId,
                PaymentReference = payment.PaymentReference,
                ProcessedByAccountantId = payment.ProcessedByAccountantId,
                ProcessedByAccountantName = accountant?.UserName ?? "",
                ProcessedAt = payment.ProcessedAt,
                AccountingNotes = payment.AccountingNotes,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                ConfirmedAt = payment.ConfirmedAt
            };
        }
    }
}

