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

            var landTimeStart = MeteorWeatherMeteorsWorld.ClockToTime("8:30AM");
            var landTimeEnd = MeteorWeatherMeteorsWorld.ClockToTime("8:40AM");
            var landInterval = 0.2f;
            var landIntervalNormalized = landInterval  / __instance.totalTime;

            if (!behaviorScript.HasSpawnedInitialMeteors)
            {
                behaviorScript.HasSpawnedInitialMeteors = true;

                behaviorScript.MeteorSpawnTimer = landTimeStart - MeteorWeatherMeteorsWorld.NormalizedTimeAmountToLand;

                while (behaviorScript.MeteorSpawnTimer < 0)
                {
                    behaviorScript.GenerateMeteorToSend(behaviorScript.MeteorSpawnTimer);

                    behaviorScript.MeteorSpawnTimer += landIntervalNormalized;
                }
            }    

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