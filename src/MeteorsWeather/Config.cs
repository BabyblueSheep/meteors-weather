using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using WeatherRegistry;

namespace MeteorsWeather;


public class MeteorWeathersConfig
{
    public ConfigEntry<string> LandStartTime { get; }
    public ConfigEntry<string> LandEndTime { get; }

    public MeteorWeathersConfig(ConfigFile configFile)
    {
        var timeFormatDescription = "Valid formats are {HH}:{MM}AM or {HH}:{MM}PM e.g. 10:45AM or 6:00PM";

        LandStartTime = configFile.Bind
        (
            section: "General",
            key: "Starting Time for Landing",
            defaultValue: "8:30AM",
            description: @$"The time at which meteor landings will start.
            {timeFormatDescription}.
            Note that they will spawn earlier than this, even spawning when the day starts if the value is low enough."
        );

        LandEndTime = configFile.Bind
        (
            section: "General",
            key: "Ending Time for Landing",
            defaultValue: "9:30PM",
            description: @$"The time at which meteor landings will stop.
            {timeFormatDescription}.
            The value will be set to the value of {LandStartTime.Definition.Key} if it is lower than it."
        );
    }
}