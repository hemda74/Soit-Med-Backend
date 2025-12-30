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
        public string SalesManId { get; set; } = string.Empty;
        
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

        #region SalesMan Report
        [MaxLength(5000)]
        public string? ReportText { get; set; }
        
        [MaxLength(2000)]
        public string? ReportAttachments { get; set; } // JSON array of file paths/URLs
        
        public DateTime? ReportSubmittedAt { get; set; }
        #endregion

        #region Salesmen Reviews (for Legal Team)
        // Second salesman ID (from offer's AssignedTo if different from primary)
        public string? SecondSalesManId { get; set; }
        
        // First salesman review
        [MaxLength(5000)]
        public string? FirstSalesManReview { get; set; }
        
        public DateTime? FirstSalesManReviewSubmittedAt { get; set; }
        
        // Second salesman review
        [MaxLength(5000)]
        public string? SecondSalesManReview { get; set; }
        
        public DateTime? SecondSalesManReviewSubmittedAt { get; set; }
        #endregion

        #region Client Account Credentials
        [MaxLength(200)]
        public string? ClientUsername { get; set; }
        
        [MaxLength(500)] // Encrypted password
        public string? ClientPassword { get; set; }
        
        public DateTime? ClientCredentialsSetAt { get; set; }
        
        public string? ClientCredentialsSetBy { get; set; }
        #endregion

        #region Legal and Completion
        public DateTime? SentToLegalAt { get; set; }
        public DateTime? ReturnedToSalesmanAt { get; set; }
        public DateTime? LegalReviewedAt { get; set; }
        [MaxLength(1000)]
        public string? LegalReturnNotes { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        [MaxLength(2000)]
        public string? CompletionNotes { get; set; }
        #endregion

        #region Navigation Properties
        public virtual SalesOffer? Offer { get; set; }
        public virtual Client Client { get; set; } = null!;
        public virtual ApplicationUser SalesMan { get; set; } = null!;
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
            Status = DealStatus.AwaitingSalesmenReviewsAndAccountSetup;
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
            Status = DealStatus.AwaitingSalesManReport;
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
        /// Return deal to salesman for new report
        /// </summary>
        public void ReturnToSalesmanForNewReport(string? returnNotes = null)
        {
            if (Status != DealStatus.SentToLegal)
                throw new InvalidOperationException($"Deal must be in SentToLegal status to return to salesman. Current status: {Status}");

            Status = DealStatus.ReturnedToSalesman;
            ReturnedToSalesmanAt = DateTime.UtcNow;
            LegalReturnNotes = returnNotes;
            // Clear previous report to allow new submission
            ReportText = null;
            ReportAttachments = null;
            ReportSubmittedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark deal as reviewed by legal (archive/mark as read)
        /// </summary>
        public void MarkAsLegalReviewed()
        {
            if (Status != DealStatus.SentToLegal)
                throw new InvalidOperationException($"Deal must be in SentToLegal status to mark as reviewed. Current status: {Status}");

            Status = DealStatus.LegalReviewed;
            LegalReviewedAt = DateTime.UtcNow;
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
        /// Checks if deal needs super Admin approval
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
                || Status == DealStatus.AwaitingSalesManReport || Status == DealStatus.AwaitingSalesmenReviewsAndAccountSetup
                || Status == DealStatus.SentToLegal || Status == DealStatus.Success;
        }

        /// <summary>
        /// Checks if deal is rejected
        /// </summary>
        public bool IsRejected()
        {
            return Status == DealStatus.RejectedByManager || Status == DealStatus.RejectedBySuperAdmin || Status == DealStatus.Failed;
        }

        /// <summary>
        /// Submit first salesman review
        /// </summary>
        public void SubmitFirstSalesManReview(string reviewText)
        {
            if (Status != DealStatus.AwaitingSalesmenReviewsAndAccountSetup)
                throw new InvalidOperationException($"Deal is not awaiting salesmen reviews. Current status: {Status}");

            FirstSalesManReview = reviewText;
            FirstSalesManReviewSubmittedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Submit second salesman review
        /// </summary>
        public void SubmitSecondSalesManReview(string reviewText)
        {
            if (Status != DealStatus.AwaitingSalesmenReviewsAndAccountSetup)
                throw new InvalidOperationException($"Deal is not awaiting salesmen reviews. Current status: {Status}");

            SecondSalesManReview = reviewText;
            SecondSalesManReviewSubmittedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Set client username and password (by Admin)
        /// </summary>
        public void SetClientCredentials(string username, string encryptedPassword, string adminId)
        {
            if (Status != DealStatus.AwaitingSalesmenReviewsAndAccountSetup)
                throw new InvalidOperationException($"Deal is not awaiting account setup. Current status: {Status}");

            ClientUsername = username;
            ClientPassword = encryptedPassword;
            ClientCredentialsSetBy = adminId;
            ClientCredentialsSetAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Check if all salesmen reviews and credentials are ready, then send to legal
        /// </summary>
        public bool CheckAndSendToLegalIfReady()
        {
            if (Status != DealStatus.AwaitingSalesmenReviewsAndAccountSetup)
                return false;

            // Check if both reviews are submitted
            bool firstReviewReady = !string.IsNullOrWhiteSpace(FirstSalesManReview);
            bool secondReviewReady = !string.IsNullOrWhiteSpace(SecondSalesManReview) || SecondSalesManId == null;
            
            // Check if credentials are set
            bool credentialsReady = !string.IsNullOrWhiteSpace(ClientUsername) && !string.IsNullOrWhiteSpace(ClientPassword);

            // If all conditions are met, send to legal
            if (firstReviewReady && secondReviewReady && credentialsReady)
            {
                Status = DealStatus.SentToLegal;
                SentToLegalAt = DateTime.UtcNow;
                UpdatedAt = DateTime.UtcNow;
                return true;
            }

            return false;
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
        public const string AwaitingSalesManReport = "AwaitingSalesManReport";
        public const string AwaitingSalesmenReviewsAndAccountSetup = "AwaitingSalesmenReviewsAndAccountSetup";
        public const string SentToLegal = "SentToLegal";
        public const string ReturnedToSalesman = "ReturnedToSalesman";
        public const string LegalReviewed = "LegalReviewed";
        public const string Failed = "Failed";
        public const string Success = "Success";
        
        public static readonly string[] AllStatuses = { 
            PendingManagerApproval, RejectedByManager, PendingSuperAdminApproval, 
            RejectedBySuperAdmin, Approved, AwaitingClientAccountCreation, 
            AwaitingSalesManReport, AwaitingSalesmenReviewsAndAccountSetup, SentToLegal, 
            ReturnedToSalesman, LegalReviewed, Failed, Success 
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



