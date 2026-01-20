using UnityEngine;
#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif

namespace Skddkkkk.Developkit.BasicTemplate.Runtime
{
    public interface ISceneTransition
    {
        public GameObject GO { get; }
#if UNITASK_INSTALLED
        UniTask OnFadeOut();
        UniTask OnFadeIn();
#endif
    }
}