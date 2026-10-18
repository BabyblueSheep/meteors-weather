using HarmonyLib;
using WeatherRegistry;

namespace MeteorsWeather.Weather.Patches;

[HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.DecideRandomDayEvents))]
internal static class PreventNaturalMeteorShowerEvent
{
    [HarmonyPostfix]
    static void Postfix(TimeOfDay __instance)
    {
        if (!__instance.IsServer)
            return;

        var shouldDisable = false;

        if (Plugin.MeteorWeathersConfig.IsMeteorsEventEnabled.Value == MeteorWeathersConfig.VanillaMeteorEventStatus.Disabled)
        {
            shouldDisable = true;
        }
        else if (Plugin.MeteorWeathersConfig.IsMeteorsEventEnabled.Value == MeteorWeathersConfig.VanillaMeteorEventStatus.EnabledOutsideMeteorShowerWeather)
        {
            var behaviorObject = MeteorWeatherRegistry.MeteorShowerWeather.Effect.WorldObject;

            if (behaviorObject != null && behaviorObject.activeSelf)
            {
                shouldDisable = true;
            }
        }

        if (shouldDisable)
        {
            __instance.meteorShowerAtTime = -1f;
        }
    }
}