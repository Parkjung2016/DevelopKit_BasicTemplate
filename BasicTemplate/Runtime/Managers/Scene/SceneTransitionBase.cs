#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public abstract class SceneTransitionBase : MonoBehaviour, ISceneTransition
    {
        public GameObject Go => gameObject;
        
        protected virtual void Awake()
        {
            SceneLoadManager.Instance.SetTransition(this);
        }
        
#if UNITASK_INSTALLED
        public abstract UniTask OnFadeOut();
        public abstract UniTask OnFadeIn();
#endif
    }
}