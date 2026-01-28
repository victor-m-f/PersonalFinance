namespace PersonalFinance.Domain.Entities;

public enum ImportedDocumentStatus
{
    Uploaded = 0,
    Parsed = 1,
    Confirmed = 2,
    Failed = 3
}
