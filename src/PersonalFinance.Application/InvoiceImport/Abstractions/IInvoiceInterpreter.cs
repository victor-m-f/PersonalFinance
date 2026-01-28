using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface IInvoiceInterpreter
{
    public Task<Result<InvoiceInterpretationResult>> InterpretAsync(
        InterpretInvoiceRequest request,
        CancellationToken ct);
}
