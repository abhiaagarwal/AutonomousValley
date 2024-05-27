using Newtonsoft.Json;
using StardewValley;

namespace Summarize.Events;

public class CharacterLocationEvent
{
    [JsonConverter(typeof(CharacterConverter))]
    public Character Character { get; set; } = null!;

    [JsonConverter(typeof(GameLocationConverter))]
    public GameLocation Location { get; set; } = null!;

    public int EnteredTime { get; set; }

    public int LeftTime { get; set; }
}
