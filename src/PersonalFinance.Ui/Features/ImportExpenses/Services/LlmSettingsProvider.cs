using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Settings;

namespace PersonalFinance.Ui.Features.ImportExpenses.Services;

public sealed class LlmSettingsProvider : ILlmSettingsProvider
{
    public LlmProviderOptions GetOptions()
    {
        return new LlmProviderOptions
        {
            Provider = "local",
            TimeoutSeconds = 240
        };
    }
}
