public sealed class ModConfig
{
    public string LLMName { get; set; }
    public uint ContextSize { get; set; }

    public ModConfig()
    {
        LLMName = "Phi-3-mini-4k-instruct-q4.gguf";
        ContextSize = 4096;
    }
}
