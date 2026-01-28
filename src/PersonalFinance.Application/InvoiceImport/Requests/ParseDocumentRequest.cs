namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class ParseDocumentRequest
{
    public Guid DocumentId { get; init; }
}
