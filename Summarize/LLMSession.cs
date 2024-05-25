using LLama.Common;
using LLama;

public class LLMSession
{
    private InstructExecutor executor;

    private readonly InferenceParams inferenceParams;

    public LLMSession(string Path, uint ContextSize)
    {
        var parameters = new ModelParams(Path)
        {
            ContextSize = ContextSize,
        };
        var model = LLamaWeights.LoadFromFile(parameters);
        var context = model.CreateContext(parameters);
        executor = new InstructExecutor(context);
        inferenceParams = new InferenceParams() {
            Temperature = 0.7f,
        };
    }

    public async IAsyncEnumerable<string> Generate(string prompt)
    {
        await foreach (var text in executor.InferAsync(prompt, inferenceParams)) {
            yield return text;
        }
    }
}