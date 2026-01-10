using System.Collections.Generic;
using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Runtime
{
    public static class WaitFor
    {
        public static WaitForFixedUpdate FixedUpdate { get; } = new WaitForFixedUpdate();

        public static WaitForEndOfFrame EndOfFrame { get; } = new WaitForEndOfFrame();

        private static readonly Dictionary<float, WaitForSeconds> waitForSecondsDic =
            new Dictionary<float, WaitForSeconds>(100, new FloatComparer());
        private static readonly Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealtimeDic =
            new Dictionary<float, WaitForSecondsRealtime>(100, new FloatComparer());
        public static WaitForSeconds Seconds(float seconds)
        {
            if (seconds < 1f / Application.targetFrameRate) return null;
            if (!waitForSecondsDic.TryGetValue(seconds, out var forSeconds))
            {
                forSeconds = new WaitForSeconds(seconds);
                waitForSecondsDic[seconds] = forSeconds;
            }

            return forSeconds;
        }
        public static WaitForSecondsRealtime SecondsRealtime(float seconds)
        {
            if (seconds < 1f / Application.targetFrameRate) return null;
            if (!waitForSecondsRealtimeDic.TryGetValue(seconds, out var forSeconds))
            {
                forSeconds = new WaitForSecondsRealtime(seconds);
                waitForSecondsRealtimeDic[seconds] = forSeconds;
            }

            return forSeconds;
        }
        class FloatComparer : IEqualityComparer<float>
        {
            public bool Equals(float x, float y) => Mathf.Abs(x - y) <= Mathf.Epsilon;
            public int GetHashCode(float obj) => obj.GetHashCode();
        }
    }
}