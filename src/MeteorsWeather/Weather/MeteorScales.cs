using System;
using System.Collections.Generic;
using System.Text;

namespace MeteorsWeather.Weather;

public record MeteorScales(float Average, float Deviation, float Weight);

public static class MeteorScalesUtils
{
    public static bool IsValidList(string scalesList)
    {
        var scalesConfigs = scalesList.Split(";");

        foreach (var scalesConfig in scalesConfigs)
        {
            var scalesConfigParts = scalesConfig.Split(':');
            if (scalesConfigParts.Length != 3)
            {
                return false;
            }

            if (!float.TryParse(scalesConfigParts[0], out var scaleAverage))
                return false;
            if (!float.TryParse(scalesConfigParts[1], out var scaleDeviation))
                return false;
            if (!float.TryParse(scalesConfigParts[2], out var weight))
                return false;

            if (scaleAverage <= 0)
                return false;

            if (scaleDeviation <= 0)
                return false;
            if ((scaleAverage - scaleDeviation) <= 0)
                return false;

            if (weight <= 0)
                return false;
        }

        return true;
    }

    public static List<MeteorScales> StringToList(string scalesList)
    {
        List<MeteorScales> list = [];

        var scalesConfigs = scalesList.Split(";");
        foreach (var scalesConfig in scalesConfigs)
        {
            var scalesConfigParts = scalesConfig.Split(':');

            var scaleAverage = float.Parse(scalesConfigParts[0]);
            var scaleDeviation = float.Parse(scalesConfigParts[1]);
            var weight = float.Parse(scalesConfigParts[2]);

            list.Add(new(scaleAverage, scaleDeviation, weight));
        }

        return list;
    }
}
