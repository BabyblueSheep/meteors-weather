using System;
using UnityEngine;

namespace MeteorsWeather.Utils;

internal static class RandomUtils
{
    extension (System.Random thisRandom)
    {
        public float NextFloat()
        {
            return (float)thisRandom.NextDouble();
        }

        public float NextFloat(float maxValue)
        {
            return (float)thisRandom.NextDouble() * maxValue;
        }

        public float NextFloat(float minValue, float maxValue)
        {
            return minValue + (maxValue - minValue) * (float)thisRandom.NextDouble();
        }
    }
}
