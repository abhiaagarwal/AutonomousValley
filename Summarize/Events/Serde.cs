using Newtonsoft.Json;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Summarize.Events;

public class CharacterConverter : JsonConverter<Character>
{
    public override void WriteJson(JsonWriter writer, Character? value, JsonSerializer serializer)
    {
        writer.WriteValue(value!.Name);
    }

    public override NPC ReadJson(
        JsonReader reader,
        Type objectType,
        Character? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        NPC? npc = null;
        var value = (string)reader.Value!;
        Utility.ForEachCharacter(character =>
        {
            if (character.Name == value)
            {
                npc = character;
                return false;
            }
            return true;
        });
        if (npc == null)
        {
            throw new JsonException($"NPC not found: {value}");
        }
        return npc;
    }
}

public class GameLocationConverter : JsonConverter<GameLocation>
{
    public override void WriteJson(
        JsonWriter writer,
        GameLocation? value,
        JsonSerializer serializer
    )
    {
        writer.WriteValue(value!.Name);
    }

    public override GameLocation ReadJson(
        JsonReader reader,
        Type objectType,
        GameLocation? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        GameLocation? location = null;
        var value = (string)reader.Value!;
        Utility.ForEachLocation(
            l =>
            {
                if (l.Name == value)
                {
                    location = l;
                    return false;
                }
                return true;
            },
            includeGenerated: true,
            includeInteriors: true
        );
        if (location == null)
        {
            throw new JsonException($"Location not found: {value}");
        }
        return location;
    }
}