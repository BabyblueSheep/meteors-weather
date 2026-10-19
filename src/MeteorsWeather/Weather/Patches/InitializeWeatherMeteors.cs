using HarmonyLib;
using MeteorsWeather.Utils;
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
        var behaviorObject = MeteorWeatherRegistry.MeteorShowerWeather.Effect.WorldObject;
        var behaviorScript = behaviorObject.GetComponent<MeteorWeatherMeteorsWorld>();

        if (behaviorObject == null)
            return;

        if (!MeteorInstance.VanillaMeteorPrefab)
        {
            LogUtils.LogDebug($"Currently not storing meteor prefab. Getting it from vanilla.");
            MeteorInstance.VanillaMeteorPrefab = TimeOfDay.Instance.MeteorWeather.meteorPrefab;
        }
        if (!MeteorInstance.VanillaDistantSpritePrefab)
        {
            LogUtils.LogDebug($"Currently not storing distant sprite prefab. Getting it from vanilla.");
            MeteorInstance.VanillaDistantSpritePrefab = TimeOfDay.Instance.MeteorWeather.distantSpritePrefab;
        }
        if (!MeteorInstance.VanillaMeteorLandingExplosionPrefab)
        {
            LogUtils.LogDebug($"Currently not storing landing explosion prefab. Getting it from vanilla.");
            MeteorInstance.VanillaMeteorLandingExplosionPrefab = TimeOfDay.Instance.MeteorWeather.meteorLandingExplosion;
        }
        if (!MeteorInstance.VanillaInsideFactoryMeteorClip)
        {
            LogUtils.LogDebug($"Currently not storing muffled inside factory audio. Getting it from vanilla.");
            MeteorInstance.VanillaInsideFactoryMeteorClip = TimeOfDay.Instance.MeteorWeather.insideFactoryMeteorClip;
        }

        behaviorScript.SetUpForThisDay();

        if (TimeOfDay.Instance.IsServer)
        {
            if (!behaviorObject.activeSelf)
                return;
        }
    }
}
