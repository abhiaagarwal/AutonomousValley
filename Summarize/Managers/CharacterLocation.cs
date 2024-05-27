using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Summarize.Managers;

/// Caches the location of characters and their locations.
public class CharacterLocationManager(IMonitor Monitor)
{
    private readonly IMonitor Monitor = Monitor;

    private readonly Dictionary<Character, (GameLocation, int, int?)> characterLocations = [];

    public readonly Queue<Events.Event> events = new();

    public void InitializeDay()
    {
        Utility.ForEachLocation(
            delegate(GameLocation location)
            {
                foreach (Character current in location.characters)
                {
                    //Monitor.Log($"(C:{current.Name}) initialized in (L:{location.Name})");
                    characterLocations[current] = (location, Game1.timeOfDay, null);
                }
                return true;
            },
            true,
            true
        );
    }

    public void EndDay()
    {
        foreach (var (character, (location, enteredTime, leftTime)) in characterLocations)
        {
            events.Enqueue(
                new Events.Event(
                    SDate.Now(),
                    1,
                    new Events.CharacterLocationEvent
                    {
                        Character = character,
                        Location = location,
                        EnteredTime = enteredTime,
                        LeftTime = leftTime ?? Game1.timeOfDay
                    },
                    [character]
                )
            );
        }
    }

    /// Marks a character as having left a location.

    public void MarkLeft(NPC character, GameLocation location)
    {
        var currentTime = Game1.timeOfDay;
        var lastTime = characterLocations[character].Item2;
        var currentLocation = characterLocations[character].Item1;
        if (!currentLocation.Equals(location))
        {
            //Monitor.Log(
            //    $"(C:{character.Name}) left (L:{location.Name}) but was in (L:{currentLocation.Name})"
            //);
            return;
        }
        //Monitor.Log($"(C:{character.Name}) left (L:{location.Name}) at {currentTime}");
        characterLocations[character] = (location, lastTime, currentTime);
    }

    /// Sets the location of a character.
    public void SetLocation(NPC character, GameLocation location)
    {
        var currentTime = Game1.timeOfDay;
        var oldLocation = characterLocations[character].Item1;
        var enteredTime = characterLocations[character].Item2;
        var leftTime = characterLocations[character].Item3;
        //Monitor.Log(
        //    $"(C:{character.Name}) entered (L:{location.Name}) from (L:{oldLocation.Name}) at {currentTime}"
        //);
        characterLocations[character] = (location, currentTime, null);
        events.Enqueue(
            new Events.Event(
                SDate.Now(),
                1,
                new Events.CharacterLocationEvent
                {
                    Character = character,
                    Location = location,
                    EnteredTime = enteredTime,
                    LeftTime = leftTime ?? Game1.timeOfDay
                },
                [character]
            )
        );
    }
}
