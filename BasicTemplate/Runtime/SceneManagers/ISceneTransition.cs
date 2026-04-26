using UnityEngine;
#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public interface ISceneTransition
    {
        public GameObject Go { get; }
#if UNITASK_INSTALLED
        UniTask OnFadeOut();
        UniTask OnFadeIn();
#endif
    }
}