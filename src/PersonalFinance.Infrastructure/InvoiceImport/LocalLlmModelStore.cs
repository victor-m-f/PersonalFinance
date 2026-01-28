namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class LocalLlmModelStore
{
    public const string ModelKey = "llm";
    public const string ModelFileName = "qwen2.5-7b-instruct-q3_k_m.gguf";
    public const string ModelDisplayName = "Qwen2.5 7B Instruct";
    public static readonly string[] ModelDownloadUrls =
    [
        "https://huggingface.co/Qwen/Qwen2.5-7B-Instruct-GGUF/resolve/main/qwen2.5-7b-instruct-q3_k_m.gguf"
    ];

    public string ModelsFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PersonalFinance",
        "models");

    public string ModelPath => Path.Combine(ModelsFolder, ModelFileName);
}
