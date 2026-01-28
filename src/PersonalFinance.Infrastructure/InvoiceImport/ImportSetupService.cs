using Microsoft.Extensions.Logging;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Responses;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class ImportSetupService : IImportSetupService
{
    private const string TessdataKey = "tesseract";
    private const string LlmKey = "llm";
    private const string TessdataBaseUrl = "https://github.com/tesseract-ocr/tessdata_fast/raw/main/";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ImportSetupService> _logger;
    private readonly string _tessDataPath;
    private readonly IOcrLanguageProvider _ocrLanguageProvider;
    private readonly LocalLlmModelStore _modelStore = new();

    public ImportSetupService(
        HttpClient httpClient,
        IOcrLanguageProvider ocrLanguageProvider,
        ILogger<ImportSetupService> logger)
    {
        _httpClient = httpClient;
        _ocrLanguageProvider = ocrLanguageProvider;
        _logger = logger;
        _tessDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PersonalFinance",
            "tessdata");
    }

    public async Task<IReadOnlyList<ImportSetupItemStatus>> GetStatusAsync(CancellationToken ct)
    {
        var items = new List<ImportSetupItemStatus>
        {
            await GetTesseractStatusAsync(ct),
            await GetLlmStatusAsync(ct)
        };

        return items;
    }

    public async Task DownloadAsync(string itemKey, IProgress<ImportSetupProgress> progress, CancellationToken ct)
    {
        if (string.Equals(itemKey, TessdataKey, StringComparison.OrdinalIgnoreCase))
        {
            await DownloadTesseractAsync(progress, ct);
            return;
        }

        if (string.Equals(itemKey, LlmKey, StringComparison.OrdinalIgnoreCase))
        {
            await DownloadLlmModelAsync(progress, ct);
            return;
        }

        throw new InvalidOperationException("Unknown setup item.");
    }

    private Task<ImportSetupItemStatus> GetTesseractStatusAsync(CancellationToken ct)
    {
        var languageCode = _ocrLanguageProvider.GetLanguageCode();
        var filePath = Path.Combine(_tessDataPath, GetTessdataFileName(languageCode));
        var installed = File.Exists(filePath);
        return Task.FromResult(new ImportSetupItemStatus
        {
            Key = TessdataKey,
            Title = "Tesseract OCR",
            LanguageCode = languageCode,
            IsInstalled = installed,
            IsRequired = true,
            Detail = installed ? "Installed" : "DownloadRequired",
            ActionText = installed ? "Installed" : "Download"
        });
    }

    private Task<ImportSetupItemStatus> GetLlmStatusAsync(CancellationToken ct)
    {
        var available = File.Exists(_modelStore.ModelPath);
        return Task.FromResult(new ImportSetupItemStatus
        {
            Key = LlmKey,
            Title = "LLM model",
            ModelName = LocalLlmModelStore.ModelDisplayName,
            IsInstalled = available,
            IsRequired = true,
            Detail = available ? "Installed" : "DownloadRequired",
            ActionText = available ? "Installed" : "Download"
        });
    }

    private async Task DownloadTesseractAsync(IProgress<ImportSetupProgress> progress, CancellationToken ct)
    {
        var languageCode = _ocrLanguageProvider.GetLanguageCode();
        Directory.CreateDirectory(_tessDataPath);
        var targetFile = Path.Combine(_tessDataPath, GetTessdataFileName(languageCode));
        progress.Report(new ImportSetupProgress { Key = TessdataKey, Percent = 0, Message = "Starting" });

        var tessdataUrl = new Uri(new Uri(TessdataBaseUrl), GetTessdataFileName(languageCode));
        using var response = await _httpClient.GetAsync(tessdataUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        var total = response.Content.Headers.ContentLength ?? -1;
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None);
        var buffer = new byte[8192];
        long readTotal = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), ct);
            readTotal += read;
            if (total > 0)
            {
                var percent = (int)Math.Round(readTotal * 100d / total);
                progress.Report(new ImportSetupProgress { Key = TessdataKey, Percent = percent, Message = "Downloading" });
            }
        }

        progress.Report(new ImportSetupProgress { Key = TessdataKey, Percent = 100, Message = "Completed" });
    }

    private async Task DownloadLlmModelAsync(IProgress<ImportSetupProgress> progress, CancellationToken ct)
    {
        Directory.CreateDirectory(_modelStore.ModelsFolder);
        var targetFile = _modelStore.ModelPath;
        progress.Report(new ImportSetupProgress { Key = LlmKey, Percent = 0, Message = "Starting" });

        HttpResponseMessage? response = null;
        foreach (var url in LocalLlmModelStore.ModelDownloadUrls)
        {
            response?.Dispose();
            response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.IsSuccessStatusCode)
            {
                break;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                continue;
            }

            response.EnsureSuccessStatusCode();
        }

        if (response is null || !response.IsSuccessStatusCode)
        {
            response?.Dispose();
            throw new HttpRequestException("Failed to download LLM model (all sources returned 404).", null, System.Net.HttpStatusCode.NotFound);
        }

        var total = response.Content.Headers.ContentLength ?? -1;
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None);
        var buffer = new byte[8192];
        long readTotal = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), ct);
            readTotal += read;
            if (total > 0)
            {
                var percent = (int)Math.Round(readTotal * 100d / total);
                progress.Report(new ImportSetupProgress { Key = LlmKey, Percent = percent, Message = "Downloading" });
            }
        }

        progress.Report(new ImportSetupProgress { Key = LlmKey, Percent = 100, Message = "Completed" });
    }

    private static string GetTessdataFileName(string languageCode)
    {
        var normalized = string.IsNullOrWhiteSpace(languageCode) ? "eng" : languageCode.Trim().ToLowerInvariant();
        return $"{normalized}.traineddata";
    }
}
