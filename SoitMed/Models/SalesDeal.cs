using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a deal in the sales module with multi-level approval workflow
    /// </summary>
    public class SalesDeal : BaseEntity
    {
        #region Basic Information
        public long? OfferId { get; set; }
        
        [Required]
        public long ClientId { get; set; }
        
        [Required]
        public string SalesmanId { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DealValue { get; set; }
        
        [Required]
        public DateTime ClosedDate { get; set; }
        #endregion

        #region Terms and Conditions
        [MaxLength(2000)]
        public string? PaymentTerms { get; set; }
        
        [MaxLength(2000)]
        public string? DeliveryTerms { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        #endregion

        #region Status and Workflow
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = DealStatus.PendingManagerApproval;
        
        [MaxLength(2000)]
        public string? Notes { get; set; }
        #endregion

        #region Manager Approval
        public string? ManagerApprovedBy { get; set; }
        public DateTime? ManagerApprovedAt { get; set; }
        
        [MaxLength(50)]
        public string? ManagerRejectionReason { get; set; } // Money, CashFlow, OtherNeeds
        
        [MaxLength(2000)]
        public string? ManagerComments { get; set; }
        #endregion

        #region SuperAdmin Approval
        public string? SuperAdminApprovedBy { get; set; }
        public DateTime? SuperAdminApprovedAt { get; set; }
        
        [MaxLength(50)]
        public string? SuperAdminRejectionReason { get; set; } // Money, CashFlow, OtherNeeds
        
        [MaxLength(2000)]
        public string? SuperAdminComments { get; set; }
        #endregion

        #region Salesman Report
        [MaxLength(5000)]
        public string? ReportText { get; set; }
        
        [MaxLength(2000)]
        public string? ReportAttachments { get; set; } // JSON array of file paths/URLs
        
        public DateTime? ReportSubmittedAt { get; set; }
        #endregion

        #region Legal and Completion
        public DateTime? SentToLegalAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        [MaxLength(2000)]
        public string? CompletionNotes { get; set; }
        #endregion

        #region Navigation Properties
        public virtual SalesOffer? Offer { get; set; }
        public virtual Client Client { get; set; } = null!;
        public virtual ApplicationUser Salesman { get; set; } = null!;
        public virtual ApplicationUser? ManagerApprover { get; set; }
        public virtual ApplicationUser? SuperAdminApprover { get; set; }
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Manager approves the deal
        /// </summary>
        public void ApproveByManager(string managerId, string? comments = null)
        {
            ManagerApprovedBy = managerId;
            ManagerApprovedAt = DateTime.UtcNow;
            ManagerComments = comments;
            Status = DealStatus.PendingSuperAdminApproval;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Manager rejects the deal with reason
        /// </summary>
        public void RejectByManager(string managerId, string rejectionReason, string? comments = null)
        {
            ManagerApprovedBy = managerId;
            ManagerApprovedAt = DateTime.UtcNow;
            ManagerRejectionReason = rejectionReason;
            ManagerComments = comments;
            Status = DealStatus.RejectedByManager;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// SuperAdmin approves the deal
        /// </summary>
        public void ApproveBySuperAdmin(string superAdminId, string? comments = null)
        {
            SuperAdminApprovedBy = superAdminId;
            SuperAdminApprovedAt = DateTime.UtcNow;
            SuperAdminComments = comments;
            Status = DealStatus.AwaitingClientAccountCreation;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// SuperAdmin rejects the deal with reason
        /// </summary>
        public void RejectBySuperAdmin(string superAdminId, string rejectionReason, string? comments = null)
        {
            SuperAdminApprovedBy = superAdminId;
            SuperAdminApprovedAt = DateTime.UtcNow;
            SuperAdminRejectionReason = rejectionReason;
            SuperAdminComments = comments;
            Status = DealStatus.RejectedBySuperAdmin;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark that client account has been created (by Admin)
        /// </summary>
        public void MarkClientAccountCreated()
        {
            Status = DealStatus.AwaitingSalesmanReport;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Submit salesman report and send to legal team
        /// </summary>
        public void SubmitReportAndSendToLegal(string reportText, string? reportAttachments = null)
        {
            ReportText = reportText;
            ReportAttachments = reportAttachments;
            ReportSubmittedAt = DateTime.UtcNow;
            Status = DealStatus.SentToLegal;
            SentToLegalAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Send to legal team
        /// </summary>
        public void SendToLegal()
        {
            Status = DealStatus.SentToLegal;
            SentToLegalAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark deal as completed
        /// </summary>
        public void MarkAsCompleted(string? completionNotes = null)
        {
            Status = DealStatus.Success;
            CompletedAt = DateTime.UtcNow;
            CompletionNotes = completionNotes;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark deal as failed
        /// </summary>
        public void MarkAsFailed(string? failureNotes = null)
        {
            Status = DealStatus.Failed;
            CompletedAt = DateTime.UtcNow;
            CompletionNotes = failureNotes;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if deal needs manager approval
        /// </summary>
        public bool NeedsManagerApproval()
        {
            return Status == DealStatus.PendingManagerApproval;
        }

        /// <summary>
        /// Checks if deal needs super admin approval
        /// </summary>
        public bool NeedsSuperAdminApproval()
        {
            return Status == DealStatus.PendingSuperAdminApproval;
        }

        /// <summary>
        /// Checks if deal is approved
        /// </summary>
        public bool IsApproved()
        {
            return Status == DealStatus.Approved || Status == DealStatus.AwaitingClientAccountCreation 
                || Status == DealStatus.AwaitingSalesmanReport || Status == DealStatus.SentToLegal 
                || Status == DealStatus.Success;
        }

        /// <summary>
        /// Checks if deal is rejected
        /// </summary>
        public bool IsRejected()
        {
            return Status == DealStatus.RejectedByManager || Status == DealStatus.RejectedBySuperAdmin || Status == DealStatus.Failed;
        }
        #endregion
    }

    #region Constants
    public static class DealStatus
    {
        public const string PendingManagerApproval = "PendingManagerApproval";
        public const string RejectedByManager = "RejectedByManager";
        public const string PendingSuperAdminApproval = "PendingSuperAdminApproval";
        public const string RejectedBySuperAdmin = "RejectedBySuperAdmin";
        public const string Approved = "Approved";
        public const string AwaitingClientAccountCreation = "AwaitingClientAccountCreation";
        public const string AwaitingSalesmanReport = "AwaitingSalesmanReport";
        public const string SentToLegal = "SentToLegal";
        public const string Failed = "Failed";
        public const string Success = "Success";
        
        public static readonly string[] AllStatuses = { 
            PendingManagerApproval, RejectedByManager, PendingSuperAdminApproval, 
            RejectedBySuperAdmin, Approved, AwaitingClientAccountCreation, 
            AwaitingSalesmanReport, SentToLegal, Failed, Success 
        };
    }

    public static class RejectionReasons
    {
        public const string Money = "Money";
        public const string CashFlow = "CashFlow";
        public const string OtherNeeds = "OtherNeeds";
        
        public static readonly string[] AllReasons = { Money, CashFlow, OtherNeeds };
    }
    #endregion
}



