using Newtonsoft.Json;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Summarize.Events;
public class Event {
    public Event(SDate date, uint importance, object eventAction, List<Character> participants) {
        Season = date.SeasonKey;
        Day = date.Day;
        Year = date.Year;
        Importance = importance;
        EventAction = eventAction;
        Participants = participants;
    }
    /// <summary>
    /// The season of the event.
    /// </summary>
    public string Season { get; set; } = null!;
    public int Day { get; set; }
    public int Year { get; set; }

    /// <summary>
    /// The importance of the event. This determines how likely the NPC is to "remember" it,
    /// and it will be prioritized in the summary.
    /// </summary>
    public uint Importance { get; set; }

    /// <summary>
    /// The actual event that took place.
    /// This must be serializable into JSON.
    /// </summary>
    public object EventAction { get; set; } = null!;
    /// <summary>
    /// The NPCs involved in the event.
    /// </summary>
    [JsonConverter(typeof(CharacterConverter))]
    public List<Character> Participants { get; set; } = null!;
}
