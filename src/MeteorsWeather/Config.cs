using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WeatherRegistry;

namespace MeteorsWeather;


public class MeteorWeathersConfig
{
    public enum VanillaMeteorEventStatus
    {
        Enabled,
        EnabledOutsideMeteorShowerWeather,
        Disabled,
    }

    public enum LoggingLevels
    {
        WarnError,
        Info,
        Debug,
    }


    public ConfigEntry<LoggingLevels> LoggingLevel { get; }

    public ConfigEntry<VanillaMeteorEventStatus> IsMeteorsEventEnabled { get; }

    public ConfigEntry<string> LandStartTime { get; }
    public ConfigEntry<string> LandEndTime { get; }
    public ConfigEntry<float> LandDuration { get; }
    public ConfigEntry<float> LandSpeed { get; }

    public ConfigEntry<float> MeteorSpawnIntervalAverage { get; }
    public ConfigEntry<float> MeteorSpawnIntervalDeviation { get; }

    public ConfigEntry<string> MeteorScales { get; }

    public MeteorWeathersConfig(ConfigFile configFile)
    {
        configFile.SaveOnConfigSet = false;

        var timeFormatDescription = "Valid formats are {HH}:{MM}AM or {HH}:{MM}PM e.g. 10:45AM, 6:00PM";
        var landMeaning = @"""Landing"" refers to meteors landing before getting close to the surface. ""Falling"" refers to meteors falling after getting close to the surface.";



        LoggingLevel = configFile.Bind
        (
            section: "General",
            key: "LoggingLevel",
            defaultValue: LoggingLevels.Info,
            description: @$"Controls the verbosity of logs."
        );

        IsMeteorsEventEnabled = configFile.Bind
        (
            section: "General",
            key: "IsMeteorShowerEventEnabled",
            defaultValue: VanillaMeteorEventStatus.Disabled,
            description: @$"Controls whether the vanilla meteor event is enabled or not.
            Mods that force the meteor event to happen are unaffected."
        );



        LandStartTime = configFile.Bind
        (
            section: "Weather",
            key: "StartingTimeForLanding",
            defaultValue: "8:30AM",
            description: @$"The time at which meteor landings will start.
            {timeFormatDescription}
            Values earlier than 8:00AM are allowed, and meteors will land as the day begins.
            {landMeaning}"
        );

        LandEndTime = configFile.Bind
        (
            section: "Weather",
            key: "EndingTimeForLanding",
            defaultValue: "9:30PM",
            description: @$"The total time a meteor will take to land.
            {timeFormatDescription}
            The value will be set to the value of {LandStartTime.Definition.Key} if it is lower than it.
            Values earlier than 8:00AM may not work as expected, and continue landing for several minutes after the ending time.
            {landMeaning}"
        );

        LandDuration = configFile.Bind
        (
            section: "Weather",
            key: "LandingDuration",
            defaultValue: 1f,
            description: @$"The total time a meteor will take to land, in hours.
            {landMeaning}"
        );

        LandSpeed = configFile.Bind
        (
            section: "Weather",
            key: "FallingSpeed",
            defaultValue: 0.6f,
            description: @$"The speed at which meteors will fall.
            {landMeaning}"
        );



        MeteorSpawnIntervalAverage = configFile.Bind
        (
            section: "Weather",
            key: "MeteorSpawnIntervalAverage",
            defaultValue: 5f,
            description: @$"The average interval of time between meteors spawning, roughly in seconds."
        );

        MeteorSpawnIntervalDeviation = configFile.Bind
        (
            section: "Weather",
            key: "MeteorSpawnIntervalDeviation",
            defaultValue: 3f,
            description: @$"The deviation of the interval of time between meteors spawning, roughly in seconds."
        );



        MeteorScales = configFile.Bind
        (
            section: "Weather",
            key: "MeteorScales",
            defaultValue: "8:2:100;16:4:50;16:8:20",
            description: @$"A list of meteor scale configurations, consisting of the average scale, scale deviation and the weight of the configuration.
            The list is semicolon-separated, and the configurations are colon-separated e.g. ""8:2:100;16:4:20"""
        );

        ClearOrphanedEntries(configFile);
        configFile.Save();

        configFile.SaveOnConfigSet = true;
    }

    private static void ClearOrphanedEntries(ConfigFile configFile)
    {
        PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile);
        orphanedEntries.Clear();
    }
}