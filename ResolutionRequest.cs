using System.ComponentModel.DataAnnotations;

namespace RetailConsole.Models;

public enum ResolutionAction
{
    MarkResolved,
    FlagForReview,
    SendToManager,
    RequestFollowUp,
    AdjustInventory,
    EscalateToVendor
}

public class ResolutionRequest
{
    [Required(ErrorMessage = "Please select an action.")]
    public ResolutionAction? Action { get; set; }

    [Required(ErrorMessage = "Please select a reason code.")]
    public string? ReasonCode { get; set; }

    [Required(ErrorMessage = "A note is required before submitting.")]
    [MinLength(10, ErrorMessage = "Note must be at least 10 characters.")]
    public string Note { get; set; } = "";

    public string ResolvedBy { get; set; } = "";
}

public static class ReasonCodes
{
    public static readonly Dictionary<string, List<string>> ByExceptionType = new()
    {
        ["Receiving discrepancy"] = new()
        {
            "Carrier shortage – vendor will credit",
            "Counted in transit – resolved",
            "Store receiving error – recounted",
            "Vendor short-shipped – claim filed",
            "Item mis-scanned at receiving"
        },
        ["Damaged item / vendor credit"] = new()
        {
            "Vendor credit memo received",
            "Item returned to vendor",
            "Item marked unsellable – disposed",
            "Store absorbed loss",
            "Claim pending with vendor"
        },
        ["Low stock / out-of-stock risk"] = new()
        {
            "Replenishment order placed",
            "Transfer requested from nearby store",
            "Vendor lead time confirmed – no action",
            "Item discontinued – no reorder",
            "Safety stock adjusted"
        },
        ["Pricing exception"] = new()
        {
            "Promotional price – correct as marked",
            "Price corrected in system",
            "Competitor match approved",
            "Error in price upload – corrected",
            "Clearance markdown applied"
        },
        ["Margin exception"] = new()
        {
            "Cost increase from vendor – accepted",
            "Temporary promotional margin",
            "Buyer approved reduced margin",
            "Price correction applied",
            "Shrink driving apparent margin drop"
        },
        ["Tax-code mismatch"] = new()
        {
            "Tax code corrected in POS",
            "Exempt certificate on file – correct",
            "Audit finding – corrected prospectively",
            "State rule change applied",
            "Item reclassified"
        }
    };

    public static List<string> GetForType(string exceptionType)
    {
        foreach (var key in ByExceptionType.Keys)
        {
            if (exceptionType.Contains(key, StringComparison.OrdinalIgnoreCase))
                return ByExceptionType[key];
        }
        return new List<string>
        {
            "Investigated – no action required",
            "Corrected in system",
            "Escalated for further review",
            "Vendor notified",
            "Other – see note"
        };
    }
}
