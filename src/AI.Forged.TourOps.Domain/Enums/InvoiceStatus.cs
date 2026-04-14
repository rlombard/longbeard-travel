namespace AI.Forged.TourOps.Domain.Enums;

public enum InvoiceStatus
{
    Draft = 1,
    Received = 2,
    Matched = 3,
    Unmatched = 4,
    PendingReview = 5,
    Approved = 6,
    Rejected = 7,
    Unpaid = 8,
    PartiallyPaid = 9,
    Paid = 10,
    Overdue = 11,
    RebatePending = 12,
    RebateApplied = 13,
    Cancelled = 14
}
