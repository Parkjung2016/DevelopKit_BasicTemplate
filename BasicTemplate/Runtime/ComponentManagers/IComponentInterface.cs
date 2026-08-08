using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>모든 컴포넌트의 Initialize가 끝난 뒤 추가 설정이 필요할 때 구현합니다.</summary>
    public interface IAfterInitable
    {
        void AfterInitialize();
    }

    /// <summary>ComponentManager가 매 프레임 갱신해야 하는 컴포넌트에 구현합니다.</summary>
    public interface IUpdatable
    {
        void OnUpdate();
    }

    /// <summary>ComponentManager가 수집할 수 있는 컴포넌트를 표시합니다.</summary>
    public interface IObjectComponentBase
    {
    }

    /// <summary>지정한 Owner가 준비될 때 초기화되는 오브젝트 컴포넌트입니다.</summary>
    /// <typeparam name="T">이 컴포넌트를 소유하는 MonoBehaviour 타입입니다.</typeparam>
    public interface IObjectComponent<in T> : IObjectComponentBase where T : MonoBehaviour
    {
        void Initialize(T owner);
    }
}
