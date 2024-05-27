using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SQLitePCL;
using StardewModdingAPI;
using StardewValley;
using Summarize.Events;

namespace Summarize.Database;

public class Database : IDisposable
{
    private readonly SqliteConnection connection = null!;

    private readonly IMonitor Monitor = null!;

    public static void SetupSqlite(string modPath)
    {
        string binaryName;
        string os;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            binaryName = "e_sqlite3.dll";
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
                os = "win-x64";
            else if (RuntimeInformation.OSArchitecture == Architecture.X86)
                os = "win-x86";
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm)
                os = "win-arm";
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                os = "win-arm64";
            else
                throw new Exception(
                    $"Unsupported architecture {RuntimeInformation.OSArchitecture} on {RuntimeInformation.OSDescription}"
                );
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            binaryName = "libe_sqlite3.dylib";
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
                os = "osx-x64";
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                os = "osx-arm64";
            else
                throw new Exception(
                    $"Unsupported architecture {RuntimeInformation.OSArchitecture} on {RuntimeInformation.OSDescription}"
                );
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            binaryName = "libe_sqlite3.so";
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
                os = "linux-x64";
            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                os = "linux-arm64";
            else
                throw new Exception(
                    $"Unsupported architecture {RuntimeInformation.OSArchitecture} on {RuntimeInformation.OSDescription}"
                );
        }
        else
        {
            throw new Exception("Unsupported OS.");
        }

        string fullPath = Path.Combine(modPath, "runtimes", os, "native", binaryName);
        Console.WriteLine("Loading native library: " + fullPath);

        SQLite3Provider_dynamic_cdecl.Setup("e_sqlite3", new NativeLibraryAdapter(fullPath));
        var imp = new SQLite3Provider_dynamic_cdecl();
        SQLitePCL.raw.SetProvider(imp);
        SQLitePCL.raw.FreezeProvider();
        Console.WriteLine("Loaded native library.");
    }

    public Database(string SavePath, IMonitor Monitor)
    {
        this.Monitor = Monitor;
        var connectionString = $"Data Source={Path.Combine(SavePath, "Summarize.db")}";
        connection = new(connectionString);
        Initialize();
    }

    public void Dispose()
    {
        connection.Close();
    }

    public void Initialize()
    {
        connection.Open();
        using var command = connection.CreateCommand();
        Monitor.Log("Creating Events table if it doesn't exist.");
        command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Events (
                Id INTEGER PRIMARY KEY,
                Season TEXT NOT NULL,
                Day INTEGER NOT NULL,
                Year INTEGER NOT NULL,
                Importance INTEGER NOT NULL,
                EventType TEXT NOT NULL,
                Event TEXT NOT NULL,
                Participants TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
        Monitor.Log("Created Events table if it didn't exist.");
    }

    public void InsertEvent(Events.Event @event)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            @"
            INSERT INTO Events (Season, Day, Year, Importance, EventType, Event, Participants)
            VALUES (@Season, @Day, @Year, @Importance, @EventType, @Event, @Participants);
        ";
        command.Parameters.AddWithValue("@Season", @event.Season);
        command.Parameters.AddWithValue("@Day", @event.Day);
        command.Parameters.AddWithValue("@Year", @event.Year);
        command.Parameters.AddWithValue("@Importance", @event.Importance);
        command.Parameters.AddWithValue("@EventType", @event.EventAction.GetType().Name);
        command.Parameters.AddWithValue("@Event", JsonConvert.SerializeObject(@event.EventAction));
        command.Parameters.AddWithValue(
            "@Participants",
            JsonConvert.SerializeObject(@event.Participants, new CharacterConverter())
        );
        var affected_rows = command.ExecuteNonQuery();
        if (affected_rows != 1)
        {
            throw new Exception("Failed to insert event into database.");
        }
    }
}
