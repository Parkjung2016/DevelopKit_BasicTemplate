using System;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public partial class Singleton<T> where T : class, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                instance ??= new T();

                return instance;
            }
        }

        public void Dispose()
        {
            OnDispose();
            instance = null;
        }

        protected virtual void OnDispose()
        {
        }
    }
}