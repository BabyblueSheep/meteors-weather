using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeatherRegistry;

namespace MeteorsWeather.Weather.Patches;

static class InitializeWeatherMeteors
{
    internal static void InitializeMeteors((SelectableLevel level, WeatherRegistry.Weather weather) param)
    {
        Plugin.Logger.LogInfo($"test0 {MeteorWeatherMeteorsWorld.ClockToTime("8:30AM")} {MeteorWeatherMeteorsWorld.ClockToTime("9:00PM")}");

        var behaviorObject = MeteorWeatherRegistry.MeteorShowerWeather.Effect.WorldObject;
        var behaviorScript = behaviorObject.GetComponent<MeteorWeatherMeteorsWorld>();

        if (behaviorObject == null)
            return;

        if (!MeteorInstance.VanillaMeteorPrefab)
            MeteorInstance.VanillaMeteorPrefab = TimeOfDay.Instance.MeteorWeather.meteorPrefab;
        if (!MeteorInstance.VanillaDistantSpritePrefab)
            MeteorInstance.VanillaDistantSpritePrefab = TimeOfDay.Instance.MeteorWeather.distantSpritePrefab;
        if (!MeteorInstance.VanillaMeteorLandingExplosionPrefab)
            MeteorInstance.VanillaMeteorLandingExplosionPrefab = TimeOfDay.Instance.MeteorWeather.meteorLandingExplosion;
        if (!MeteorInstance.VanillaInsideFactoryMeteorClip)
            MeteorInstance.VanillaInsideFactoryMeteorClip = TimeOfDay.Instance.MeteorWeather.insideFactoryMeteorClip;

        behaviorScript.SetUpForThisDay();

        if (TimeOfDay.Instance.IsServer)
        {
            if (!behaviorObject.activeSelf)
                return;
        }
    }
}
