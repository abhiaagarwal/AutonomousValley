using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace Summarize.Patches;

internal class All
{
    private static IMonitor Monitor = null!;

    internal static void Apply(Harmony harmony, IMonitor monitor)
    {
        Monitor = monitor;

        var assembly = Assembly.Load("Stardew Valley");

        foreach (var type in assembly.GetTypes())
        {
            {
                try
                {
                    // Skip anonymous types
                    if (type.IsAnonymousType())
                        continue;

                    // Loop through all public methods in the type
                    foreach (
                        var method in type.GetMethods(
                            BindingFlags.Public
                                | BindingFlags.Instance
                                | BindingFlags.Static
                                | BindingFlags.DeclaredOnly
                        )
                    )
                    {
                        if (ShouldSkipMethod(method))
                        {
                            Monitor.Log($"Skipping method: {method.Name}");
                            continue;
                        }

                        if (method is DynamicMethod)
                            continue;

                        if (method.IsSpecialName)
                            continue;

                        PatchMethod(harmony, method);
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to process type: {type.FullName}. Exception: {ex}");
                }
            }
        }
    }

    private static bool ShouldSkipMethod(MethodBase method)
    {
        string[] noisyMethodNames =
        [
            "IsOnMainThread",
            "IsLocalMultiplayer",
            "getMouseX",
            "getMouseY",
            "containsPoint",
            "IsStartDown",
            "IsRunningOnSteamDeck",
            "getBoundingBox",
            "GetBoundingBoxAt",
            "GetHashCode",
            "GetAdditionalTilePropertyRadius",
            "getTilesWide",
        ];
        string[] ignoredFullNames =
        [
            "StardewValley.Game1",
            "StardewValley.BellsAndWhistles.SpriteText",
            "StardewValley.GameRunner",
            "StardewValley.LocalizedContentManager",
            "StardewValley.TemporaryAnimatedSpriteList",
            "StardewValley.TemporaryAnimatedSprite",
            "StardewValley.Options",
            "StardewValley.InputState",
            "StardewValley.StringBuilderFormatEx",
            "StardewValley.Utility",
        ];
        string[] ignoredClasses =
        [
            "StardewValley.Menus.",
            "StardewValley.TerrainFeatures.",
            "StardewValley.Extensions.",
            "StardewValley.Pathfinding.",
            "StardewValley.ItemRegistry.",
            "StardewValley.ItemTypeDefinitions.",
            "StardewValley.Network.",
            "Netcode.",
        ];
        var fullName = method.DeclaringType!.FullName!;
        return ignoredFullNames.Contains(fullName)
            || noisyMethodNames.Contains(method.Name)
            || ignoredClasses.Any(c => fullName.StartsWith(c));
    }

    private static void PatchMethod(Harmony harmony, MethodBase method)
    {
        try
        {
            // Apply the postfix only for methods that return a value
            if (method is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void))
            {
                var postfix = new HarmonyMethod(
                    typeof(All).GetMethod(
                        nameof(LogMethodExit),
                        BindingFlags.Static | BindingFlags.NonPublic
                    )
                );
                harmony.Patch(method, null, postfix);
            }
        }
        catch (Exception ex)
        {
            // Console.WriteLine(
            //     $"Failed to patch method: {method.Name} in type: {method.DeclaringType!.FullName}. Exception: {ex}"
            // );
        }
    }

    private static void LogMethodExit(MethodBase __originalMethod, object? __result)
    {
        try
        {
            var message =
                $"Method '{__originalMethod.DeclaringType!.FullName}.{__originalMethod.Name}' called";
            if (__result != null)
            {
                message += $", returning: '{__result}'";
            }
            Monitor.Log(message);
        }
        catch (Exception ex)
        {
            Monitor.Log(
                $"Error logging method exit for {__originalMethod.DeclaringType!.FullName}.{__originalMethod.Name}. Exception: {ex}"
            );
        }
    }
}

public static class TypeExtensions
{
    public static bool IsAnonymousType(this Type type)
    {
        return type.IsGenericType
            && (type.Name.Contains("AnonymousType") || type.Name.StartsWith("<>f__AnonymousType"))
            && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
    }
}
