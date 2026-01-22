using System.Globalization;

namespace PersonalFinance.Ui.Services.Localization;

public interface ILocalizationService
{
    public CultureInfo CurrentCulture { get; }
    public IReadOnlyList<CultureInfo> SupportedCultures { get; }
    public void InitializeFromSettings();
    public void SetCulture(string cultureName);
}
