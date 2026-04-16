namespace RetailConsole.Models;

public enum ExceptionStatus
{
    Open,
    InReview,
    PendingApproval,
    Resolved,
    Escalated,
    Closed
}

public enum ExceptionPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class RetailException
{
    public string ExceptionId { get; set; } = "";
    public string Store { get; set; } = "";
    public string SKU { get; set; } = "";
    public string ItemDescription { get; set; } = "";
    public string ExceptionType { get; set; } = "";
    public string Message { get; set; } = "";
    public ExceptionStatus Status { get; set; }
    public ExceptionPriority Priority { get; set; }
    public int? OnHandQty { get; set; }
    public int? ExpectedQty { get; set; }
    public int? ReceivedQty { get; set; }
    public decimal? Price { get; set; }
    public decimal? Cost { get; set; }
    public decimal? MarginPct { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Notes { get; set; } = "";
    public string AssignedTo { get; set; } = "";
    public string ResolutionReason { get; set; } = "";
    public string AIRecommendation { get; set; } = "";
    public DateTime? ResolvedAt { get; set; }
    public string ResolvedBy { get; set; } = "";
    public string Department { get; set; } = "";
    public string VendorName { get; set; } = "";
}
