using System;
using System.Collections.Generic;
using System.Text;

namespace MeteorsWeather.Utils;

internal static class MathUtils
{
    extension(UnityEngine.Mathf)
    {
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }
    }
}
