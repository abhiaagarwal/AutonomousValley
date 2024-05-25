using System.Runtime.InteropServices;
using HarmonyLib;
using LLama.Native;
//using Microsoft.Data.Sqlite;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Summarize
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        internal Harmony Harmony = null!;

        //        internal SqliteConnection connection = null!;

        private ModConfig Config = null!;

        internal LLMSession? session;

        public override void Entry(IModHelper helper)
        {
            // WORKAROUND: Since the game is emulated under Rosetta, we need to force the game to use the x64 version of LLama
            /*
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var path = Path.Combine(Helper.DirectoryPath, "runtimes", "osx64", "native");
                if (path != null)
                {
                    var llamaPath = Path.Combine(path, "libllama.dylib");
                    if (!File.Exists(llamaPath))
                    {
                        Monitor.Log($"LLama library not found: {llamaPath}", LogLevel.Error);
                        return;
                    }
                    var llavaPath = Path.Combine(path, "libllava_shared.dylib");
                    if (!File.Exists(llavaPath))
                    {
                        Monitor.Log($"LLava library not found: {llavaPath}", LogLevel.Error);
                        return;
                    }
                    NativeLibraryConfig.Instance.WithLibrary(llamaPath, llavaPath);
                    Monitor.Log("Overrode LLama library path for x64 MacOS", LogLevel.Info);
                }
            }
            */

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
            session = new LLMSession(modelPath, ContextSize: Config.ContextSize);
            Monitor.Log("LLama loaded.");

            Helper.ConsoleCommands.Add("prompt", "Prompts the internal LLM", Prompt);
        }

        private async void Prompt(string command, string[] args)
        {
            if (session == null)
            {
                Monitor.Log("LLama not loaded.", LogLevel.Error);
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
                await foreach (var text in session.Generate(prompt))
                {
                    Console.Write(text);
                }
            });
        }
    }
}
