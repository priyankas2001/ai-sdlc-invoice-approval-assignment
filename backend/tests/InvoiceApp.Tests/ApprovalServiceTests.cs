using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalServiceTests
{
    private static Invoice InvoiceWith(string id, InvoiceStatus status, decimal amount = 100m) =>
        new() { Id = id, CustomerName = "Test School", Amount = amount, Status = status };

    private static ApprovalService ServiceFor(params Invoice[] invoices) =>
        new(new FakeInvoiceRepository(invoices));

    [Fact]
    public void Submit_MovesDraftToPendingApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Draft);

        var result = ServiceFor(invoice).Submit("INV-1");

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
    }

    [Fact]
    public void Submit_RejectsInvoiceThatIsNotADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Approved));

        Assert.Throws<InvalidOperationException>(() => service.Submit("INV-1"));
    }

    [Fact]
    public void Manager_Approve_AllowsAmountAtThreshold()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50_000m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "manager.one", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("manager.one", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Manager_Approve_RejectsAmountJustAboveThreshold()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50_001m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "manager.one", "Manager"));

        Assert.Contains("50,000", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
        Assert.Null(invoice.DecisionNote);
        Assert.Null(invoice.FirstApprover);
        Assert.Null(invoice.SecondApprover);
    }

    [Fact]
    public void Director_Approve_AllowsAmountAtThreshold()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500_000m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "director.one", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("director.one", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Director_Approve_AllowsAmountBelowManagerLimit()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 10_000m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "director.one", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("director.one", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Manager_Approve_LeavesExistingStateUnchanged_WhenAmountIsAboveLimit()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 50_001m);
        invoice.DecidedBy = "previous.approver";
        invoice.DecidedAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        invoice.DecisionNote = "Prior rejection note";

        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "manager.one", "Manager"));

        Assert.Contains("50,000", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("previous.approver", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Prior rejection note", invoice.DecisionNote);
    }

    [Fact]
    public void Approve_RejectsLegacyApproverRole()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 10_000m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.approver", "Approver"));

        Assert.Contains("Manager and Director", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsLegacyAdminRole()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 10_000m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.admin", "Admin"));

        Assert.Contains("Manager and Director", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsInvoiceThatIsStillADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Draft));

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "manager.one", "Manager"));
    }

    [Fact]
    public void Approve_ThrowsWhenInvoiceDoesNotExist()
    {
        var service = ServiceFor();

        Assert.Throws<KeyNotFoundException>(() => service.Approve("INV-404", "manager.one", "Manager"));
    }

    [Fact]
    public void Reject_RecordsReasonAndApprover()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Reject("INV-1", "demo.approver", "Duplicate invoice");

        Assert.Equal(InvoiceStatus.Rejected, result.Status);
        Assert.Equal("Duplicate invoice", result.DecisionNote);
    }

    [Fact]
    public void Reject_RequiresAReason()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        Assert.Throws<ArgumentException>(() => service.Reject("INV-1", "demo.approver", "  "));
    }

    [Fact]
    public void Manager_Approve_HighValueInvoice_RequiresSecondDistinctDirectorApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m);
        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "manager.one", "Manager"));

        Assert.Contains("50,000", ex.Message);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.FirstApprover);
        Assert.Null(invoice.SecondApprover);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
        Assert.Null(invoice.DecisionNote);
    }

    [Fact]
    public void Director_Approve_HighValueInvoice_LeavesInvoicePendingUntilSecondDirectorApproves()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m);
        var service = ServiceFor(invoice);

        var firstResult = service.Approve("INV-1", "director.one", "Director");

        Assert.Equal(InvoiceStatus.PendingApproval, firstResult.Status);
        Assert.Equal("director.one", firstResult.FirstApprover);
        Assert.Null(firstResult.SecondApprover);
        Assert.Equal("director.one", firstResult.DecidedBy);
        Assert.NotNull(firstResult.DecidedAt);
        Assert.Equal("Awaiting second Director approval.", firstResult.DecisionNote);
    }

    [Fact]
    public void Director_Approve_HighValueInvoice_DoesNotApproveImmediately_AndRequiresSecondDistinctDirector()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m);
        var service = ServiceFor(invoice);

        var firstResult = service.Approve("INV-1", "director.one", "Director");

        Assert.Equal(InvoiceStatus.PendingApproval, firstResult.Status);
        Assert.Equal("director.one", firstResult.FirstApprover);
        Assert.Null(firstResult.SecondApprover);
        Assert.Equal("director.one", firstResult.DecidedBy);
        Assert.Equal("Awaiting second Director approval.", firstResult.DecisionNote);

        var secondResult = service.Approve("INV-1", "director.two", "Director");

        Assert.Equal(InvoiceStatus.Approved, secondResult.Status);
        Assert.Equal("director.one", secondResult.FirstApprover);
        Assert.Equal("director.two", secondResult.SecondApprover);
    }

    [Fact]
    public void Director_Approve_HighValueInvoice_AllowsSecondDistinctDirectorApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m);
        invoice.FirstApprover = "director.one";
        invoice.DecidedBy = "director.one";
        invoice.DecidedAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        invoice.DecisionNote = "Awaiting second Director approval.";

        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "director.two", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("director.one", result.FirstApprover);
        Assert.Equal("director.two", result.SecondApprover);
        Assert.Equal("director.two", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Director_Approve_HighValueInvoice_RejectsSameUserAsSecondApprover()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m);
        invoice.FirstApprover = "director.one";
        invoice.DecidedBy = "director.one";
        invoice.DecidedAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        invoice.DecisionNote = "Awaiting second Director approval.";

        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "director.one", "Director"));

        Assert.Contains("same user", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("director.one", invoice.FirstApprover);
        Assert.Null(invoice.SecondApprover);
        Assert.Equal("director.one", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Awaiting second Director approval.", invoice.DecisionNote);
    }

    [Fact]
    public void Director_Approve_HighValueInvoice_RejectsNonDirectorSecondApprover()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m);
        invoice.FirstApprover = "director.one";
        invoice.DecidedBy = "director.one";
        invoice.DecidedAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        invoice.DecisionNote = "Awaiting second Director approval.";

        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "manager.one", "Manager"));

        Assert.Contains("different Director", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("director.one", invoice.FirstApprover);
        Assert.Null(invoice.SecondApprover);
        Assert.Equal("director.one", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Awaiting second Director approval.", invoice.DecisionNote);
    }

    [Fact]
    public void Director_Approve_HighValueInvoice_LeavesStateUnchanged_WhenSecondApproverIsRejected()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 600_000m);
        invoice.FirstApprover = "director.one";
        invoice.DecidedBy = "director.one";
        invoice.DecidedAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        invoice.DecisionNote = "Awaiting second Director approval.";

        var service = ServiceFor(invoice);

        var ex = Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "director.one", "Director"));

        Assert.Contains("same user", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Equal("director.one", invoice.FirstApprover);
        Assert.Null(invoice.SecondApprover);
        Assert.Equal("director.one", invoice.DecidedBy);
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), invoice.DecidedAt);
        Assert.Equal("Awaiting second Director approval.", invoice.DecisionNote);
    }

    [Fact]
    public void Approve_RejectsBlankApprover_WithoutChangingInvoice()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 10_000m);
        var service = ServiceFor(invoice);

        Assert.Throws<ArgumentException>(
            () => service.Approve("INV-1", " ", "Manager"));

        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
        Assert.Null(invoice.DecisionNote);
    }

    [Fact]
    public void Submit_RejectsNonDraftInvoice_WithoutChangingInvoice()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Approved);
        invoice.DecidedBy = "director.one";
        invoice.DecisionNote = "Already approved";
        var service = ServiceFor(invoice);

        Assert.Throws<InvalidOperationException>(
            () => service.Submit("INV-1"));

        Assert.Equal(InvoiceStatus.Approved, invoice.Status);
        Assert.Equal("director.one", invoice.DecidedBy);
        Assert.Equal("Already approved", invoice.DecisionNote);
    }

    [Fact]
    public void Reject_RejectsNonPendingInvoice_WithoutChangingInvoice()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Approved);
        invoice.DecidedBy = "director.one";
        invoice.DecisionNote = "Already approved";
        var service = ServiceFor(invoice);

        Assert.Throws<InvalidOperationException>(
            () => service.Reject("INV-1", "director.two", "Duplicate"));

        Assert.Equal(InvoiceStatus.Approved, invoice.Status);
        Assert.Equal("director.one", invoice.DecidedBy);
        Assert.Equal("Already approved", invoice.DecisionNote);
    }

    [Fact]
    public void Reject_RequiresReason_WithoutChangingInvoice()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval);
        var service = ServiceFor(invoice);

        Assert.Throws<ArgumentException>(
            () => service.Reject("INV-1", "director.one", " "));

        Assert.Equal(InvoiceStatus.PendingApproval, invoice.Status);
        Assert.Null(invoice.DecidedBy);
        Assert.Null(invoice.DecidedAt);
        Assert.Null(invoice.DecisionNote);
    }

    [Fact]
    public void Director_Approve_HighValueInvoice_RemainsPendingAfterFirstApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval, 500_001m);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "director.one", "Director");

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
        Assert.Equal("director.one", result.FirstApprover);
        Assert.Null(result.SecondApprover);
        Assert.Equal("director.one", result.DecidedBy);
        Assert.Equal("Awaiting second Director approval.", result.DecisionNote);
    }
}
