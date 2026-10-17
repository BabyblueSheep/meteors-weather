using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MeteorsWeather.Weather;
using System.Reflection;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Utils;

namespace MeteorsWeather;

[BepInAutoPlugin]
[BepInDependency(MrovLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(WeatherRegistry.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public partial class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;

    public static MeteorWeathersConfig MeteorWeathersConfig { get; private set; } = null!;

    private readonly Harmony _harmony = new(Id);

    private void Awake()
    {
        PatchNetcodeMethods();

        Logger = base.Logger;
        MeteorWeathersConfig = new MeteorWeathersConfig(Config);

        MeteorWeatherRegistry.Register();
        MeteorWeatherPatchLoader.Load(_harmony);

        Logger.LogInfo($"Plugin {Name} is loaded!");
    }

    private void PatchNetcodeMethods()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }
}