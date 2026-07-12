using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// 컴포넌트의 계층에서 <typeparamref name="T"/> 타입의 첫 번째 컴포넌트를 찾습니다.
        /// 부모를 먼저 탐색하며, 찾지 못한 경우 자식을 탐색합니다.
        /// </summary>
        /// <typeparam name="T">검색할 컴포넌트 타입입니다.</typeparam>
        /// <param name="component">계층을 탐색할 기준 컴포넌트입니다.</param>
        /// <param name="includeInactive">비활성화된 오브젝트도 검색에 포함할지 여부입니다.</param>
        /// <returns>
        /// 부모 또는 자식에서 처음으로 찾은 컴포넌트를 반환합니다.
        /// 자식 검색 시에는 자기 자신은 제외하며, 찾지 못한 경우 null을 반환합니다.
        /// </returns>
        /// <example>
        /// <code>
        /// AudioListener listener = someComponent.GetComponentInHierarchy&lt;AudioListener&gt;(includeInactive: true);
        /// </code>
        /// </example>
        public static T GetComponentInHierarchy<T>(this Component component, bool includeInactive = false)
            where T : Component
        {
            if (component == null) return null;

            var parentComponent = component.GetComponentInParentOnly<T>(includeInactive);
            if (parentComponent != null)
            {
                return parentComponent;
            }

            var hierarchyComponents = component.GetComponentsInChildren<T>(includeInactive);
            for (var i = 0; i < hierarchyComponents.Length; i++)
            {
                var candidate = hierarchyComponents[i];
                if (candidate.transform != component.transform)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// 부모 계층에서만 <typeparamref name="T"/> 타입의 컴포넌트를 찾습니다.
        /// 현재 오브젝트는 검색 대상에 포함되지 않습니다.
        /// </summary>
        /// <typeparam name="T">검색할 컴포넌트 타입입니다.</typeparam>
        /// <param name="component">검색을 시작할 기준 컴포넌트입니다.</param>
        /// <param name="includeInactive">비활성화된 부모 오브젝트도 검색에 포함할지 여부입니다.</param>
        /// <returns>부모 계층에서 찾은 첫 번째 컴포넌트를 반환하며, 찾지 못한 경우 null을 반환합니다.</returns>
        /// <example>
        /// <code>
        /// Rigidbody parentBody = wheelCollider.GetComponentInParentOnly&lt;Rigidbody&gt;();
        /// </code>
        /// </example>
        public static T GetComponentInParentOnly<T>(this Component component, bool includeInactive = false)
            where T : Component
        {
            if (component == null) return null;

            var parent = component.transform.parent;
            return parent != null ? parent.GetComponentInParent<T>(includeInactive) : null;
        }

        /// <summary>
        /// 자식 오브젝트들 중에 지정한 타입의 컴포넌트가 존재하는지 확인합니다.
        /// </summary>
        /// <typeparam name="T">검색할 컴포넌트 타입입니다.</typeparam>
        /// <param name="component">기준이 되는 컴포넌트입니다.</param>
        /// <returns>컴포넌트가 존재하면 true, 없으면 false를 반환합니다.</returns>
        public static bool HasComponentInChildren<T>(this Component component) where T : Component
        {
            return component.GetComponentInChildren<T>();
        }

        /// <summary>
        /// 부모 오브젝트들 중에 지정한 타입의 컴포넌트가 존재하는지 확인합니다.
        /// </summary>
        /// <typeparam name="T">검색할 컴포넌트 타입입니다.</typeparam>
        /// <param name="component">기준이 되는 컴포넌트입니다.</param>
        /// <returns>컴포넌트가 존재하면 true, 없으면 false를 반환합니다.</returns>
        public static bool HasComponentInParent<T>(this Component component) where T : Component
        {
            return component.GetComponentInChildren<T>();
        }

        /// <summary>
        /// 현재 오브젝트에 지정한 타입의 컴포넌트가 존재하는지 확인합니다.
        /// </summary>
        /// <typeparam name="T">검색할 컴포넌트 타입입니다.</typeparam>
        /// <param name="component">기준이 되는 컴포넌트입니다.</param>
        /// <returns>컴포넌트가 존재하면 true, 없으면 false를 반환합니다.</returns>
        public static bool HasComponent<T>(this Component component) where T : Component
        {
            return component.GetComponent<T>();
        }

        /// <summary>
        /// 현재 오브젝트에 지정합 타입의 컴포넌트틀 찾아서 반환합니다. 없으면 새로 추가합니다.
        /// </summary>
        /// <typeparam name="T">검색할 컴포넌트 타입입니다.</typeparam>
        /// <param name="gameObject">기준이 되는 오브젝트입니다.</param>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            return ComponentUtil.GetOrAddComponent<T>(gameObject);
        }
    }
}