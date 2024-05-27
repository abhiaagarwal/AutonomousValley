namespace Summarize;

public sealed class ModConfig
{
    public string LLMName { get; set; }
    public uint ContextSize { get; set; }

    public string TextEmbeddingModel { get; set; }

    public ModConfig()
    {
        LLMName = "Phi-3-mini-4k-instruct.Q4_K_M.gguf";
        ContextSize = 4096;
        TextEmbeddingModel = "mxbai-embed-large-v1.Q5_K_M.gguf";
    }
}
