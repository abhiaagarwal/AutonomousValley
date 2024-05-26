using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using StardewModdingAPI;

namespace Summarize.World;

public class LLMLogger : ILogger
{
    IMonitor monitor;

    public LLMLogger(IMonitor monitor)
    {
        this.monitor = monitor;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => default!;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        monitor.Log(
            formatter(state, exception),
            logLevel switch
            {
                Microsoft.Extensions.Logging.LogLevel.Trace => StardewModdingAPI.LogLevel.Trace,
                Microsoft.Extensions.Logging.LogLevel.Debug => StardewModdingAPI.LogLevel.Debug,
                Microsoft.Extensions.Logging.LogLevel.Information
                    => StardewModdingAPI.LogLevel.Info,
                Microsoft.Extensions.Logging.LogLevel.Warning => StardewModdingAPI.LogLevel.Warn,
                Microsoft.Extensions.Logging.LogLevel.Error => StardewModdingAPI.LogLevel.Error,
                Microsoft.Extensions.Logging.LogLevel.Critical => StardewModdingAPI.LogLevel.Alert,
                _ => StardewModdingAPI.LogLevel.Trace,
            }
        );
    }
}

public class LLMSession
{
    private InstructExecutor executor;

    private readonly InferenceParams inferenceParams;

    private readonly string promptTemplate;

    IMonitor monitor;

    public LLMSession(string path, uint contextSize, IMonitor monitor)
    {
        var llamaLogger = new LLMLogger(monitor);
        var parameters = new ModelParams(path) { ContextSize = contextSize, };
        var model = LLamaWeights.LoadFromFile(parameters);
        var context = model.CreateContext(parameters, llamaLogger);
        executor = new InstructExecutor(
            context,
            instructionPrefix: "<|user|>\n",
            instructionSuffix: "<|assistant|>\n",
            logger: llamaLogger
        );
        inferenceParams = new InferenceParams()
        {
            Temperature = 0.7f,
            AntiPrompts = ["<|end|>"],
        };
        promptTemplate =
            "<|user|>\nYou are an AI assistant. Please answer the following question from the user: {0} <|end|><|assistant|>\n";
        this.monitor = monitor;
    }

    public async IAsyncEnumerable<string> Generate(string prompt)
    {
        var promptString = string.Format(promptTemplate, prompt);
        monitor.Log($"Prompting LLM with: {promptString}");
        await foreach (var text in executor.InferAsync(promptString, inferenceParams))
        {
            yield return text;
        }
    }
}
