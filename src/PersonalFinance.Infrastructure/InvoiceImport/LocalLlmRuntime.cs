using System.Text;
using LLama;
using LLama.Common;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class LocalLlmRuntime
{
    private readonly LocalLlmModelStore _modelStore;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private LLamaWeights? _weights;

    public LocalLlmRuntime(LocalLlmModelStore modelStore)
    {
        _modelStore = modelStore;
    }

    public async Task<Result<string>> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        if (!File.Exists(_modelStore.ModelPath))
        {
            return Result<string>.Failure("ModelNotReady", "LLM model not downloaded.");
        }

        return await Task.Run(async () =>
        {
            await _mutex.WaitAsync(ct);
            try
            {
                var parameters = new ModelParams(_modelStore.ModelPath)
                {
                    ContextSize = 2048,
                    GpuLayerCount = 0
                };

                _weights ??= LLamaWeights.LoadFromFile(parameters);
                using var context = _weights.CreateContext(parameters);
                var executor = new InteractiveExecutor(context);

                var inference = new InferenceParams
                {
                    Temperature = 0.2f,
                    MaxTokens = 256,
                    AntiPrompts = new[] { "<|user|>", "<|system|>", "</s>" }
                };

                var prompt = $"<|system|>\n{systemPrompt}\n<|user|>\n{userPrompt}\n<|assistant|>\n";
                var sb = new StringBuilder();
                await foreach (var token in executor.InferAsync(prompt, inference, ct))
                {
                    sb.Append(token);
                }

                var text = sb.ToString().Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return Result<string>.Failure("LlmFailed", "Empty response.");
                }

                return Result<string>.Success(text);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure("LlmFailed", ex.Message);
            }
            finally
            {
                _mutex.Release();
            }
        }, ct);
    }
}
