#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>씬 전환 전후에 화면 효과를 재생합니다.</summary>
    public interface ISceneTransition
    {
#if UNITASK_INSTALLED
        /// <summary>현재 화면을 가린 뒤 완료됩니다.</summary>
        UniTask OnFadeOut();

        /// <summary>새 씬을 보여 준 뒤 완료됩니다.</summary>
        UniTask OnFadeIn();
#endif
    }
}
