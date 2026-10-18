using System;
using System.Collections.Generic;
using System.Text;

namespace MeteorsWeather.Utils;

public static class TimeUtils
{
    public static bool IsValidClock(string timeString)
    {
        var timeParts = timeString.Split(':');
        if (timeParts.Length != 2)
            return false;

        if (!int.TryParse(timeParts[0], out var hours))
            return false;
        if (!int.TryParse(timeParts[1][..2], out var minutes))
            return false;

        if (timeParts[0].Length != 2 && timeParts[0].Length != 1)
            return false;
        if (timeParts[1][..2].Length != 2)
            return false;

        if (hours <= 0 || hours > 12)
            return false;
        if (minutes < 0 || minutes >= 60)
            return false;

        var amPm = timeParts[1][2..];
        if (amPm != "AM" && amPm != "PM")
            return false;

        return true;
    }

    // Modified from HUDManager.SetClock and https://discord.com/channels/1168655651455639582/1168656318345777313/1229245905379524680
    public static float ClockToTime(string timeString)
    {
        var numberOfHours = TimeOfDay.Instance.numberOfHours;

        var timeParts = timeString.Split(':');
        var hours = int.Parse(timeParts[0]);
        var minutes = int.Parse(timeParts[1][..2]);
        var amPm = timeParts[1][2..];

        if (amPm == "PM" && hours != 12)
        {
            hours += 12;
        }
        if (amPm == "AM" && hours == 12)
        {
            hours += 12;
        }

        var totalTime = hours * 60 + minutes;
        var timeNormalized = (totalTime - 360) / (60f * numberOfHours);

        return timeNormalized;
    }
}
