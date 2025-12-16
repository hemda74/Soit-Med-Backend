<!-- acd45b7d-638c-4cba-ad8a-aab1b6f101e5 f36b69d1-01e8-4876-9c0a-eae6c8621089 -->

# خطة تنفيذ وحدة المبيعات الكاملة | Complete Sales Module Implementation Plan

## نظرة عامة | Overview

هذه الخطة تغطي تنفيذ وحدة مبيعات شاملة تشمل:

- إدارة العملاء مع تاريخ كامل
- هيكل جديد: WeeklyPlan → Tasks → Progress
- العروض والصفقات مع نظام موافقات متعدد المستويات

This plan covers implementing a comprehensive sales module including:

- Client management with complete history
- New structure: WeeklyPlan → Tasks → Progress
- Offers and deals with multi-level approval workflow

---

## Phase 1: إنشاء Models الأساسية | Create Core Models

### 1.1 Client Model (نموذج العميل)

**الملف | File:** `SoitMed/Models/Client.cs` (New file)

```csharp
public class Client : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; }

    [Required, MaxLength(50)]
    public string Type { get; set; } // Hospital, Clinic, Lab, Pharmacy

    [MaxLength(100)]
    public string? Specialization { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Governorate { get; set; }

    [MaxLength(200)]
    public string? ContactPerson { get; set; }

    [MaxLength(20)]
    public string? ContactPersonPhone { get; set; }

    [MaxLength(100)]
    public string? ContactPersonEmail { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Inactive, Potential

    [Required, MaxLength(50)]
    public string Priority { get; set; } = "Normal"; // VIP, High, Normal, Low

    // Client Classification
    [MaxLength(1)]
    public string? Classification { get; set; } // A, B, C, D

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PotentialValue { get; set; }

    public DateTime? LastContactDate { get; set; }
    public DateTime? NextContactDate { get; set; }
    public int? SatisfactionRating { get; set; }

    [Required]
    public string CreatedBy { get; set; }

    public string? AssignedTo { get; set; }

    // Navigation Properties - Complete History
    public virtual ICollection<TaskProgress> TaskProgresses { get; set; } // All visits/interactions
    public virtual ICollection<Offer> Offers { get; set; } // All offers
    public virtual ICollection<Deal> Deals { get; set; } // All deals (success/failed)
}
```

### 1.2 WeeklyPlan Model (نموذج الخطة الأسبوعية)

**الملف | File:** `SoitMed/Models/WeeklyPlan.cs` (Update existing)

```csharp
public class WeeklyPlan : BaseEntity
{
    [Required]
    public string EmployeeId { get; set; }

    [Required]
    public DateTime WeekStartDate { get; set; }

    [Required]
    public DateTime WeekEndDate { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    // Manager Review Fields
    public int? Rating { get; set; } // 1-5

    [MaxLength(1000)]
    public string? ManagerComment { get; set; }

    public DateTime? ManagerReviewedAt { get; set; }

    public string? ReviewedBy { get; set; }

    // Navigation Properties
    public virtual ApplicationUser Employee { get; set; }
    public virtual ApplicationUser? Reviewer { get; set; }
    public virtual ICollection<WeeklyPlanTask> Tasks { get; set; } // NEW: Tasks instead of Items
}
```

### 1.3 WeeklyPlanTask Model (المهمة في الخطة) - NEW

**ملف جديد | New File:** `SoitMed/Models/WeeklyPlanTask.cs`

```csharp
public class WeeklyPlanTask : BaseEntity
{
    [Required]
    public long WeeklyPlanId { get; set; }

    // ========== Task Type ==========
    [Required, MaxLength(50)]
    public string TaskType { get; set; } = "Visit"; // Visit, FollowUp

    // ========== Client Information ==========
    public long? ClientId { get; set; } // NULL for new clients

    [MaxLength(20)]
    public string? ClientStatus { get; set; } // "Old", "New"

    // For NEW clients - basic info
    [MaxLength(200)]
    public string? ClientName { get; set; }

    [MaxLength(200)]
    public string? PlaceName { get; set; }

    [MaxLength(50)]
    public string? PlaceType { get; set; } // Hospital, Clinic, Lab

    [MaxLength(20)]
    public string? ClientPhone { get; set; }

    [MaxLength(500)]
    public string? ClientAddress { get; set; }

    [MaxLength(100)]
    public string? ClientLocation { get; set; }

    // ========== Client Classification ==========
    [MaxLength(1)]
    public string? ClientClassification { get; set; } // A, B, C, D

    // ========== Task Planning ==========
    [Required]
    public DateTime PlannedDate { get; set; }

    [MaxLength(20)]
    public string? PlannedTime { get; set; }

    [MaxLength(500)]
    public string? Purpose { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string Priority { get; set; } = "Medium"; // High, Medium, Low

    [MaxLength(50)]
    public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled

    // Navigation Properties
    public virtual WeeklyPlan WeeklyPlan { get; set; }
    public virtual Client? Client { get; set; } // Link to existing client if Old
    public virtual ICollection<TaskProgress> Progresses { get; set; } // Multiple progress updates
}

// ========== Constants ==========
public static class TaskTypeConstants
{
    public const string Visit = "Visit";
    public const string FollowUp = "FollowUp";

    public static readonly string[] AllTypes = { Visit, FollowUp };
}
```

### 1.4 TaskProgress Model (تقدم المهمة) - NEW

**ملف جديد | New File:** `SoitMed/Models/TaskProgress.cs`

```csharp
public class TaskProgress : BaseEntity
{
    [Required]
    public long TaskId { get; set; } // Link to WeeklyPlanTask

    public long? ClientId { get; set; } // Link to Client for history tracking

    [Required]
    public string EmployeeId { get; set; } // Who made this progress

    // ========== Visit/Progress Details ==========
    [Required]
    public DateTime ProgressDate { get; set; }

    [MaxLength(50)]
    public string ProgressType { get; set; } = "Visit"; // Visit, Call, Meeting, Email

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // ========== Visit Result ==========
    [MaxLength(20)]
    public string? VisitResult { get; set; } // "Interested", "NotInterested"

    // ========== If NOT Interested ==========
    [MaxLength(2000)]
    public string? NotInterestedComment { get; set; } // Why not interested

    // ========== If Interested - Next Step ==========
    [MaxLength(20)]
    public string? NextStep { get; set; } // "NeedsDeal", "NeedsOffer"

    // Links to created requests/offers/deals
    public long? OfferRequestId { get; set; }
    public long? OfferId { get; set; }
    public long? DealId { get; set; }

    // ========== Follow-up ==========
    public DateTime? NextFollowUpDate { get; set; }

    [MaxLength(1000)]
    public string? FollowUpNotes { get; set; }

    // ========== Satisfaction ==========
    public int? SatisfactionRating { get; set; } // 1-5

    [MaxLength(2000)]
    public string? Feedback { get; set; }

    // ========== Attachments ==========
    [MaxLength(2000)]
    public string? Attachments { get; set; } // JSON array of file paths

    // Navigation Properties
    public virtual WeeklyPlanTask Task { get; set; }
    public virtual Client? Client { get; set; }
    public virtual ApplicationUser Employee { get; set; }
    public virtual OfferRequest? OfferRequest { get; set; }
    public virtual Offer? Offer { get; set; }
    public virtual Deal? Deal { get; set; }
}

// ========== Constants ==========
public static class ProgressTypeConstants
{
    public const string Visit = "Visit";
    public const string Call = "Call";
    public const string Meeting = "Meeting";
    public const string Email = "Email";

    public static readonly string[] AllTypes = { Visit, Call, Meeting, Email };
}

public static class VisitResultConstants
{
    public const string Interested = "Interested";
    public const string NotInterested = "NotInterested";

    public static readonly string[] AllResults = { Interested, NotInterested };
}

public static class NextStepConstants
{
    public const string NeedsDeal = "NeedsDeal";
    public const string NeedsOffer = "NeedsOffer";

    public static readonly string[] AllSteps = { NeedsDeal, NeedsOffer };
}
```

### 1.5 OfferRequest Model (نموذج طلب العرض)

**ملف جديد | New File:** `SoitMed/Models/OfferRequest.cs`

```csharp
public class OfferRequest : BaseEntity
{
    [Required]
    public string RequestedBy { get; set; } // SalesMan ID

    [Required]
    public long ClientId { get; set; }

    public long? TaskProgressId { get; set; } // Link to the progress that triggered this request

    [Required, MaxLength(2000)]
    public string RequestedProducts { get; set; } // JSON or comma-separated

    [MaxLength(2000)]
    public string? SpecialNotes { get; set; }

    [Required]
    public DateTime RequestDate { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Requested"; // Requested, InProgress, Ready, Sent, Cancelled

    public string? AssignedTo { get; set; } // Sales Support ID

    public long? CreatedOfferId { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(1000)]
    public string? CompletionNotes { get; set; }

    // Navigation Properties
    public virtual ApplicationUser Requester { get; set; }
    public virtual Client Client { get; set; }
    public virtual TaskProgress? TaskProgress { get; set; }
    public virtual ApplicationUser? AssignedSupportUser { get; set; }
    public virtual Offer? CreatedOffer { get; set; }
}
```

### 1.6 Offer Model (نموذج العرض)

**ملف جديد | New File:** `SoitMed/Models/Offer.cs`

```csharp
public class Offer : BaseEntity
{
    public long? OfferRequestId { get; set; }

    [Required]
    public long ClientId { get; set; }

    [Required]
    public string CreatedBy { get; set; } // Sales Support ID

    [Required]
    public string AssignedTo { get; set; } // SalesMan ID

    [Required, MaxLength(2000)]
    public string Products { get; set; } // JSON or detailed list

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(2000)]
    public string? PaymentTerms { get; set; }

    [MaxLength(2000)]
    public string? DeliveryTerms { get; set; }

    [Required]
    public DateTime ValidUntil { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Sent, UnderReview, Accepted, Rejected, NeedsModification, Expired

    public DateTime? SentToClientAt { get; set; }

    [MaxLength(2000)]
    public string? ClientResponse { get; set; }

    [MaxLength(2000)]
    public string? Documents { get; set; } // JSON array of file paths

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual OfferRequest? OfferRequest { get; set; }
    public virtual Client Client { get; set; }
    public virtual ApplicationUser Creator { get; set; }
    public virtual ApplicationUser SalesMan { get; set; }
    public virtual Deal? Deal { get; set; }
}
```

### 1.7 Deal Model (نموذج الصفقة)

**ملف جديد | New File:** `SoitMed/Models/Deal.cs`

```csharp
public class Deal : BaseEntity
{
    [Required]
    public long OfferId { get; set; }

    [Required]
    public long ClientId { get; set; }

    [Required]
    public string SalesManId { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal DealValue { get; set; }

    [Required]
    public DateTime ClosedDate { get; set; }

    [MaxLength(2000)]
    public string? PaymentTerms { get; set; }

    [MaxLength(2000)]
    public string? DeliveryTerms { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "PendingManagerApproval";
    // PendingManagerApproval, RejectedByManager, PendingSuperAdminApproval,
    // RejectedBySuperAdmin, Approved, SentToLegal, Failed, Success

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    // Manager Approval
    public string? ManagerApprovedBy { get; set; }
    public DateTime? ManagerApprovedAt { get; set; }

    [MaxLength(50)]
    public string? ManagerRejectionReason { get; set; } // Money, CashFlow, OtherNeeds

    [MaxLength(2000)]
    public string? ManagerComments { get; set; }

    // SuperAdmin Approval
    public string? SuperAdminApprovedBy { get; set; }
    public DateTime? SuperAdminApprovedAt { get; set; }

    [MaxLength(50)]
    public string? SuperAdminRejectionReason { get; set; } // Money, CashFlow, OtherNeeds

    [MaxLength(2000)]
    public string? SuperAdminComments { get; set; }

    public DateTime? SentToLegalAt { get; set; }

    // Deal Outcome
    public DateTime? CompletedAt { get; set; }

    [MaxLength(2000)]
    public string? CompletionNotes { get; set; }

    // Navigation Properties
    public virtual Offer Offer { get; set; }
    public virtual Client Client { get; set; }
    public virtual ApplicationUser SalesMan { get; set; }
    public virtual ApplicationUser? ManagerApprover { get; set; }
    public virtual ApplicationUser? SuperAdminApprover { get; set; }
}
```

---

## Phase 2: تحديث Context وإضافة Relationships | Update Context and Add Relationships

**الملف | File:** `SoitMed/Models/Context.cs`

إضافة DbSets الجديدة | Add new DbSets:

```csharp
public DbSet<Client> Clients { get; set; }
public DbSet<WeeklyPlanTask> WeeklyPlanTasks { get; set; } // NEW
public DbSet<TaskProgress> TaskProgresses { get; set; } // NEW
public DbSet<OfferRequest> OfferRequests { get; set; }
public DbSet<Offer> Offers { get; set; }
public DbSet<Deal> Deals { get; set; }

// Remove or rename old one if exists
// public DbSet<WeeklyPlanItem> WeeklyPlanItems { get; set; } // OLD - to be replaced
```

إضافة Relationships في OnModelCreating | Add relationships in OnModelCreating:

```csharp
// WeeklyPlan relationships
modelBuilder.Entity<WeeklyPlan>()
    .HasOne(wp => wp.Employee)
    .WithMany()
    .HasForeignKey(wp => wp.EmployeeId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<WeeklyPlan>()
    .HasOne(wp => wp.Reviewer)
    .WithMany()
    .HasForeignKey(wp => wp.ReviewedBy)
    .OnDelete(DeleteBehavior.SetNull);

// WeeklyPlanTask relationships
modelBuilder.Entity<WeeklyPlanTask>()
    .HasOne(wpt => wpt.WeeklyPlan)
    .WithMany(wp => wp.Tasks)
    .HasForeignKey(wpt => wpt.WeeklyPlanId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<WeeklyPlanTask>()
    .HasOne(wpt => wpt.Client)
    .WithMany()
    .HasForeignKey(wpt => wpt.ClientId)
    .OnDelete(DeleteBehavior.SetNull);

// TaskProgress relationships
modelBuilder.Entity<TaskProgress>()
    .HasOne(tp => tp.Task)
    .WithMany(wpt => wpt.Progresses)
    .HasForeignKey(tp => tp.TaskId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<TaskProgress>()
    .HasOne(tp => tp.Client)
    .WithMany(c => c.TaskProgresses)
    .HasForeignKey(tp => tp.ClientId)
    .OnDelete(DeleteBehavior.SetNull);

modelBuilder.Entity<TaskProgress>()
    .HasOne(tp => tp.Employee)
    .WithMany()
    .HasForeignKey(tp => tp.EmployeeId)
    .OnDelete(DeleteBehavior.Restrict);

// OfferRequest relationships
modelBuilder.Entity<OfferRequest>()
    .HasOne(or => or.Requester)
    .WithMany()
    .HasForeignKey(or => or.RequestedBy)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<OfferRequest>()
    .HasOne(or => or.Client)
    .WithMany()
    .HasForeignKey(or => or.ClientId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<OfferRequest>()
    .HasOne(or => or.TaskProgress)
    .WithMany()
    .HasForeignKey(or => or.TaskProgressId)
    .OnDelete(DeleteBehavior.SetNull);

// Offer relationships
modelBuilder.Entity<Offer>()
    .HasOne(o => o.OfferRequest)
    .WithOne(or => or.CreatedOffer)
    .HasForeignKey<Offer>(o => o.OfferRequestId)
    .OnDelete(DeleteBehavior.SetNull);

modelBuilder.Entity<Offer>()
    .HasOne(o => o.Client)
    .WithMany(c => c.Offers)
    .HasForeignKey(o => o.ClientId)
    .OnDelete(DeleteBehavior.Restrict);

// Deal relationships
modelBuilder.Entity<Deal>()
    .HasOne(d => d.Offer)
    .WithOne(o => o.Deal)
    .HasForeignKey<Deal>(d => d.OfferId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Deal>()
    .HasOne(d => d.Client)
    .WithMany(c => c.Deals)
    .HasForeignKey(d => d.ClientId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Deal>()
    .HasOne(d => d.SalesMan)
    .WithMany()
    .HasForeignKey(d => d.SalesManId)
    .OnDelete(DeleteBehavior.Restrict);

// Indexes for performance
modelBuilder.Entity<Deal>()
    .HasIndex(d => new { d.Status, d.SalesManId });

modelBuilder.Entity<Offer>()
    .HasIndex(o => new { o.Status, o.AssignedTo });

modelBuilder.Entity<OfferRequest>()
    .HasIndex(or => new { or.Status, or.RequestedBy });

modelBuilder.Entity<TaskProgress>()
    .HasIndex(tp => new { tp.ClientId, tp.ProgressDate });

modelBuilder.Entity<WeeklyPlanTask>()
    .HasIndex(wpt => new { wpt.WeeklyPlanId, wpt.Status });
```

---

## Phase 3: إنشاء DTOs | Create DTOs

**ملف جديد | New File:** `SoitMed/DTO/SalesModuleDTOs.cs`

```csharp
// ==================== Client DTOs ====================
public class CreateClientDTO
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string? Specialization { get; set; }
    public string? Location { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPersonPhone { get; set; }
    public string? ContactPersonEmail { get; set; }
    public string? Notes { get; set; }
    public string Priority { get; set; }
    public string? Classification { get; set; } // A, B, C, D
}

public class ClientResponseDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string? Specialization { get; set; }
    public string? Location { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public string? Classification { get; set; }
    public DateTime? LastContactDate { get; set; }
    public DateTime? NextContactDate { get; set; }
    public int? SatisfactionRating { get; set; }
    public DateTime CreatedAt { get; set; }
}

// NEW - Client Profile with Complete History
public class ClientProfileDTO
{
    public ClientResponseDTO ClientInfo { get; set; }
    public List<TaskProgressSummaryDTO> AllVisits { get; set; } // All visits/interactions
    public List<OfferSummaryDTO> AllOffers { get; set; } // All offers
    public List<DealSummaryDTO> AllDeals { get; set; } // All deals (success/failed)
    public ClientStatisticsDTO Statistics { get; set; }
}

public class ClientStatisticsDTO
{
    public int TotalVisits { get; set; }
    public int TotalOffers { get; set; }
    public int SuccessfulDeals { get; set; }
    public int FailedDeals { get; set; }
    public decimal? TotalRevenue { get; set; }
    public double? AverageSatisfaction { get; set; }
}

// ==================== WeeklyPlan DTOs ====================
public class CreateWeeklyPlanDTO
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public List<CreateWeeklyPlanTaskDTO> Tasks { get; set; }
}

public class CreateWeeklyPlanTaskDTO
{
    public string TaskType { get; set; } // Visit, FollowUp
    public long? ClientId { get; set; } // NULL for new client
    public string? ClientStatus { get; set; } // "Old", "New"

    // For new clients
    public string? ClientName { get; set; }
    public string? PlaceName { get; set; }
    public string? PlaceType { get; set; }
    public string? ClientPhone { get; set; }
    public string? ClientAddress { get; set; }
    public string? ClientLocation { get; set; }

    public string? ClientClassification { get; set; } // A, B, C, D
    public DateTime PlannedDate { get; set; }
    public string? PlannedTime { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
    public string Priority { get; set; }
}

public class WeeklyPlanResponseDTO
{
    public long Id { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int? Rating { get; set; }
    public string? ManagerComment { get; set; }
    public DateTime? ManagerReviewedAt { get; set; }
    public List<WeeklyPlanTaskResponseDTO> Tasks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WeeklyPlanTaskResponseDTO
{
    public long Id { get; set; }
    public string TaskType { get; set; }
    public long? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientStatus { get; set; }
    public string? ClientClassification { get; set; }
    public DateTime PlannedDate { get; set; }
    public string? PlannedTime { get; set; }
    public string? Purpose { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public int ProgressCount { get; set; } // Number of progress updates
    public List<TaskProgressResponseDTO>? Progresses { get; set; }
}

public class ReviewWeeklyPlanDTO
{
    public int? Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

// ==================== TaskProgress DTOs ====================
public class CreateTaskProgressDTO
{
    public long TaskId { get; set; }
    public DateTime ProgressDate { get; set; }
    public string ProgressType { get; set; } // Visit, Call, Meeting, Email
    public string? Description { get; set; }
    public string? Notes { get; set; }

    // Visit Result
    public string? VisitResult { get; set; } // Interested, NotInterested
    public string? NotInterestedComment { get; set; }

    // If Interested
    public string? NextStep { get; set; } // NeedsDeal, NeedsOffer

    // Follow-up
    public DateTime? NextFollowUpDate { get; set; }
    public string? FollowUpNotes { get; set; }

    // Satisfaction
    public int? SatisfactionRating { get; set; }
    public string? Feedback { get; set; }
}

public class TaskProgressResponseDTO
{
    public long Id { get; set; }
    public long TaskId { get; set; }
    public long? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public DateTime ProgressDate { get; set; }
    public string ProgressType { get; set; }
    public string? Description { get; set; }
    public string? VisitResult { get; set; }
    public string? NotInterestedComment { get; set; }
    public string? NextStep { get; set; }
    public long? OfferRequestId { get; set; }
    public long? OfferId { get; set; }
    public long? DealId { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public int? SatisfactionRating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TaskProgressSummaryDTO
{
    public long Id { get; set; }
    public DateTime ProgressDate { get; set; }
    public string ProgressType { get; set; }
    public string? VisitResult { get; set; }
    public string? NextStep { get; set; }
    public int? SatisfactionRating { get; set; }
}

// ==================== OfferRequest DTOs ====================
public class CreateOfferRequestDTO
{
    public long ClientId { get; set; }
    public long? TaskProgressId { get; set; }
    public string RequestedProducts { get; set; }
    public string? SpecialNotes { get; set; }
}

public class OfferRequestResponseDTO
{
    public long Id { get; set; }
    public string RequestedBy { get; set; }
    public string RequestedByName { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public string RequestedProducts { get; set; }
    public string? SpecialNotes { get; set; }
    public DateTime RequestDate { get; set; }
    public string Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public long? CreatedOfferId { get; set; }
}

// ==================== Offer DTOs ====================
public class CreateOfferDTO
{
    public long OfferRequestId { get; set; }
    public long ClientId { get; set; }
    public string AssignedTo { get; set; }
    public string Products { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public DateTime ValidUntil { get; set; }
    public string? Notes { get; set; }
}

public class OfferResponseDTO
{
    public long Id { get; set; }
    public long? OfferRequestId { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public string CreatedBy { get; set; }
    public string CreatedByName { get; set; }
    public string AssignedTo { get; set; }
    public string AssignedToName { get; set; }
    public string Products { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Status { get; set; }
    public DateTime? SentToClientAt { get; set; }
    public string? ClientResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OfferSummaryDTO
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public DateTime ValidUntil { get; set; }
}

// ==================== Deal DTOs ====================
public class CreateDealDTO
{
    public long OfferId { get; set; }
    public long ClientId { get; set; }
    public decimal DealValue { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}

public class DealResponseDTO
{
    public long Id { get; set; }
    public long OfferId { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public string SalesManId { get; set; }
    public string SalesManName { get; set; }
    public decimal DealValue { get; set; }
    public DateTime ClosedDate { get; set; }
    public string Status { get; set; }
    public string? ManagerRejectionReason { get; set; }
    public string? ManagerComments { get; set; }
    public string? SuperAdminRejectionReason { get; set; }
    public string? SuperAdminComments { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DealSummaryDTO
{
    public long Id { get; set; }
    public DateTime ClosedDate { get; set; }
    public decimal DealValue { get; set; }
    public string Status { get; set; } // Success, Failed, Pending, etc.
}

public class ApproveDealDTO
{
    public bool Approved { get; set; }
    public string? RejectionReason { get; set; } // Money, CashFlow, OtherNeeds
    public string? Comments { get; set; }
}
```

---

## Phase 4: إنشاء Repositories | Create Repositories

الملفات في: `SoitMed/Repositories/`

### 4.1 ClientRepository.cs

- GetClientWithHistoryAsync(long clientId) - Returns client with all visits, offers, deals
- GetClientsByAssignedToAsync(string salesmanId)
- GetClientsByClassificationAsync(string classification)

### 4.2 WeeklyPlanTaskRepository.cs (NEW)

- GetTasksByWeeklyPlanIdAsync(long weeklyPlanId)
- GetTasksByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)
- GetOverdueTasksAsync(string employeeId)

### 4.3 TaskProgressRepository.cs (NEW)

- GetProgressesByTaskIdAsync(long taskId)
- GetProgressesByClientIdAsync(long clientId) - For client history
- GetProgressesByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)

### 4.4 OfferRequestRepository.cs

- GetRequestsByStatusAsync(string status)
- GetRequestsBySalesManAsync(string salesmanId)
- GetRequestsAssignedToAsync(string supportId)

### 4.5 OfferRepository.cs

- GetOffersByClientIdAsync(long clientId) - For client history
- GetOffersBySalesManAsync(string salesmanId)
- GetOffersByStatusAsync(string status)

### 4.6 DealRepository.cs

- GetDealsByClientIdAsync(long clientId) - For client history
- GetDealsBySalesManAsync(string salesmanId)
- GetDealsByStatusAsync(string status)
- GetPendingApprovalsForManagerAsync()
- GetPendingApprovalsForSuperAdminAsync()

---

## Phase 5: إنشاء Services | Create Services

الملفات في: `SoitMed/Services/`

### 5.1 ClientService.cs

- **GetClientProfileAsync(long clientId)** - Returns complete client history
     - All visits/interactions
     - All offers (pending, accepted, rejected)
     - All deals (success/failed)
     - Statistics

### 5.2 WeeklyPlanService.cs

- CreateWeeklyPlanWithTasksAsync()
- UpdateWeeklyPlanTaskAsync()
- GetWeeklyPlanWithTasksAndProgressesAsync()

### 5.3 TaskProgressService.cs (NEW)

- **CreateProgressAsync()** - Create new progress for a task
- **CreateProgressAndOfferRequestAsync()** - When salesman selects "NeedsOffer"
- GetProgressesByTaskAsync()
- GetProgressesByClientAsync() - For client history

### 5.4 OfferRequestService.cs

- CreateOfferRequestFromProgressAsync()
- AssignToSupportAsync()
- UpdateStatusAsync()

### 5.5 OfferService.cs

- CreateOfferFromRequestAsync()
- SendToSalesManAsync()
- GetOffersByClientAsync() - For client history

### 5.6 DealService.cs

- CreateDealAsync()
- **ManagerApprovalAsync()** - Manager approve/reject with reason
- **SuperAdminApprovalAsync()** - CEO approve/reject with reason
- **SendToLegalAsync()** - Automatic after final approval
- GetDealsByClientAsync() - For client history

---

## Phase 6: إنشاء Controllers | Create Controllers

الملفات في: `SoitMed/Controllers/`

### 6.1 ClientController.cs

- GET /api/clients
- GET /api/clients/{id}
- **GET /api/clients/{id}/profile** - Complete history (NEW)
- POST /api/clients
- PUT /api/clients/{id}
- DELETE /api/clients/{id}

### 6.2 WeeklyPlanController.cs

- POST /api/weekly-plans
- PUT /api/weekly-plans/{id}
- GET /api/weekly-plans
- GET /api/weekly-plans/{id}
- POST /api/weekly-plans/{id}/review (Manager only)

### 6.3 WeeklyPlanTaskController.cs (NEW)

- POST /api/weekly-plan-tasks
- PUT /api/weekly-plan-tasks/{id}
- GET /api/weekly-plan-tasks/{id}
- DELETE /api/weekly-plan-tasks/{id}

### 6.4 TaskProgressController.cs (NEW)

- **POST /api/task-progress** - Create progress
- **POST /api/task-progress/with-offer-request** - Create progress + trigger offer request
- GET /api/task-progress/{taskId}
- GET /api/task-progress/by-client/{clientId}
- PUT /api/task-progress/{id}

### 6.5 OfferRequestController.cs

- POST /api/offer-requests
- GET /api/offer-requests
- PUT /api/offer-requests/{id}/assign
- PUT /api/offer-requests/{id}/status

### 6.6 OfferController.cs

- POST /api/offers
- PUT /api/offers/{id}
- GET /api/offers
- GET /api/offers/by-client/{clientId}
- PUT /api/offers/{id}/send-to-salesman
- PUT /api/offers/{id}/client-response

### 6.7 DealController.cs

- POST /api/deals
- GET /api/deals
- GET /api/deals/{id}
- GET /api/deals/by-client/{clientId}
- **POST /api/deals/{id}/manager-approval**
- **POST /api/deals/{id}/superAdmin-approval**

---

## Phase 7: إضافة Authorization | Add Authorization

تحديث: `SoitMed/Common/AuthorizationHelper.cs`

```csharp
public static class SalesRoles
{
    public const string SalesMan = "SalesMan";
    public const string SalesManager = "SalesManager";
    public const string SalesSupport = "SalesSupport";
    public const string SuperAdmin = "SuperAdmin";
}

public static class SalesPermissions
{
    // Client permissions
    public const string ViewOwnClients = "ViewOwnClients";
    public const string ViewAllClients = "ViewAllClients";
    public const string ViewClientHistory = "ViewClientHistory";
    public const string CreateClient = "CreateClient";
    public const string EditClient = "EditClient";

    // Weekly Plan permissions
    public const string CreateWeeklyPlan = "CreateWeeklyPlan";
    public const string ViewOwnWeeklyPlans = "ViewOwnWeeklyPlans";
    public const string ViewAllWeeklyPlans = "ViewAllWeeklyPlans";
    public const string ReviewWeeklyPlan = "ReviewWeeklyPlan";

    // Task Progress permissions
    public const string CreateTaskProgress = "CreateTaskProgress";
    public const string ViewOwnProgress = "ViewOwnProgress";
    public const string ViewAllProgress = "ViewAllProgress";

    // Offer permissions
    public const string RequestOffer = "RequestOffer";
    public const string CreateOffer = "CreateOffer";
    public const string ViewOwnOffers = "ViewOwnOffers";
    public const string ViewAllOffers = "ViewAllOffers";

    // Deal permissions
    public const string CreateDeal = "CreateDeal";
    public const string ViewOwnDeals = "ViewOwnDeals";
    public const string ViewAllDeals = "ViewAllDeals";
    public const string ApproveManagerDeal = "ApproveManagerDeal";
    public const string ApproveSuperAdminDeal = "ApproveSuperAdminDeal";
}
```

---

## Phase 8: إنشاء Migration وتحديث Database | Create Migration and Update Database

```bash
cd SoitMed
dotnet ef migrations add AddCompleteSalesModuleWithTaskProgress
dotnet ef database update
```

**ملاحظة مهمة:** إذا كان هناك `WeeklyPlanItem` موجود، قد نحتاج إلى:

1. Migration لإعادة تسمية الجدول أو
2. Migration لنقل البيانات القديمة أو
3. إنشاء جداول جديدة وحذف القديم

---

## Phase 9: إضافة Notifications | Add Notifications

تحديث: `SoitMed/Services/NotificationService.cs`

إضافة notifications للأحداث التالية | Add notifications for:

- عرض جديد جاهز للمندوب
- طلب عرض جديد لدعم المبيعات
- تقدم جديد تم إضافته لمهمة
- صفقة تحتاج موافقة المدير
- صفقة تحتاج موافقة SuperAdmin
- صفقة مرفوضة (للمندوب مع الأسباب)
- صفقة موافق عليها نهائياً
- تقييم جديد على الخطة الأسبوعية
- تذكير بمواعيد المتابعة

---

## Phase 10: Testing والتوثيق | Testing and Documentation

### Testing Checklist:

**Weekly Plan & Tasks:**

- [ ] إنشاء خطة أسبوعية مع مهام متعددة
- [ ] إضافة مهمة لعميل قديم (Old Client)
- [ ] إضافة مهمة لعميل جديد (New Client)
- [ ] تقييم المدير للخطة

**Task Progress:**

- [ ] إضافة تقدم لمهمة
- [ ] إضافة تقدمات متعددة لنفس المهمة
- [ ] نتيجة "Not Interested" مع تعليق
- [ ] نتيجة "Interested" + "Needs Offer"
- [ ] نتيجة "Interested" + "Needs Deal"

**Client History:**

- [ ] عرض ملف عميل قديم مع جميع الزيارات
- [ ] عرض جميع العروض المقدمة للعميل
- [ ] عرض جميع الصفقات (Success/Failed)
- [ ] إحصائيات العميل

**Offer Workflow:**

- [ ] طلب عرض من المندوب
- [ ] إنشاء عرض من الدعم
- [ ] إرسال العرض للمندوب
- [ ] تقديم العرض للعميل

**Deal Approval Workflow:**

- [ ] إنشاء صفقة من المندوب
- [ ] موافقة/رفض مدير المبيعات (مع الأسباب)
- [ ] موافقة/رفض SuperAdmin (مع الأسباب)
- [ ] إرسال للفريق القانوني

**Notifications:**

- [ ] جميع الإشعارات تعمل بشكل صحيح

---

## الملفات الرئيسية المطلوبة | Key Files Required:

### Models (8 ملفات):

- Client.cs (NEW)
- WeeklyPlan.cs (UPDATE)
- WeeklyPlanTask.cs (NEW)
- TaskProgress.cs (NEW)
- OfferRequest.cs (NEW)
- Offer.cs (NEW)
- Deal.cs (NEW)
- ~~WeeklyPlanItem.cs~~ (OLD - to be replaced/removed)

### DTOs (1 ملف):

- SalesModuleDTOs.cs (NEW)

### Repositories (6 ملفات):

- ClientRepository.cs
- WeeklyPlanTaskRepository.cs (NEW)
- TaskProgressRepository.cs (NEW)
- OfferRequestRepository.cs
- OfferRepository.cs
- DealRepository.cs

### Services (6 ملفات):

- ClientService.cs
- WeeklyPlanService.cs (UPDATE)
- TaskProgressService.cs (NEW)
- OfferRequestService.cs
- OfferService.cs
- DealService.cs

### Controllers (7 ملفات):

- ClientController.cs
- WeeklyPlanController.cs (UPDATE)
- WeeklyPlanTaskController.cs (NEW)
- TaskProgressController.cs (NEW)
- OfferRequestController.cs
- OfferController.cs
- DealController.cs

---

## ملاحظات مهمة | Important Notes:

### البنية الجديدة:

1. **WeeklyPlan → Tasks → Progress** هيكل جديد تماماً
2. **Client History** يظهر كل التفاعلات التاريخية
3. **Multiple Progresses per Task** يسمح بتحديثات متعددة لنفس المهمة

### Migration Strategy:

1. إنشاء الجداول الجديدة (WeeklyPlanTask, TaskProgress)
2. إذا كان هناك بيانات في WeeklyPlanItem القديم:

      - نقل البيانات للجداول الجديدة
      - أو الاحتفاظ بالجدول القديم للتاريخ فقط

3. تحديث الـ relationships

### Authorization:

- التحقق من الصلاحيات في كل endpoint
- المندوب يرى عملائه فقط
- المدير يرى كل شيء (للقراءة فقط)
- SuperAdmin يوافق/يرفض الصفقات

### Performance:

- Indexes على الأعمدة المستخدمة في البحث
- Lazy loading للبيانات التاريخية
- Pagination في عرض القوائم

---

هل تريد البدء في التنفيذ؟ | Ready to start implementation?

### To-dos

- [ ] Create all 7 models (Client, WeeklyPlan update, WeeklyPlanItem, ClientVisit, OfferRequest, Offer, Deal) with proper relationships and validations
- [ ] Update Context.cs with new DbSets and configure all relationships in OnModelCreating
- [ ] Create SalesModuleDTOs.cs with all request/response DTOs for the sales module
- [ ] Create 5 repository classes (Client, WeeklyPlan, OfferRequest, Offer, Deal) with CRUD and custom queries
- [ ] Create 5 service classes implementing business logic for each entity with proper authorization checks
- [ ] Create 5 API controllers (Client, WeeklyPlan, OfferRequest, Offer, Deal) with all endpoints and authorization
- [ ] Update AuthorizationHelper.cs with sales roles and permissions constants
- [ ] Create and run migration AddCompleteSalesModule to update database schema
- [ ] Update NotificationService to send notifications for all sales workflow events
- [ ] Test all endpoints, verify workflows, and create API documentation
