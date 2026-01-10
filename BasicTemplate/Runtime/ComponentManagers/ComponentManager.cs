using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Runtime
{
    public class ComponentManager
    {
        private readonly Dictionary<Type, IObjectComponentBase> componentDic = new();

        public void AddComponentToDictionary(MonoBehaviour owner)
        {
            var components = owner.GetComponentsInChildren<IObjectComponentBase>(true);
            var orderedComponentEnumerable = components
                .OrderBy(c =>
                {
                    var attr = c.GetType().GetCustomAttributes(typeof(ComponentOrderAttribute), false)
                        .FirstOrDefault() as ComponentOrderAttribute;
                    return attr?.Order ?? 0;
                });

            foreach (var component in orderedComponentEnumerable)
            {
                var compType = component.GetType();
                componentDic[compType] = component;
            }
        }

        public void ComponentInitialize<T>(T owner) where T : MonoBehaviour
        {
            componentDic.Values.ForEach(component =>
            {
                IObjectComponent<T> objectComponent = component as IObjectComponent<T>;
                objectComponent.Initialize(owner);
            });
        }

        public void AfterInitialize()
        {
            componentDic.Values.OfType<IAfterInitable>().ForEach(afterInitable => { afterInitable.AfterInitialize(); });
        }

        public T GetCompo<T>(bool isDerived = false) where T : IObjectComponentBase
        {
            if (componentDic.TryGetValue(typeof(T), out var baseComponent))
            {
                if (baseComponent is T exactMatch)
                    return exactMatch;
            }

            if (isDerived)
            {
                foreach (var kvp in componentDic)
                {
                    if (typeof(T).IsAssignableFrom(kvp.Key))
                    {
                        if (kvp.Value is T derivedMatch)
                            return derivedMatch;
                    }
                }
            }

            return default;
        }

        public bool TryGetCompo<T1>(out T1 compo, bool isDerived = false) where T1 : IObjectComponentBase
        {
            compo = GetCompo<T1>(isDerived);
            return compo != null;
        }

        public void EnableComponents(bool isEnabled)
        {
            componentDic.Values.OfType<MonoBehaviour>().ForEach(component => { component.enabled = isEnabled; });
        }
    }
}