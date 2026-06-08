namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Overdue,
    Cancelled,
    Refunded
}

public enum InvoiceType
{
    ClassFees,
    PrivateLesson,
    Camp,
    CompetitionEntry,
    CoachPayment,
    Other
}

public class Invoice : TenantEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid? GuardianId { get; set; }
    public Guardian? Guardian { get; set; }
    public Guid? CoachId { get; set; }
    public Coach? Coach { get; set; }
    public InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "GBP";
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? Notes { get; set; }

    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
}

public class InvoiceLineItem : TenantEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}
