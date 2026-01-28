using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Shared.Results;
using PdfiumViewer;
using Tesseract;
using PdfiumDocument = PdfiumViewer.PdfDocument;

namespace PersonalFinance.Infrastructure.Documents;

public sealed class DocumentTextExtractor : IDocumentTextExtractor
{
    private readonly ILogger<DocumentTextExtractor> _logger;
    private readonly string _tessDataPath;
    private readonly string _languageCode;

    public DocumentTextExtractor(
        ILogger<DocumentTextExtractor> logger,
        IOcrLanguageProvider languageProvider)
    {
        _logger = logger;
        _tessDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PersonalFinance",
            "tessdata");
        _languageCode = languageProvider.GetLanguageCode();
    }

    public async Task<Result<DocumentTextExtractionResponse>> ExtractAsync(
        DocumentTextExtractionRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FileExtension))
        {
            return Result<DocumentTextExtractionResponse>.Failure("ValidationError", "File extension is required.");
        }

        var extension = request.FileExtension.Trim('.').ToLowerInvariant();
        if (extension is "pdf")
        {
            return await Task.Run(() => ExtractFromPdfAsync(request.Content, ct), ct);
        }

        if (extension is "png" or "jpg" or "jpeg")
        {
            return await Task.Run(() => ExtractFromImageAsync(request.Content, ct), ct);
        }

        return Result<DocumentTextExtractionResponse>.Failure("Unsupported", "Unsupported file type.");
    }

    private async Task<Result<DocumentTextExtractionResponse>> ExtractFromPdfAsync(Stream content, CancellationToken ct)
    {
        if (!content.CanSeek)
        {
            var buffered = new MemoryStream();
            await content.CopyToAsync(buffered, ct);
            buffered.Position = 0;
            content = buffered;
        }

        if (!HasOcrData())
        {
            return Result<DocumentTextExtractionResponse>.Failure("OcrNotConfigured", "OCR data not found.");
        }

        var ocrResult = ExtractPdfWithOcr(content);
        if (!ocrResult.IsSuccess)
        {
            return ocrResult;
        }

        return Result<DocumentTextExtractionResponse>.Success(ocrResult.Value!);
    }

    private Result<DocumentTextExtractionResponse> ExtractPdfWithOcr(Stream content)
    {
        if (!HasOcrData())
        {
            return Result<DocumentTextExtractionResponse>.Failure("OcrNotConfigured", "OCR data not found.");
        }

        var pageTexts = new List<string>();
        try
        {
            content.Position = 0;
            using var pdf = PdfiumDocument.Load(content);
            using var engine = new TesseractEngine(_tessDataPath, _languageCode, EngineMode.Default);
            for (var i = 0; i < pdf.PageCount; i++)
            {
                using var bitmap = pdf.Render(i, 300, 300, PdfRenderFlags.Annotations);
                using var memory = new MemoryStream();
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                var bytes = memory.ToArray();
                using var pix = Pix.LoadFromMemory(bytes);
                using var page = engine.Process(pix);
                pageTexts.Add(page.GetText() ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to OCR PDF.");
            return Result<DocumentTextExtractionResponse>.Failure("OcrFailed", "OCR failed for PDF.");
        }

        var rawText = string.Join(Environment.NewLine, pageTexts);
        return Result<DocumentTextExtractionResponse>.Success(new DocumentTextExtractionResponse
        {
            RawText = rawText,
            PageTexts = pageTexts,
            IsOcrUsed = true
        });
    }

    private async Task<Result<DocumentTextExtractionResponse>> ExtractFromImageAsync(Stream content, CancellationToken ct)
    {
        if (!HasOcrData())
        {
            return Result<DocumentTextExtractionResponse>.Failure("OcrNotConfigured", "OCR data not found.");
        }

        try
        {
            using var engine = new TesseractEngine(_tessDataPath, _languageCode, EngineMode.Default);
            using var memory = new MemoryStream();
            await content.CopyToAsync(memory, ct);
            var bytes = memory.ToArray();
            using var pix = Pix.LoadFromMemory(bytes);
            using var page = engine.Process(pix);
            var text = page.GetText() ?? string.Empty;
            return Result<DocumentTextExtractionResponse>.Success(new DocumentTextExtractionResponse
            {
                RawText = text,
                PageTexts = new[] { text },
                IsOcrUsed = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to OCR image.");
            return Result<DocumentTextExtractionResponse>.Failure("OcrFailed", "OCR failed for image.");
        }
    }

    private bool HasOcrData()
    {
        if (!Directory.Exists(_tessDataPath))
        {
            return false;
        }

        var normalized = string.IsNullOrWhiteSpace(_languageCode) ? "eng" : _languageCode.Trim().ToLowerInvariant();
        var filePath = Path.Combine(_tessDataPath, $"{normalized}.traineddata");
        return File.Exists(filePath);
    }
}
