namespace InvoiceApp.Domain;

using System.Globalization;

/// <summary>
/// Moves an invoice through its lifecycle: Draft -> PendingApproval -> Approved or Rejected.
/// One invoice at a time, decided as a whole by a single approver.
/// </summary>
public class ApprovalService
{
    private const decimal ManagerLimit = 50_000m;
    private const decimal DirectorLimit = 500_000m;

    private readonly IInvoiceRepository _repository;

    public ApprovalService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public Invoice Submit(string invoiceId)
    {
        var invoice = Find(invoiceId);

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Invoice {invoiceId} is {invoice.Status}; only drafts can be submitted.");
        }

        invoice.Status = InvoiceStatus.PendingApproval;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Approve(string invoiceId, string approver, string role)
    {
        if (string.IsNullOrWhiteSpace(approver))
        {
            throw new ArgumentException("Approver is required.", nameof(approver));
        }

        var invoice = Find(invoiceId);
        EnsurePending(invoice);
        ValidateApprovalRoleAndAmount(role, invoice.Amount);

        invoice.Status = InvoiceStatus.Approved;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Reject(string invoiceId, string approver, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required to reject an invoice.", nameof(reason));
        }

        var invoice = Find(invoiceId);
        EnsurePending(invoice);

        invoice.Status = InvoiceStatus.Rejected;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.DecisionNote = reason;
        _repository.Update(invoice);
        return invoice;
    }

    private Invoice Find(string invoiceId) =>
        _repository.GetById(invoiceId)
        ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");

    private static void EnsurePending(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                $"Invoice {invoice.Id} is {invoice.Status}; only pending invoices can be decided.");
        }
    }

    private static void ValidateApprovalRoleAndAmount(string role, decimal amount)
    {
        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (amount > ManagerLimit)
            {
                throw new InvalidOperationException(
                    $"Managers may approve invoices up to {ManagerLimit.ToString("N0", CultureInfo.InvariantCulture)}. Invoice amount is {amount.ToString("N0", CultureInfo.InvariantCulture)}.");
            }

            return;
        }

        if (string.Equals(role, "Director", StringComparison.OrdinalIgnoreCase))
        {
            if (amount > DirectorLimit)
            {
                throw new InvalidOperationException(
                    $"Directors may approve invoices up to {DirectorLimit.ToString("N0", CultureInfo.InvariantCulture)}. Invoice amount is {amount.ToString("N0", CultureInfo.InvariantCulture)}.");
            }

            return;
        }

        throw new InvalidOperationException(
            "Approval is limited to Manager and Director roles.");
    }
}
