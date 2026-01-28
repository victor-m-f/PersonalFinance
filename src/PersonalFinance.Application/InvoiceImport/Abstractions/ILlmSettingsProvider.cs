using PersonalFinance.Application.InvoiceImport.Settings;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface ILlmSettingsProvider
{
    LlmProviderOptions GetOptions();
}
