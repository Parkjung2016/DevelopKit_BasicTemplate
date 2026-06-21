using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>
    /// Unity 씬이 바뀌어도 파괴되지 않고 유지되는 싱글톤-나중에 생긴 인스턴스만 유지합니다.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public abstract partial class RegulatorMonoSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;


        public float InitializationTime { get; private set; }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null)
                    {
                        var go = new GameObject(typeof(T).Name + " Auto-Generated");
                        go.hideFlags = HideFlags.HideAndDontSave;
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;
            InitializationTime = Time.time;
            DontDestroyOnLoad(gameObject);
#if UNITY_6000_4_OR_NEWER
            T[] oldInstances = FindObjectsByType<T>();
#else
            T[] oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
#endif
            foreach (T old in oldInstances)
            {
                if (old.GetComponent<RegulatorMonoSingleton<T>>().InitializationTime < InitializationTime)
                {
                    Destroy(old.gameObject);
                }
            }

            if (instance == null)
            {
                instance = this as T;
            }
        }
    }
}