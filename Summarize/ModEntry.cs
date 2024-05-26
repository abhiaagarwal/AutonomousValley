using HarmonyLib;
//using Microsoft.Data.Sqlite;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Summarize;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    internal Harmony Harmony = null!;

    //        internal SqliteConnection connection = null!;

    private ModConfig Config = null!;

    internal World.SemanticKernel? kernel;

    public override void Entry(IModHelper helper)
    {
        Harmony = new Harmony(ModManifest.UniqueID);
        Config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.GameLaunched += GameLaunched;
    }

    private void GameLaunched(object? sender, GameLaunchedEventArgs args)
    {
        LoadLLM();
    }

    private void LoadLLM()
    {
        Monitor.Log("Loading LLama...");
        var modelPath = Path.Combine(Helper.DirectoryPath, "assets", Config.LLMName);
        if (!File.Exists(modelPath))
        {
            Monitor.Log($"Model file not found: {modelPath}", LogLevel.Error);
            return;
        }

        var embeddingPath = Path.Combine(Helper.DirectoryPath, "assets", Config.TextEmbeddingModel);
        if (!File.Exists(embeddingPath))
        {
            Monitor.Log($"Embedding file not found: {embeddingPath}", LogLevel.Error);
            return;
        }
        kernel = new World.SemanticKernel(modelPath, embeddingPath, Config.ContextSize, Monitor);
        Monitor.Log("LLama loaded.");

        Helper.ConsoleCommands.Add("/prompt", "Prompts the internal LLM", Prompt);
        Helper.ConsoleCommands.Add(
            "/embeddings",
            "Prints embeddings of the specified text",
            Embeddings
        );
    }

    private async void Prompt(string command, string[] args)
    {
        if (kernel == null)
        {
            Monitor.Log("Kernel not loaded.", LogLevel.Error);
            return;
        }
        var prompt = args.ElementAtOrDefault(0);
        if (prompt == default)
        {
            Monitor.Log("Prompt cannot be null.", LogLevel.Error);
            return;
        }
        Monitor.Log("Prompting LLM...");
        await Task.Run(async () =>
        {
            await foreach (var text in kernel.GenerateText(prompt))
            {
                Console.Write(text);
            }
        });
    }

    private async void Embeddings(string command, string[] args)
    {
        if (kernel == null)
        {
            Monitor.Log("Kernel not loaded.", LogLevel.Error);
            return;
        }
        var text = args.ElementAtOrDefault(0);
        if (text == default)
        {
            Monitor.Log("Text cannot be null.", LogLevel.Error);
            return;
        }
        Monitor.Log("Embedding text...");
        await Task.Run(async () =>
        {
            var embeddings = kernel.GetEmbeddings(text);
            await foreach (var embedding in embeddings)
            {
                Monitor.Log(embedding.ToString());
            }
        });
    }
}
