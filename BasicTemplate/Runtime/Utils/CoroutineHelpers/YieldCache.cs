using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>기존 코루틴 코드에서 WaitFor 캐시를 사용할 수 있게 연결합니다.</summary>
    public static class YieldCache
    {
        public static WaitForSeconds GetWaitForSeconds(float seconds) => WaitFor.Seconds(seconds);

        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float seconds) =>
            WaitFor.SecondsRealtime(seconds);
    }
}
