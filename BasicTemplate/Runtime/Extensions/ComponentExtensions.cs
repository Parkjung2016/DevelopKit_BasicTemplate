using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class ComponentExtensions
    {
        /// <summary>부모에서 먼저 찾고, 없으면 자식에서 컴포넌트를 찾습니다.</summary>
        public static T GetComponentInHierarchy<T>(
            this Component component,
            bool includeInactive = false)
            where T : Component
        {
            if (component == null)
                return null;

            T parentComponent = component.GetComponentInParentOnly<T>(includeInactive);
            if (parentComponent != null)
                return parentComponent;

            Transform transform = component.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                T childComponent = transform.GetChild(i).GetComponentInChildren<T>(includeInactive);
                if (childComponent != null)
                    return childComponent;
            }

            return null;
        }

        /// <summary>현재 오브젝트를 제외하고 부모에서 컴포넌트를 찾습니다.</summary>
        public static T GetComponentInParentOnly<T>(
            this Component component,
            bool includeInactive = false)
            where T : Component
        {
            if (component == null)
                return null;

            Transform parent = component.transform.parent;
            return parent != null ? parent.GetComponentInParent<T>(includeInactive) : null;
        }

        public static bool HasComponentInChildren<T>(this Component component)
            where T : Component =>
            component != null && component.GetComponentInChildren<T>() != null;

        public static bool HasComponentInParent<T>(this Component component)
            where T : Component =>
            component != null && component.GetComponentInParentOnly<T>() != null;

        public static bool HasComponent<T>(this Component component)
            where T : Component =>
            component != null && component.TryGetComponent(out T _);

        /// <summary>컴포넌트가 있으면 반환하고, 없으면 추가합니다.</summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            if (gameObject == null)
                return null;

            return gameObject.TryGetComponent(out T component)
                ? component
                : gameObject.AddComponent<T>();
        }
    }
}