using System;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>필요할 때 한 번 생성되는 일반 C# 싱글톤입니다.</summary>
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public partial class Singleton<T> : IDisposable where T : class, new()
    {
        private static T instance;

        public static T Instance => instance ??= new T();
        public static bool HasInstance => instance != null;

        public static bool TryGetInstance(out T value)
        {
            value = instance;
            return value != null;
        }

        public void Dispose()
        {
            if (!ReferenceEquals(instance, this))
                return;

            OnDispose();
            instance = null;
        }

        protected virtual void OnDispose()
        {
        }
    }
}