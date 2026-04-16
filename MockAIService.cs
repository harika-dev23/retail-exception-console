using RetailConsole.Models;

namespace RetailConsole.Services;

public class MockAIService : IAIService
{
    // Simulated latency so the UX reflects an async call
    private static readonly Random _rng = new();

    public async Task<string> GetRecommendationAsync(RetailException exception)
    {
        await Task.Delay(_rng.Next(300, 800));

        // Return pre-seeded AI recommendation if available
        if (!string.IsNullOrWhiteSpace(exception.AIRecommendation))
            return exception.AIRecommendation;

        // Fallback: generate a contextual recommendation based on exception metadata
        return GenerateContextualRecommendation(exception);
    }

    public async Task<string> GenerateDraftNoteAsync(RetailException exception, ResolutionAction action)
    {
        // Simulated AI thinking time (0.8â€“1.5s to feel realistic)
        await Task.Delay(_rng.Next(800, 1500));

        var actionLabel = action switch
        {
            ResolutionAction.MarkResolved => "resolved",
            ResolutionAction.FlagForReview => "flagged for review",
            ResolutionAction.SendToManager => "escalated to manager",
            ResolutionAction.RequestFollowUp => "flagged for follow-up",
            ResolutionAction.AdjustInventory => "adjusted in inventory",
            ResolutionAction.EscalateToVendor => "escalated to vendor",
            _ => "actioned"
        };

        return exception.ExceptionType switch
        {
            var t when t.Contains("Receiving", StringComparison.OrdinalIgnoreCase) =>
                $"Reviewed receiving discrepancy for {exception.SKU} â€“ {exception.ItemDescription} at {exception.Store}. " +
                $"Expected {exception.ExpectedQty} units; {exception.ReceivedQty} received, {exception.OnHandQty} confirmed on hand. " +
                $"Investigated carrier documentation and store receiving records. Exception has been {actionLabel}.",

            var t when t.Contains("Damaged", StringComparison.OrdinalIgnoreCase) =>
                $"Reviewed damage claim for {exception.SKU} â€“ {exception.ItemDescription} ({exception.VendorName}) at {exception.Store}. " +
                $"Damage assessed and documented. Vendor notified. Exception has been {actionLabel}.",

            var t when t.Contains("stock", StringComparison.OrdinalIgnoreCase) =>
                $"Reviewed stock risk for {exception.SKU} â€“ {exception.ItemDescription} at {exception.Store}. " +
                $"Current on-hand: {exception.OnHandQty} units. " +
                $"Replenishment options evaluated. Exception has been {actionLabel}.",

            var t when t.Contains("Pricing", StringComparison.OrdinalIgnoreCase) =>
                $"Reviewed pricing exception for {exception.SKU} â€“ {exception.ItemDescription} at {exception.Store}. " +
                $"Current price: {exception.Price:C}. Price file and system records reviewed. Exception has been {actionLabel}.",

            var t when t.Contains("Margin", StringComparison.OrdinalIgnoreCase) =>
                $"Reviewed margin exception for {exception.SKU} â€“ {exception.ItemDescription}. " +
                $"Current margin: {exception.MarginPct:F1}% (target: 50%+). " +
                $"Cost and pricing evaluated. Exception has been {actionLabel}.",

            var t when t.Contains("Tax", StringComparison.OrdinalIgnoreCase) =>
                $"Reviewed tax-code mismatch for {exception.SKU} â€“ {exception.ItemDescription} at {exception.Store}. " +
                $"POS tax classification verified against state guidelines. Exception has been {actionLabel}.",

            _ =>
                $"Reviewed exception {exception.ExceptionId} for {exception.ItemDescription} at {exception.Store}. " +
                $"Investigated root cause and took appropriate action. Exception has been {actionLabel}."
        };
    }

    private static string GenerateContextualRecommendation(RetailException e)
    {
        return e.ExceptionType switch
        {
            var t when t.Contains("Receiving", StringComparison.OrdinalIgnoreCase) =>
                $"Shortage of {e.ExpectedQty - e.ReceivedQty} units detected. Verify carrier BOL and request signed POD. If shortage confirmed, file a freight claim or request vendor credit.",

            var t when t.Contains("Damaged", StringComparison.OrdinalIgnoreCase) =>
                "Document all damaged units with photos before any disposal. Contact vendor for RMA or credit memo.",

            var t when t.Contains("stock", StringComparison.OrdinalIgnoreCase) =>
                e.OnHandQty == 0
                    ? "Item is fully out of stock. Initiate emergency replenishment or store transfer immediately."
                    : "Stock is critically low. Place a replenishment order or coordinate a store-to-store transfer.",

            var t when t.Contains("Pricing", StringComparison.OrdinalIgnoreCase) =>
                e.MarginPct < 0
                    ? "Negative margin â€” this price is below cost. Suspend selling and correct the price file immediately."
                    : "Review price file for upload errors. Compare against current vendor invoice.",

            var t when t.Contains("Margin", StringComparison.OrdinalIgnoreCase) =>
                "Compare most recent vendor invoice against item cost in the system. If cost increased, buyer approval is needed before a price change.",

            var t when t.Contains("Tax", StringComparison.OrdinalIgnoreCase) =>
                "Verify item tax classification against the current state tax matrix. Correct POS code and check for retroactive customer overcharges.",

            _ => "Review exception details and coordinate with the appropriate team to determine next steps."
        };
    }
}

