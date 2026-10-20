using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Definitions;
using WeatherRegistry.Utils;

namespace MeteorsWeather.Weather;

public static class MeteorWeatherRegistry
{
    public static WeatherRegistry.Weather MeteorShowerWeather { get; private set; } = null!;

    public static GameObject SpawnSyncer { get; private set; } = null!;

    internal static void Register()
    {
        var weatherBehaviorObject = new GameObject("MeteorWeatherBehavior")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        weatherBehaviorObject.AddComponent<MeteorWeatherMeteorsWorld>();
        GameObject.DontDestroyOnLoad(weatherBehaviorObject);

        var improvedWeatherEffect = ScriptableObject.CreateInstance<ImprovedWeatherEffect>();
        improvedWeatherEffect.WorldObject = weatherBehaviorObject;
        MeteorShowerWeather = new WeatherRegistry.Weather("Meteor Shower", improvedWeatherEffect)
        {
            ColorGradient = ColorConverter.ToTMPColorGradient(new UnityEngine.Color(1f, 0.4f, 0f))
        };
        WeatherManager.RegisterWeather(MeteorShowerWeather);



        var networkObject = new GameObject("MeteorWeatherSpawnSyncer")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        networkObject.AddComponent<NetworkObject>();
        networkObject.AddComponent<MeteorSpawnSyncer>();

        networkObject.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
        networkObject.GetComponent<NetworkObject>().SceneMigrationSynchronization = true;
        networkObject.GetComponent<NetworkObject>().DestroyWithScene = false;

        // TODO: I don't like this method and I feel like there is a better way to do this.
        // But I'm fed up with figuring out NGO so whatever
        byte[] value = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + networkObject.name));
        networkObject.GetComponent<NetworkObject>().GlobalObjectIdHash = BitConverter.ToUInt32(value, 0);

        GameObject.DontDestroyOnLoad(networkObject);

        SpawnSyncer = networkObject;
    }
}
