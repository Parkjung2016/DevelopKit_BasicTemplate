using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>
    /// Owner 하위의 오브젝트 컴포넌트를 수집하고 정해진 순서로 초기화와 Update를 전달합니다.
    /// </summary>
    public class ComponentManager
    {
        private readonly struct OrderedComponent
        {
            public OrderedComponent(IObjectComponentBase component, int order, int discoveryIndex)
            {
                Component = component;
                Order = order;
                DiscoveryIndex = discoveryIndex;
            }

            public IObjectComponentBase Component { get; }
            public int Order { get; }
            public int DiscoveryIndex { get; }
        }

        private static readonly Dictionary<Type, int> OrderCache = new();

        private readonly Dictionary<Type, IObjectComponentBase> componentsByType = new();
        private readonly Dictionary<Type, IObjectComponentBase> derivedTypeCache = new();
        private readonly HashSet<Type> missingDerivedTypes = new();
        private readonly List<IObjectComponentBase> components = new();
        private readonly List<IAfterInitable> afterInitializeComponents = new();
        private readonly List<IUpdatable> updatableComponents = new();
        private readonly List<MonoBehaviour> behaviourBuffer = new();
        private readonly List<OrderedComponent> orderBuffer = new();

        /// <summary>마지막 수집에서 발견한 컴포넌트 개수입니다.</summary>
        public int Count => components.Count;

        /// <summary>ComponentInitialize가 완료되어 Update를 전달할 수 있는 상태인지 나타냅니다.</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Owner 자신과 비활성 자식을 포함해 IObjectComponentBase 구현을 수집합니다.
        /// 다시 호출하면 이전 수집 결과를 모두 교체합니다.
        /// </summary>
        public void AddComponentToDictionary(MonoBehaviour owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            Clear();
            owner.GetComponentsInChildren(true, behaviourBuffer);

            for (int i = 0; i < behaviourBuffer.Count; i++)
            {
                if (behaviourBuffer[i] is not IObjectComponentBase component)
                    continue;

                orderBuffer.Add(new OrderedComponent(component, GetOrder(component.GetType()), i));
            }

            orderBuffer.Sort(static (left, right) =>
            {
                int order = left.Order.CompareTo(right.Order);
                return order != 0 ? order : left.DiscoveryIndex.CompareTo(right.DiscoveryIndex);
            });

            for (int i = 0; i < orderBuffer.Count; i++)
            {
                IObjectComponentBase component = orderBuffer[i].Component;
                components.Add(component);

                // 같은 타입이 여러 개면 Hierarchy에서 먼저 발견된 컴포넌트를 단일 조회 결과로 사용합니다.
                componentsByType.TryAdd(component.GetType(), component);
            }

            behaviourBuffer.Clear();
            orderBuffer.Clear();
        }

        /// <summary>수집한 컴포넌트 중 Owner 타입과 호환되는 컴포넌트를 순서대로 초기화합니다.</summary>
        public void ComponentInitialize<T>(T owner) where T : MonoBehaviour
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            afterInitializeComponents.Clear();
            updatableComponents.Clear();

            for (int i = 0; i < components.Count; i++)
            {
                IObjectComponentBase component = components[i];
                if (component is not IObjectComponent<T> objectComponent)
                    continue;

                objectComponent.Initialize(owner);
                if (component is IAfterInitable afterInitialize)
                    afterInitializeComponents.Add(afterInitialize);
                if (component is IUpdatable updatable)
                    updatableComponents.Add(updatable);
            }

            IsInitialized = true;
        }

        /// <summary>초기화된 컴포넌트에 AfterInitialize를 순서대로 전달합니다.</summary>
        public void AfterInitialize()
        {
            if (!IsInitialized)
                return;

            for (int i = 0; i < afterInitializeComponents.Count; i++)
                afterInitializeComponents[i].AfterInitialize();
        }

        /// <summary>초기화된 IUpdatable 컴포넌트에 Update를 순서대로 전달합니다.</summary>
        public void OnUpdate()
        {
            if (!IsInitialized)
                return;

            for (int i = 0; i < updatableComponents.Count; i++)
                updatableComponents[i].OnUpdate();
        }

        /// <summary>정확한 타입 또는 선택적으로 파생 타입까지 포함해 컴포넌트 하나를 반환합니다.</summary>
        public T GetCompo<T>(bool isDerived = false) where T : IObjectComponentBase
        {
            Type requestedType = typeof(T);
            if (componentsByType.TryGetValue(requestedType, out IObjectComponentBase exact))
                return (T)exact;

            if (!isDerived)
                return default;

            if (derivedTypeCache.TryGetValue(requestedType, out IObjectComponentBase cached))
                return (T)cached;
            if (missingDerivedTypes.Contains(requestedType))
                return default;

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is not T compatible)
                    continue;

                derivedTypeCache.Add(requestedType, compatible);
                return compatible;
            }

            missingDerivedTypes.Add(requestedType);
            return default;
        }

        /// <summary>컴포넌트를 찾아 반환하고, 없으면 false를 반환합니다.</summary>
        public bool TryGetCompo<T>(out T compo, bool isDerived = false) where T : IObjectComponentBase
        {
            compo = GetCompo<T>(isDerived);
            return compo != null;
        }

        /// <summary>수집된 MonoBehaviour 컴포넌트의 enabled 상태를 한 번에 변경합니다.</summary>
        public void EnableComponents(bool isEnabled)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is MonoBehaviour behaviour && behaviour != null)
                    behaviour.enabled = isEnabled;
            }
        }

        /// <summary>수집 결과와 초기화 상태를 모두 비웁니다.</summary>
        public void Clear()
        {
            componentsByType.Clear();
            derivedTypeCache.Clear();
            missingDerivedTypes.Clear();
            components.Clear();
            afterInitializeComponents.Clear();
            updatableComponents.Clear();
            behaviourBuffer.Clear();
            orderBuffer.Clear();
            IsInitialized = false;
        }

        private static int GetOrder(Type componentType)
        {
            if (OrderCache.TryGetValue(componentType, out int order))
                return order;

            order = componentType.GetCustomAttribute<ComponentOrderAttribute>(true)?.Order ?? 0;
            OrderCache.Add(componentType, order);
            return order;
        }
    }
}
