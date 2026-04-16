using RetailConsole.Models;

namespace RetailConsole.Services;

public class MockExceptionService : IExceptionService
{
    private readonly List<RetailException> _exceptions;

    public MockExceptionService()
    {
        _exceptions = SeedData();
    }

    public Task<List<RetailException>> GetExceptionsAsync()
    {
        return Task.FromResult(_exceptions.ToList());
    }

    public Task<RetailException?> GetExceptionByIdAsync(string id)
    {
        return Task.FromResult(_exceptions.FirstOrDefault(e => e.ExceptionId == id));
    }

    public Task<bool> ResolveExceptionAsync(string id, ResolutionRequest request)
    {
        var exception = _exceptions.FirstOrDefault(e => e.ExceptionId == id);
        if (exception is null) return Task.FromResult(false);

        exception.Status = request.Action switch
        {
            ResolutionAction.MarkResolved => ExceptionStatus.Resolved,
            ResolutionAction.FlagForReview => ExceptionStatus.InReview,
            ResolutionAction.SendToManager => ExceptionStatus.PendingApproval,
            ResolutionAction.RequestFollowUp => ExceptionStatus.InReview,
            ResolutionAction.AdjustInventory => ExceptionStatus.Resolved,
            ResolutionAction.EscalateToVendor => ExceptionStatus.Escalated,
            _ => ExceptionStatus.InReview
        };

        exception.ResolutionReason = request.ReasonCode ?? "";
        exception.Notes = string.IsNullOrWhiteSpace(exception.Notes)
            ? request.Note
            : exception.Notes + "\n\n" + request.Note;
        exception.ResolvedBy = request.ResolvedBy;
        exception.UpdatedAt = DateTime.Now;

        if (exception.Status == ExceptionStatus.Resolved || exception.Status == ExceptionStatus.Closed)
            exception.ResolvedAt = DateTime.Now;

        return Task.FromResult(true);
    }

    public Task<List<RetailException>> SearchAsync(
        string query,
        string? statusFilter,
        string? priorityFilter,
        string? typeFilter,
        string? sortField,
        bool sortDescending)
    {
        var results = _exceptions.AsEnumerable();

        // Text search
        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLowerInvariant();
            results = results.Where(e =>
                e.ExceptionId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.SKU.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.ItemDescription.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.Store.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.ExceptionType.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.Notes.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.VendorName.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        // Status filter
        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ExceptionStatus>(statusFilter, out var status))
            results = results.Where(e => e.Status == status);

        // Priority filter
        if (!string.IsNullOrEmpty(priorityFilter) && Enum.TryParse<ExceptionPriority>(priorityFilter, out var priority))
            results = results.Where(e => e.Priority == priority);

        // Type filter
        if (!string.IsNullOrEmpty(typeFilter))
            results = results.Where(e => e.ExceptionType.Equals(typeFilter, StringComparison.OrdinalIgnoreCase));

        // Sorting
        results = sortField switch
        {
            "Store" => sortDescending ? results.OrderByDescending(e => e.Store) : results.OrderBy(e => e.Store),
            "Priority" => sortDescending ? results.OrderByDescending(e => e.Priority) : results.OrderBy(e => e.Priority),
            "Status" => sortDescending ? results.OrderByDescending(e => e.Status) : results.OrderBy(e => e.Status),
            "ExceptionType" => sortDescending ? results.OrderByDescending(e => e.ExceptionType) : results.OrderBy(e => e.ExceptionType),
            "UpdatedAt" => sortDescending ? results.OrderByDescending(e => e.UpdatedAt) : results.OrderBy(e => e.UpdatedAt),
            _ => results.OrderByDescending(e => e.Priority).ThenByDescending(e => e.UpdatedAt)
        };

        return Task.FromResult(results.ToList());
    }

    private static List<RetailException> SeedData() => new()
    {
        new RetailException
        {
            ExceptionId = "EX-1042",
            Store = "Store 118",
            SKU = "JWL-4471",
            ItemDescription = "Gold-tone bracelet, 7.5in",
            ExceptionType = "Receiving discrepancy",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.High,
            OnHandQty = 2, ExpectedQty = 12, ReceivedQty = 8,
            Price = 19.99m, Cost = 9.50m, MarginPct = 52.5m,
            UpdatedAt = DateTime.Now.AddHours(-3),
            Notes = "Store reported carton shortage. Driver left before count was confirmed.",
            AssignedTo = "Ops Queue",
            VendorName = "Brightfield Accessories",
            Department = "Jewelry",
            AIRecommendation = "Likely carrier shortage — file a freight claim and request POD from the carrier. Vendor credit is probable if shortage is confirmed on the BOL."
        },
        new RetailException
        {
            ExceptionId = "EX-1043",
            Store = "Store 204",
            SKU = "HBC-2210",
            ItemDescription = "Moisturizing hand cream 8oz",
            ExceptionType = "Damaged item / vendor credit",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.Medium,
            OnHandQty = 14, ExpectedQty = 14, ReceivedQty = 14,
            Price = 8.49m, Cost = 3.80m, MarginPct = 55.2m,
            UpdatedAt = DateTime.Now.AddHours(-7),
            Notes = "6 units arrived with broken pump mechanisms. Cannot be sold as-is.",
            AssignedTo = "Vendor Relations",
            VendorName = "LuxeBeauty Inc.",
            Department = "Health & Beauty",
            AIRecommendation = "Request a vendor credit memo for 6 units at cost ($22.80). Document damage with photos before disposing. Check if vendor has a standard return policy for damaged goods."
        },
        new RetailException
        {
            ExceptionId = "EX-1044",
            Store = "Store 033",
            SKU = "APP-8814",
            ItemDescription = "Women's fleece vest, size M",
            ExceptionType = "Low stock / out-of-stock risk",
            Status = ExceptionStatus.InReview,
            Priority = ExceptionPriority.High,
            OnHandQty = 1, ExpectedQty = 15, ReceivedQty = 15,
            Price = 44.99m, Cost = 18.00m, MarginPct = 60.0m,
            UpdatedAt = DateTime.Now.AddDays(-1),
            Notes = "Size M selling rapidly. Store 033 at risk of OOS before weekend.",
            AssignedTo = "Merch Team",
            VendorName = "NorthPeak Apparel",
            Department = "Apparel",
            AIRecommendation = "Strong sell-through rate suggests under-allocation. Check if Store 077 or Store 118 have excess units for a store transfer. Also verify if next PO can be expedited."
        },
        new RetailException
        {
            ExceptionId = "EX-1045",
            Store = "Store 077",
            SKU = "ELC-3302",
            ItemDescription = "Wireless earbuds, sport edition",
            ExceptionType = "Pricing exception",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.Critical,
            OnHandQty = 22, ExpectedQty = 22, ReceivedQty = 22,
            Price = 9.99m, Cost = 28.50m, MarginPct = -185.2m,
            UpdatedAt = DateTime.Now.AddMinutes(-45),
            Notes = "POS showing $9.99 instead of $39.99. Incorrect price file pushed overnight.",
            AssignedTo = "IT / Pricing",
            VendorName = "SoundTech LLC",
            Department = "Electronics",
            AIRecommendation = "This is an active margin bleed — suspend sale immediately and push a corrected price file. Check if other SKUs in last night's upload are affected. Escalate to IT for root cause."
        },
        new RetailException
        {
            ExceptionId = "EX-1046",
            Store = "Store 118",
            SKU = "HMD-5521",
            ItemDescription = "Bamboo cutting board set (3pc)",
            ExceptionType = "Receiving discrepancy",
            Status = ExceptionStatus.Resolved,
            Priority = ExceptionPriority.Low,
            OnHandQty = 24, ExpectedQty = 24, ReceivedQty = 21,
            Price = 29.99m, Cost = 12.00m, MarginPct = 59.9m,
            UpdatedAt = DateTime.Now.AddDays(-2),
            Notes = "Shortage of 3 units. Recount confirmed 24 on hand — discrepancy was a receiving scan error.",
            AssignedTo = "Ops Queue",
            VendorName = "HomeEssentials Co.",
            Department = "Housewares",
            ResolvedBy = "Jamie T.",
            ResolvedAt = DateTime.Now.AddDays(-2),
            ResolutionReason = "Store receiving error – recounted",
            AIRecommendation = "Scan error at receiving dock is consistent with the discrepancy pattern for this SKU. No vendor action needed."
        },
        new RetailException
        {
            ExceptionId = "EX-1047",
            Store = "Store 204",
            SKU = "TOY-9901",
            ItemDescription = "Building blocks set, 200pc",
            ExceptionType = "Damaged item / vendor credit",
            Status = ExceptionStatus.Escalated,
            Priority = ExceptionPriority.High,
            OnHandQty = 0, ExpectedQty = 18, ReceivedQty = 18,
            Price = 34.99m, Cost = 14.75m, MarginPct = 57.8m,
            UpdatedAt = DateTime.Now.AddHours(-12),
            Notes = "All 18 units received with crushed packaging. Cannot be sold. Vendor unresponsive to initial contact.",
            AssignedTo = "Vendor Relations",
            VendorName = "FunBuild Toys",
            Department = "Toys",
            AIRecommendation = "Vendor non-response after 48hrs — escalate to buyer and consider formal dispute. Total exposure is $265.50 at cost. Document all communication timestamps."
        },
        new RetailException
        {
            ExceptionId = "EX-1048",
            Store = "Store 033",
            SKU = "GRO-1144",
            ItemDescription = "Organic oat granola 12oz",
            ExceptionType = "Tax-code mismatch",
            Status = ExceptionStatus.InReview,
            Priority = ExceptionPriority.Medium,
            OnHandQty = 88, ExpectedQty = 88, ReceivedQty = 88,
            Price = 5.99m, Cost = 2.40m, MarginPct = 59.9m,
            UpdatedAt = DateTime.Now.AddDays(-3),
            Notes = "Item ringing taxable at register but should be tax-exempt under state grocery exemption.",
            AssignedTo = "Finance / Tax",
            VendorName = "GoodGrain Foods",
            Department = "Grocery",
            AIRecommendation = "Correct the tax code in POS immediately to avoid customer-facing overcharges. Review register transactions from past 14 days for potential refund exposure."
        },
        new RetailException
        {
            ExceptionId = "EX-1049",
            Store = "Store 077",
            SKU = "BED-6631",
            ItemDescription = "Microfiber sheet set, Queen",
            ExceptionType = "Margin exception",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.Medium,
            OnHandQty = 31, ExpectedQty = 31, ReceivedQty = 31,
            Price = 39.99m, Cost = 22.00m, MarginPct = 44.9m,
            UpdatedAt = DateTime.Now.AddDays(-1),
            Notes = "Vendor invoice shows new cost of $22.00, up from $16.50. Margin now below 45% target.",
            AssignedTo = "Merch Team",
            VendorName = "SleepWell Textiles",
            Department = "Bedding",
            AIRecommendation = "Cost increase of $5.50/unit requires a price adjustment to maintain target margin. Suggested new retail is $46.99. Buyer sign-off needed before price change."
        },
        new RetailException
        {
            ExceptionId = "EX-1050",
            Store = "Store 118",
            SKU = "SPT-2287",
            ItemDescription = "Yoga mat, 6mm non-slip",
            ExceptionType = "Low stock / out-of-stock risk",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.Medium,
            OnHandQty = 3, ExpectedQty = 20, ReceivedQty = 20,
            Price = 27.99m, Cost = 10.50m, MarginPct = 62.5m,
            UpdatedAt = DateTime.Now.AddHours(-5),
            Notes = "Rapid depletion after end-cap feature. Next PO not arriving for 12 days.",
            AssignedTo = "Ops Queue",
            VendorName = "FlexFit Gear",
            Department = "Sporting Goods",
            AIRecommendation = "High sell-through post-feature placement is expected. Place an emergency replenishment order or request a transfer from the DC. Consider moving units from a lower-velocity store."
        },
        new RetailException
        {
            ExceptionId = "EX-1051",
            Store = "Store 204",
            SKU = "ELC-7744",
            ItemDescription = "USB-C fast charger, 65W",
            ExceptionType = "Receiving discrepancy",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.High,
            OnHandQty = 5, ExpectedQty = 24, ReceivedQty = 24,
            Price = 34.99m, Cost = 14.20m, MarginPct = 59.4m,
            UpdatedAt = DateTime.Now.AddHours(-2),
            Notes = "19 units unaccounted for after receiving. Loss prevention has been notified.",
            AssignedTo = "Loss Prevention",
            VendorName = "ChargeRight Electronics",
            Department = "Electronics",
            AIRecommendation = "Discrepancy pattern (19 of 24 missing) suggests possible internal theft or receiving dock diversion. Recommend LP review of receiving dock footage before closing this exception."
        },
        new RetailException
        {
            ExceptionId = "EX-1052",
            Store = "Store 033",
            SKU = "KIT-3388",
            ItemDescription = "Stainless steel knife block set",
            ExceptionType = "Pricing exception",
            Status = ExceptionStatus.PendingApproval,
            Priority = ExceptionPriority.Medium,
            OnHandQty = 9, ExpectedQty = 9, ReceivedQty = 9,
            Price = 89.99m, Cost = 41.00m, MarginPct = 54.4m,
            UpdatedAt = DateTime.Now.AddHours(-6),
            Notes = "Regional manager requested price match to online competitor at $69.99. Awaiting buyer approval.",
            AssignedTo = "Buyer – Housewares",
            VendorName = "ChefPro Cutlery",
            Department = "Housewares",
            AIRecommendation = "Price match to $69.99 reduces margin to ~42%. Within acceptable range for a competitive move. Buyer approval should note this is store-specific and not a chain-wide change."
        },
        new RetailException
        {
            ExceptionId = "EX-1053",
            Store = "Store 077",
            SKU = "PET-5512",
            ItemDescription = "Grain-free dry dog food 15lb",
            ExceptionType = "Tax-code mismatch",
            Status = ExceptionStatus.Resolved,
            Priority = ExceptionPriority.Low,
            OnHandQty = 42, ExpectedQty = 42, ReceivedQty = 42,
            Price = 49.99m, Cost = 24.50m, MarginPct = 51.0m,
            UpdatedAt = DateTime.Now.AddDays(-4),
            Notes = "Tax code corrected after state changed pet food exemption rules Q1.",
            AssignedTo = "Finance / Tax",
            VendorName = "PawsFirst Nutrition",
            Department = "Pet Supplies",
            ResolvedBy = "Dana W.",
            ResolvedAt = DateTime.Now.AddDays(-4),
            ResolutionReason = "State rule change applied",
            AIRecommendation = "Tax code correction applied correctly. No additional action required."
        },
        new RetailException
        {
            ExceptionId = "EX-1054",
            Store = "Store 118",
            SKU = "APP-2241",
            ItemDescription = "Men's slim-fit chinos, 32x30",
            ExceptionType = "Low stock / out-of-stock risk",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.High,
            OnHandQty = 0, ExpectedQty = 12, ReceivedQty = 12,
            Price = 54.99m, Cost = 21.50m, MarginPct = 60.9m,
            UpdatedAt = DateTime.Now.AddHours(-1),
            Notes = "Fully out of stock. This size had highest unit velocity last 30 days.",
            AssignedTo = "Merch Team",
            VendorName = "NorthPeak Apparel",
            Department = "Apparel",
            AIRecommendation = "Size 32x30 is a core basic — immediate replenishment needed. Pull from DC reserve stock or transfer from lower-velocity stores. Flag buyer if next PO allocation is insufficient."
        },
        new RetailException
        {
            ExceptionId = "EX-1055",
            Store = "Store 204",
            SKU = "HBC-8830",
            ItemDescription = "Vitamin C serum 1oz",
            ExceptionType = "Margin exception",
            Status = ExceptionStatus.InReview,
            Priority = ExceptionPriority.Low,
            OnHandQty = 27, ExpectedQty = 27, ReceivedQty = 27,
            Price = 16.99m, Cost = 9.80m, MarginPct = 42.3m,
            UpdatedAt = DateTime.Now.AddDays(-5),
            Notes = "Margin sitting 3 points below category target. No recent cost change.",
            AssignedTo = "Merch Team",
            VendorName = "LuxeBeauty Inc.",
            Department = "Health & Beauty",
            AIRecommendation = "Margin shortfall appears structural, not a data error. Options: raise price to $18.49 (~47% margin), negotiate cost reduction with vendor, or reclassify item to a lower-margin category target."
        },
        new RetailException
        {
            ExceptionId = "EX-1056",
            Store = "Store 033",
            SKU = "HMD-7712",
            ItemDescription = "Cast iron skillet 10in",
            ExceptionType = "Receiving discrepancy",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.Low,
            OnHandQty = 18, ExpectedQty = 16, ReceivedQty = 16,
            Price = 39.99m, Cost = 16.00m, MarginPct = 59.9m,
            UpdatedAt = DateTime.Now.AddDays(-2),
            Notes = "Store received 2 extra units beyond PO quantity. Possible vendor over-ship.",
            AssignedTo = "Ops Queue",
            VendorName = "HomeEssentials Co.",
            Department = "Housewares",
            AIRecommendation = "Vendor over-shipment of 2 units — confirm before selling. Review invoice against PO. If vendor agrees to honor, update receiving record. If not, units must be held for return."
        },
        new RetailException
        {
            ExceptionId = "EX-1057",
            Store = "Store 077",
            SKU = "SPT-4490",
            ItemDescription = "Resistance band set, 5pc",
            ExceptionType = "Damaged item / vendor credit",
            Status = ExceptionStatus.Open,
            Priority = ExceptionPriority.Medium,
            OnHandQty = 8, ExpectedQty = 24, ReceivedQty = 24,
            Price = 21.99m, Cost = 8.75m, MarginPct = 60.2m,
            UpdatedAt = DateTime.Now.AddHours(-9),
            Notes = "16 units received with torn packaging and band breakage. Vendor contacted.",
            AssignedTo = "Vendor Relations",
            VendorName = "FlexFit Gear",
            Department = "Sporting Goods",
            AIRecommendation = "Request credit for 16 damaged units ($140.00 at cost). Ask vendor to confirm shipping method — band breakage suggests transit compression issue. Consider requesting improved packaging."
        },
        new RetailException
        {
            ExceptionId = "EX-1058",
            Store = "Store 118",
            SKU = "GRO-5566",
            ItemDescription = "Cold brew coffee concentrate 32oz",
            ExceptionType = "Pricing exception",
            Status = ExceptionStatus.Resolved,
            Priority = ExceptionPriority.Low,
            OnHandQty = 33, ExpectedQty = 33, ReceivedQty = 33,
            Price = 12.99m, Cost = 5.20m, MarginPct = 60.0m,
            UpdatedAt = DateTime.Now.AddDays(-3),
            Notes = "End-cap promo ended 3 days ago but sale price was still active. Corrected in price file.",
            AssignedTo = "IT / Pricing",
            VendorName = "GoodGrain Foods",
            Department = "Grocery",
            ResolvedBy = "Sam K.",
            ResolvedAt = DateTime.Now.AddDays(-3),
            ResolutionReason = "Error in price upload – corrected",
            AIRecommendation = "Promotional price override not removed on schedule — review price file management process to prevent recurrence."
        },
        new RetailException
        {
            ExceptionId = "EX-1059",
            Store = "Store 204",
            SKU = "TOY-3321",
            ItemDescription = "Foam pool noodles (4pk)",
            ExceptionType = "Low stock / out-of-stock risk",
            Status = ExceptionStatus.Closed,
            Priority = ExceptionPriority.Low,
            OnHandQty = 0, ExpectedQty = 30, ReceivedQty = 30,
            Price = 9.99m, Cost = 3.50m, MarginPct = 64.9m,
            UpdatedAt = DateTime.Now.AddDays(-7),
            Notes = "Seasonal item sold through at end of summer. No reorder needed.",
            AssignedTo = "Merch Team",
            VendorName = "FunBuild Toys",
            Department = "Seasonal",
            ResolvedBy = "Taylor R.",
            ResolvedAt = DateTime.Now.AddDays(-7),
            ResolutionReason = "Item discontinued – no reorder",
            AIRecommendation = "Seasonal sell-through complete. Archive item and update planogram for next season."
        }
    };
}
