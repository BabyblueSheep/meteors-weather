using EasyTextEffects;
using MeteorsWeather.Utils;

namespace Tests;

public class IsValidClockTests
{
    [Test]
    public void IsValidClock_NonClockString_ReturnsFalse()
    {
        Assert.That(TimeUtils.IsValidClock("Hello, World!"), Is.False);
    }

    [Test]
    public void IsValidClock_ValidRangesWithoutAmPm_ReturnsFalse([Values(4, 8, 12)] int hours, [Values(0, 15, 30, 45)] int minutes)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:00}:{minutes:00}"), Is.False);
    }

    [Test]
    public void IsValidClock_ValidRangesWithAmPm_ReturnsTrue([Range(1, 12)] int hours, [Range(0, 59)] int minutes, [Values("AM", "PM")] string amPm)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:00}:{minutes:00}{amPm}"), Is.True);
    }

    [Test]
    public void IsValidClock_InvalidHourRanges_ReturnsFalse([Values(0)] [Range(13, 99)] int hours, [Values(0, 15, 30, 45)] int minutes, [Values("AM", "PM")] string amPm)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:00}:{minutes:00}{amPm}"), Is.False);
    }

    [Test]
    public void IsValidClock_InvalidMinuteRanges_ReturnsFalse([Values(4, 8, 12)] int hours, [Range(60, 99)] int minutes, [Values("AM", "PM")] string amPm)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:00}:{minutes:00}{amPm}"), Is.False);
    }

    [Test]
    public void IsValidClock_SingleDigitHours_ReturnsFalse([Range(1, 9)] int hours, [Values(0, 15, 30, 45)] int minutes, [Values("AM", "PM")] string amPm)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:0}:{minutes:00}{amPm}"), Is.False);
    }

    [Test]
    public void IsValidClock_SingleDigitMinuteRanges_ReturnsFalse([Values(4, 8, 12)] int hours, [Range(0, 9)] int minutes, [Values("AM", "PM")] string amPm)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:00}:{minutes:0}{amPm}"), Is.False);
    }
    [Test]

    public void IsValidClock_TripleDigitHours_ReturnsFalse([Range(1, 12)] int hours, [Values(0, 15, 30, 45)] int minutes, [Values("AM", "PM")] string amPm)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:000}:{minutes:00}{amPm}"), Is.False);
    }

    [Test]
    public void IsValidClock_TripleDigitMinuteRanges_ReturnsFalse([Values(4, 8, 12)] int hours, [Range(0, 59)] int minutes, [Values("AM", "PM")] string amPm)
    {
        Assert.That(TimeUtils.IsValidClock($"{hours:00}:{minutes:000}{amPm}"), Is.False);
    }
}
