using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    internal static partial class TimerBootstrapper
    {
        private static PlayerLoopSystem timerSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveTimerManager<Update>(ref playerLoop);

            if (!InsertTimerManager<Update>(ref playerLoop, 0))
            {
                CDebug.LogWarning("TimerManager를 Unity Update 루프에 등록하지 못했습니다.");
                return;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveTimerManager<Update>(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
            TimerManager.Instance.Clear();
        }
#endif

        private static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
        {
            if (timerSystem.type != null)
                PlayerLoopUtils.RemoveSystem<T>(ref loop, in timerSystem);
        }

        private static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            timerSystem = new PlayerLoopSystem
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.Instance.UpdateTimers
            };

            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }
    }
}