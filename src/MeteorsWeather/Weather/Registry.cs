using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Utils;

namespace MeteorsWeather.Weather;

public static class MeteorWeatherRegistry
{
    public static WeatherRegistry.Weather MeteorShowerWeather { get; private set; } = null!;

    internal static void Register()
    {
        var behaviorObject = new GameObject("MeteorWeatherBehavior");
        behaviorObject.hideFlags = HideFlags.HideAndDontSave;
        behaviorObject.AddComponent<MeteorWeatherMeteorsWorld>();

        MeteorShowerWeather = new WeatherRegistry.Weather("Meteor Shower", new ImprovedWeatherEffect(effectObject: null, worldObject: behaviorObject))
        {
            ColorGradient = ColorConverter.ToTMPColorGradient(new UnityEngine.Color(1f, 0.4f, 0f))
        };
        WeatherManager.RegisterWeather(MeteorShowerWeather);
    }
}
