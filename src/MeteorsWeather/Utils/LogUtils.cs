using System;
using System.Collections.Generic;
using System.Text;

namespace MeteorsWeather.Utils;

internal static class LogUtils
{
    public static void LogError(string message)
    {
        if (Plugin.MeteorWeathersConfig.LoggingLevel.Value >= MeteorWeathersConfig.LoggingLevels.WarnError)
        {
            Plugin.Logger.LogError(message);
        }
    }

    public static void LogWarn(string message)
    {
        if (Plugin.MeteorWeathersConfig.LoggingLevel.Value >= MeteorWeathersConfig.LoggingLevels.WarnError)
        {
            Plugin.Logger.LogWarning(message);
        }
    }

    public static void LogInfo(string message)
    {
        if (Plugin.MeteorWeathersConfig.LoggingLevel.Value >= MeteorWeathersConfig.LoggingLevels.Info)
        {
            Plugin.Logger.LogInfo(message);
        }
    }

    public static void LogDebug(string message)
    {
        if (Plugin.MeteorWeathersConfig.LoggingLevel.Value >= MeteorWeathersConfig.LoggingLevels.Debug)
        {
            Plugin.Logger.LogDebug(message);
        }
    }
}
