using HarmonyLib;
using MeteorsWeather.Utils;
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
            LogUtils.LogInfo($"Disabling natural meteor showers for the day.");
            shouldDisable = true;
        }
        else if (Plugin.MeteorWeathersConfig.IsMeteorsEventEnabled.Value == MeteorWeathersConfig.VanillaMeteorEventStatus.EnabledOutsideMeteorShowerWeather)
        {
            var behaviorObject = MeteorWeatherRegistry.MeteorShowerWeather.Effect.WorldObject;

            if (behaviorObject != null && behaviorObject.activeSelf)
            {
                LogUtils.LogInfo($"Detected a Meteor Shower weather. Disabling natural meteor showers for the day.");
                shouldDisable = true;
            }
        }

        if (shouldDisable)
        {
            __instance.meteorShowerAtTime = -1f;
        }
    }
}