using System.Collections.Generic;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>자주 사용하는 코루틴 대기 객체를 재사용합니다.</summary>
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public static partial class WaitFor
    {
        private const int MaxCachedDurations = 128;
        private static readonly Dictionary<float, WaitForSeconds> SecondsCache = new();

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        public static WaitForFixedUpdate FixedUpdate { get; } = new();

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        public static WaitForEndOfFrame EndOfFrame { get; } = new();

        /// <summary>게임 시간 기준으로 기다립니다. 0 이하는 다음 프레임까지 기다립니다.</summary>
        public static WaitForSeconds Seconds(float seconds)
        {
            if (seconds <= 0f)
                return null;

            if (SecondsCache.TryGetValue(seconds, out WaitForSeconds cached))
                return cached;

            var wait = new WaitForSeconds(seconds);
            if (SecondsCache.Count < MaxCachedDurations)
                SecondsCache.Add(seconds, wait);

            return wait;
        }

        /// <summary>
        /// 실제 시간 기준으로 기다립니다. WaitForSecondsRealtime은 실행 상태를 가지므로 호출할 때마다 새로 만듭니다.
        /// </summary>
        public static WaitForSecondsRealtime SecondsRealtime(float seconds) =>
            new(Mathf.Max(0f, seconds));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearCache()
        {
            SecondsCache.Clear();
        }
    }
}