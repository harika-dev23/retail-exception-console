using RetailConsole.Models;

namespace RetailConsole;

public static class Extensions
{
    public static string ToDisplayString(this ExceptionStatus status) => status switch
    {
        ExceptionStatus.Open => "Open",
        ExceptionStatus.InReview => "In Review",
        ExceptionStatus.PendingApproval => "Pending Approval",
        ExceptionStatus.Resolved => "Resolved",
        ExceptionStatus.Escalated => "Escalated",
        ExceptionStatus.Closed => "Closed",
        _ => status.ToString()
    };

    public static string ToCssClass(this ExceptionStatus status) => status switch
    {
        ExceptionStatus.Open => "open",
        ExceptionStatus.InReview => "in-review",
        ExceptionStatus.PendingApproval => "pending",
        ExceptionStatus.Resolved => "resolved",
        ExceptionStatus.Escalated => "escalated",
        ExceptionStatus.Closed => "closed",
        _ => "open"
    };

    public static string ToDisplayString(this ResolutionAction action) => action switch
    {
        ResolutionAction.MarkResolved => "Mark Resolved",
        ResolutionAction.FlagForReview => "Flag for Review",
        ResolutionAction.SendToManager => "Send to Manager",
        ResolutionAction.RequestFollowUp => "Request Follow-Up",
        ResolutionAction.AdjustInventory => "Adjust Inventory",
        ResolutionAction.EscalateToVendor => "Escalate to Vendor",
        _ => action.ToString()
    };

    public static string ToRelativeTime(this DateTime dt)
    {
        var diff = DateTime.Now - dt;
        return diff switch
        {
            { TotalMinutes: < 1 } => "just now",
            { TotalMinutes: < 60 } => $"{(int)diff.TotalMinutes}m ago",
            { TotalHours: < 24 } => $"{(int)diff.TotalHours}h ago",
            { TotalDays: < 7 } => $"{(int)diff.TotalDays}d ago",
            _ => dt.ToString("MMM d")
        };
    }
}
