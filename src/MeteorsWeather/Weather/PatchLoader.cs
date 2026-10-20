using HarmonyLib;
using MeteorsWeather.Weather.Patches;

namespace MeteorsWeather.Weather;

internal static class MeteorWeatherPatchLoader
{
    internal static void Load(Harmony harmony)
    {
        harmony.PatchAll(typeof(RegisterNetworkPrefabs));
        harmony.PatchAll(typeof(InstantiateSpawnSyncer));

        harmony.PatchAll(typeof(PreventNaturalMeteorShowerEvent));
        harmony.PatchAll(typeof(SpawnMeteors));

        WeatherRegistry.EventManager.ShipLanding.AddListener(InitializeWeatherMeteors.InitializeMeteors);
    }
}
