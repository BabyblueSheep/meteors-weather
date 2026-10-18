using MeteorsWeather.Utils;
using MeteorsWeather.Weather;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace Tests;

public class MeteorScalesIsValidListTests
{
    [Test]
    public void IsValidList_GenericString_ReturnsFalse()
    {
        Assert.That(MeteorScalesUtils.IsValidList("Hello, World!"), Is.False);
    }

    [Test, Sequential]
    public void IsValidList_OneItemValidValues_ReturnsTrue
    (
        [Values(4f, 8f, 16f)] float scaleAverage,
        [Values(1f, 2f, 4f)] float scaleDeviation,
        [Values(100f, 500f, 25f)] float weight
    )
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverage}:{scaleDeviation}:{weight}"), Is.True);
    }

    [Test, Sequential]
    public void IsValidList_OneItemNegativeValues_ReturnsFalse
    (
        [Values(-8f, 8f, -8f)] float scaleAverage,
        [Values(2f, -2f, -2f)] float scaleDeviation,
        [Values(-100f, 100f, -100f)] float weight
    )
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverage}:{scaleDeviation}:{weight}"), Is.False);
    }

    [Test, Sequential]
    public void IsValidList_OneItemOneNegativeValue_ReturnsFalse
    (
        [Values(-8f, 8f, 8f)] float scaleAverage,
        [Values(2f, -2f, 2f)] float scaleDeviation,
        [Values(100f, 100f, -100f)] float weight
    )
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverage}:{scaleDeviation}:{weight}"), Is.False);
    }

    [Test, Sequential]
    public void IsValidList_OneItemNegativeRange_ReturnsFalse
    (
        [Values(4f, 8f)] float scaleAverage,
        [Values(8f, 16f)] float scaleDeviation,
        [Values(100f, 100f)] float weight
    )
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverage}:{scaleDeviation}:{weight}"), Is.False);
    }

    [Test, Pairwise]
    public void IsValidList_TwoItemsValidValues_ReturnsTrue
    (
        [Values(8f, 4f, 16f)] float scaleAverageFirst,
        [Values(8f, 4f, 16f)] float scaleAverageSecond,
        [Values(2f, 1f, 3f)] float scaleDeviationFirst,
        [Values(2f, 1f, 3f)] float scaleDeviationSecond,
        [Values(100f, 50f)] float weightFirst,
        [Values(100f, 50f)] float weightSecond
    )
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverageFirst}:{scaleDeviationFirst}:{weightFirst};{scaleAverageSecond}:{scaleDeviationSecond}:{weightSecond}"), Is.True);
    }

    [Test, Pairwise]
    public void IsValidList_TwoItemsOneValidOneInvalid_ReturnsFalse
    (
        [Values(8f, 4f, 16f)] float scaleAverageFirst,
        [Values(2f, 1f, 3f)] float scaleDeviationFirst,
        [Values(-8f, 4f, -16f)] float scaleAverageSecond,
        [Values(4f, 8f, -4f)] float scaleDeviationSecond,
        [Values(100f, 50f)] float weightFirst,
        [Values(100f, -50f)] float weightSecond
    )
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverageFirst}:{scaleDeviationFirst}:{weightFirst};{scaleAverageSecond}:{scaleDeviationSecond}:{weightSecond}"), Is.False);
    }

    [Test, Pairwise]
    public void IsValidList_OneItemSpaced_ReturnsTrue
(
    [Values(4f, 8f)] float scaleAverage,
    [Values(1f, 2f)] float scaleDeviation,
    [Values(100f)] float weight
)
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverage} : {scaleDeviation} : {weight}"), Is.True);
    }

    [Test, Pairwise]
    public void IsValidList_TwoItemsSpaced_ReturnsTrue
    (
        [Values(8f)] float scaleAverageFirst,
        [Values(2f)] float scaleDeviationFirst,
        [Values(16f)] float scaleAverageSecond,
        [Values(4f)] float scaleDeviationSecond,
        [Values(100f)] float weightFirst,
        [Values(100f)] float weightSecond
    )
    {
        Assert.That(MeteorScalesUtils.IsValidList($"{scaleAverageFirst}:{scaleDeviationFirst}:{weightFirst} ; {scaleAverageSecond}:{scaleDeviationSecond}:{weightSecond}"), Is.True);
    }

    [Test, Pairwise]
    public void IsValidList_FiveItemsValid_ReturnsTrue()
    {
        var random = new Random();
        var stringToCheck = "";
        for (int i = 0; i < 5; i++)
        {
            var scaleAverage = 4.0 + (16.0 - 4.0) * random.NextDouble();
            var scaleDeviation = scaleAverage / 5.0;
            var stringToAdd = $"{scaleAverage}:{scaleDeviation}:100";
            if (i != 0)
            {
                stringToCheck = $"{stringToCheck};{stringToAdd}";
            }
            else
            {
                stringToCheck = stringToAdd;
            }
        }
        Assert.That(MeteorScalesUtils.IsValidList(stringToCheck), Is.True);
    }

    [Test, Pairwise]
    public void IsValidList_FiveItemsSpaced_ReturnsTrue()
    {
        var random = new Random();
        var stringToCheck = "";
        for (int i = 0; i < 5; i++)
        {
            var scaleAverage = 4.0 + (16.0 - 4.0) * random.NextDouble();
            var scaleDeviation = scaleAverage / 5.0;
            var stringToAdd = $"{scaleAverage} : {scaleDeviation} : 100";
            if (i != 0)
            {
                stringToCheck = $"{stringToCheck} ; {stringToAdd}";
            }
            else
            {
                stringToCheck = stringToAdd;
            }
        }
        Assert.That(MeteorScalesUtils.IsValidList(stringToCheck), Is.True);
    }
}
