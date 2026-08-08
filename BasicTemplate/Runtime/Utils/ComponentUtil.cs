using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>GameObject에서 컴포넌트와 자식을 찾는 보조 함수입니다.</summary>
    public static class ComponentUtil
    {
        /// <summary>직계 자식 또는 전체 자식에서 이름과 타입이 맞는 첫 항목을 찾습니다.</summary>
        public static T FindChild<T>(GameObject gameObject, string name = null, bool recursive = false)
            where T : Object
        {
            if (gameObject == null)
                return null;

            Transform root = gameObject.transform;
            if (!recursive)
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    Transform child = root.GetChild(i);
                    if (!string.IsNullOrEmpty(name) && child.name != name)
                        continue;

                    T component = child.GetComponent<T>();
                    if (component != null)
                        return component;
                }

                return null;
            }

            T[] components = gameObject.GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component != null && (string.IsNullOrEmpty(name) || component.name == name))
                    return component;
            }

            return null;
        }

        /// <summary>직계 자식 또는 전체 자식에서 이름이 맞는 첫 GameObject를 찾습니다.</summary>
        public static GameObject FindChild(GameObject gameObject, string name = null, bool recursive = false)
        {
            Transform child = FindChild<Transform>(gameObject, name, recursive);
            return child != null ? child.gameObject : null;
        }

        /// <summary>컴포넌트가 있으면 반환하고, 없으면 추가합니다.</summary>
        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component =>
            gameObject.GetOrAddComponent<T>();
    }
}
