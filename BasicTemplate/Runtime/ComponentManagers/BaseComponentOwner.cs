using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>자식 오브젝트의 관리 컴포넌트를 수집하고 생명주기를 전달하는 Owner 기본 클래스입니다.</summary>
    public abstract class BaseComponentOwner<T> : MonoBehaviour where T : BaseComponentOwner<T>
    {
        private readonly ComponentManager componentManager = new();

        /// <summary>파생 클래스에서 고급 제어가 필요할 때 사용하는 ComponentManager입니다.</summary>
        protected ComponentManager Components => componentManager;

        /// <summary>Owner 하위 컴포넌트를 수집하고 전체 초기화 순서를 실행합니다.</summary>
        protected void InitComponent<T1>(T1 owner) where T1 : T
        {
            componentManager.AddComponentToDictionary(owner);
            BeforeComponentsInitialize();
            componentManager.ComponentInitialize(owner);
            componentManager.AfterInitialize();
            AfterComponentsInitialize();
        }

        /// <summary>개별 컴포넌트가 Initialize되기 직전에 호출됩니다.</summary>
        protected virtual void BeforeComponentsInitialize()
        {
        }

        /// <summary>모든 컴포넌트의 AfterInitialize가 끝난 뒤 호출됩니다.</summary>
        protected virtual void AfterComponentsInitialize()
        {
        }

        protected virtual void Update() => componentManager.OnUpdate();

        /// <summary>정확한 타입 또는 선택적으로 파생 타입까지 포함해 컴포넌트를 반환합니다.</summary>
        public T1 GetCompo<T1>(bool isDerived = false) where T1 : IObjectComponentBase =>
            componentManager.GetCompo<T1>(isDerived);

        /// <summary>컴포넌트를 찾아 반환하고, 없으면 false를 반환합니다.</summary>
        public bool TryGetCompo<T1>(out T1 compo, bool isDerived = false)
            where T1 : IObjectComponentBase =>
            componentManager.TryGetCompo(out compo, isDerived);

        /// <summary>Owner가 관리하는 MonoBehaviour 컴포넌트의 enabled 상태를 변경합니다.</summary>
        public void EnableComponents(bool isEnabled) => componentManager.EnableComponents(isEnabled);
    }
}
