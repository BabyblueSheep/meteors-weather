using HarmonyLib;
using MeteorsWeather.Utils;

namespace MeteorsWeather.Weather.Patches;

[HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.MoveTimeOfDay))]
internal static class SpawnMeteors
{
    [HarmonyPostfix]
    static void Postfix(TimeOfDay __instance)
    {
        if (!__instance.IsServer)
            return;

        var behaviorObject = MeteorWeatherRegistry.MeteorShowerWeather.Effect.WorldObject;
        var behaviorScript = behaviorObject.GetComponent<MeteorWeatherMeteorsWorld>();

        if (behaviorObject == null)
            return;

        if (TimeOfDay.Instance.IsServer)
        {
            if (!behaviorObject.activeSelf)
                return;

            behaviorScript.TrySpawningMeteors(__instance.totalTime, __instance.normalizedTimeOfDay);
        }

    }
}