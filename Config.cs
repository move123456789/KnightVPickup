using RedLoader;
using UnityEngine;

namespace KnightVAndGliderPickup;

public static class Config
{
    public static ConfigCategory KnightVAndGliderPickup { get; private set; }

    public static ConfigEntry<bool> LogsToConsole { get; private set; }

    public static void Init()
    {
        KnightVAndGliderPickup = ConfigSystem.CreateCategory("Advanced", "KnightVAndGliderPickup", true);

        LogsToConsole = KnightVAndGliderPickup.CreateEntry(
            "enable_logging",
            false,
            "Enable Debug Logs",
            "Enables KnightVAndGliderPickup Logs To Be Pushed to the console.");
    }
}