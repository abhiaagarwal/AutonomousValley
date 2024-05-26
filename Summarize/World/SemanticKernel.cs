using LLama;
using LLama.Common;
using LLamaSharp.SemanticKernel;
using LLamaSharp.SemanticKernel.TextCompletion;
using LLamaSharp.SemanticKernel.TextEmbedding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using StardewModdingAPI;
using StardewValley;

namespace Summarize.World;

public class SemanticKernel
{
    Kernel kernel;

    IMonitor monitor;

    public SemanticKernel(string LLMPath, string embeddingPath, uint contextSize, IMonitor monitor)
    {
        var llamaLogger = new LLMLogger(monitor);

        var parameters = new ModelParams(LLMPath) { ContextSize = contextSize };
        var weights = LLamaWeights.LoadFromFile(parameters);
        var context = new LLamaContext(weights, parameters, llamaLogger);
        var instructExecutor = new InstructExecutor(
            context,
            instructionPrefix: "<|user|>\n",
            instructionSuffix: "<|assistant|>\n",
            logger: llamaLogger
        );

        var embeddingParamaters = new ModelParams(embeddingPath) {
            Embeddings = true,
        };
        var embeddingWeights = LLamaWeights.LoadFromFile(embeddingParamaters);
        var embeddingModel = new LLamaEmbedder(embeddingWeights, embeddingParamaters, llamaLogger);

        var builder = Kernel.CreateBuilder();
        builder
            .Services.AddKeyedSingleton<ITextGenerationService>(
                "local-llm",
                new LLamaSharpTextCompletion(instructExecutor)
            )
            .AddKeyedSingleton<ITextEmbeddingGenerationService>(
                "local-embedding",
                new LLamaSharpEmbeddingGeneration(embeddingModel)
            );

        kernel = builder.Build();
        this.monitor = monitor;
    }

    public async IAsyncEnumerable<float> GetEmbeddings(string text)
    {
        var embeddingsService =
            kernel.Services.GetRequiredKeyedService<ITextEmbeddingGenerationService>(
                "local-embedding"
            );
        var embeddings = await embeddingsService.GenerateEmbeddingsAsync([text]);

        foreach (var embed in embeddings)
        {
            foreach (var value in embed.Span.ToArray())
            {
                yield return value;
            }
        }
    }

    public async IAsyncEnumerable<string> GenerateText(string prompt)
    {
        var textGenerationService = kernel.Services.GetRequiredKeyedService<ITextGenerationService>(
            "local-llm"
        );
        var promptTemplate =
            "<|user|>\nYou are an AI assistant. Please answer the following question from the user: {0} <|end|><|assistant|>\n";
        var promptString = string.Format(promptTemplate, prompt);
        var settings = new LLamaSharpPromptExecutionSettings 
        {
            Temperature = 0.7f,
            StopSequences = ["<|end|>"],
        };
        monitor.Log($"Prompting LLM with: {promptString}");
        await foreach (
            var text in textGenerationService.GetStreamingTextContentsAsync(promptString, settings)
        )
        {
            var content = text.Text;
            if (content == null)
            {
                continue;
            }
            yield return content;
        }
    }
}
