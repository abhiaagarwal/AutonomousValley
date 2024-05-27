using HarmonyLib;
using Newtonsoft.Json;
//using Microsoft.Data.Sqlite;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Summarize.Managers;

namespace Summarize;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    internal Harmony Harmony = null!;

    internal Database.Database Database = null!;

    private ModConfig Config = null!;

    internal IModHelper helper = null!;

    /// <summary>The current time relative to the game clock.</summary>
    internal uint currentTime = 0;

    internal CharacterLocationManager characterLocationManager = null!;

    public override void Entry(IModHelper helper)
    {
        this.helper = helper;
        Harmony = new Harmony(ModManifest.UniqueID);
        Config = helper.ReadConfig<ModConfig>();
        Summarize.Database.Database.SetupSqlite(helper.DirectoryPath);
        characterLocationManager = new(Monitor);
        helper.Events.GameLoop.GameLaunched += GameLaunched;
        //helper.Events.GameLoop.UpdateTicked += UpdateTicked;
        helper.Events.GameLoop.OneSecondUpdateTicked += OneSecondUpdateTicked;
        helper.Events.World.NpcListChanged += NpcListChanged;
        helper.Events.GameLoop.SaveLoaded += SaveLoaded;
        helper.Events.GameLoop.DayStarted += DayStarted;
        helper.Events.GameLoop.DayEnding += DayEnding;
    }

    private void GameLaunched(object? sender, GameLaunchedEventArgs args) { }

    private void OneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (characterLocationManager.events.Count > 0)
        {
            var obj = characterLocationManager.events.Dequeue();
            Monitor.Log(JsonConvert.SerializeObject(obj));
        }
    }

    private void SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        var savePath =
            Constants.CurrentSavePath ?? throw new InvalidOperationException("Save path is null.");
        Database = new(savePath, Monitor);
    }

    private void DayStarted(object? sender, DayStartedEventArgs e)
    {
        characterLocationManager.InitializeDay();
    }

    private void DayEnding(object? sender, DayEndingEventArgs e)
    {
        characterLocationManager.EndDay();
    }

    private void NpcListChanged(object? sender, NpcListChangedEventArgs e)
    {
        var location = e.Location;
        // foreach (var npc in e.Removed)
        // {
        //     characterLocationManager.MarkLeft(npc, location);
        // }
        foreach (var npc in e.Added)
        {
            characterLocationManager.SetLocation(npc, location);
        }
    }

    /*
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
    */
}
