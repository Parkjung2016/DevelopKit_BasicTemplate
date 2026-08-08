using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>현재 씬에서 하나만 유지되는 MonoBehaviour입니다.</summary>
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public abstract partial class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = FindOrCreate();

                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        public static bool TryGetInstance(out T value)
        {
            if (instance == null)
                instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);

            value = instance;
            return value != null;
        }

        protected virtual void Awake()
        {
            if (!Application.isPlaying)
                return;

            T current = this as T;
            if (instance != null && instance != current)
            {
                Destroy(gameObject);
                return;
            }

            instance = current;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private static T FindOrCreate()
        {
            T found = FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (found != null)
                return found;

            var gameObject = new GameObject($"[{typeof(T).Name}]");
            return gameObject.AddComponent<T>();
        }
    }
}