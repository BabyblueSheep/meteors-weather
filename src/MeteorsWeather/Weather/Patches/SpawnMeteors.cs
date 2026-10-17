using HarmonyLib;

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

            var landTimeStart = MeteorWeatherMeteorsWorld.ClockToTime("8:00AM");
            var landTimeEnd = MeteorWeatherMeteorsWorld.ClockToTime("9:30AM");
            var landInterval = 2f;
            var landIntervalNormalized = landInterval  / __instance.totalTime;

            var landTime = behaviorScript.MeteorSpawnTimer + MeteorWeatherMeteorsWorld.NormalizedTimeAmountToLand;
            if (landTime > landTimeEnd)
                return;

            while (behaviorScript.MeteorSpawnTimer < __instance.normalizedTimeOfDay)
            {
                behaviorScript.GenerateMeteorToSend(behaviorScript.MeteorSpawnTimer);

                behaviorScript.MeteorSpawnTimer += landIntervalNormalized;
            }
        }

    }
}