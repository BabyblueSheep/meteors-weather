using HarmonyLib;

namespace MeteorsWeather.Weather.Patches;

[HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.DecideRandomDayEvents))]
internal static class PreventNaturalMeteorShowerEvent
{
    [HarmonyPostfix]
    static void Postfix(TimeOfDay __instance)
    {
        if (!__instance.IsServer)
            return;

        __instance.meteorShowerAtTime = -1f;
    }
}