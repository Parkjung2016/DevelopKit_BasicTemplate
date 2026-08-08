using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>씬이 바뀌어도 유지되며, 가장 나중에 생성된 인스턴스를 사용하는 싱글톤입니다.</summary>
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
                    instance = FindOrCreate();

                return instance;
            }
        }

        protected virtual void Awake() => InitializeSingleton();

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying)
                return;

            T current = this as T;
            InitializationTime = Time.realtimeSinceStartup;

            if (instance != null && instance != current)
                Destroy(instance.gameObject);

            instance = current;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
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
