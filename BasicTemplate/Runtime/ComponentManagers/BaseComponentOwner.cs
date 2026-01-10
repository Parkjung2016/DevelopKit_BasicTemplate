using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Runtime
{
    public abstract class BaseComponentOwner<T> : MonoBehaviour where T : BaseComponentOwner<T>
    {
        private readonly ComponentManager componentManager = new();

        protected void InitComponent<T1>(T1 t) where T1 : T
        {
            componentManager.AddComponentToDictionary(t);
            BeforeComponentsInitialize();
            componentManager.ComponentInitialize(t);
            componentManager.AfterInitialize();
            AfterComponentsInitialize();
        }

        protected virtual void BeforeComponentsInitialize()
        {
        }

        protected virtual void AfterComponentsInitialize()
        {
        }

        public T1 GetCompo<T1>(bool isDerived = false) where T1 : IObjectComponentBase
        {
            return componentManager.GetCompo<T1>(isDerived);
        }

        public bool TryGetCompo<T1>(out T1 compo, bool isDerived = false)
            where T1 : IObjectComponentBase
        {
            return componentManager.TryGetCompo(out compo, isDerived);
        }

        public void EnableComponents(bool isEnabled)
        {
            componentManager.EnableComponents(enabled);
        }
    }
}