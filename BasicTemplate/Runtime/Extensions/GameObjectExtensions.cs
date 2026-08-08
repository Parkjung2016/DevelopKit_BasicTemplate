using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class GameObjectExtensions
    {
        public static bool IsInLayerMask(this GameObject gameObject, LayerMask mask) =>
            gameObject != null && (mask.value & (1 << gameObject.layer)) != 0;

        public static void HideInHierarchy(this GameObject gameObject)
        {
            if (gameObject != null)
                gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        /// <summary>컴포넌트가 있으면 반환하고, 없으면 추가합니다.</summary>
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component =>
            gameObject.GetOrAddComponent<T>();

        /// <summary>파괴된 Unity Object를 실제 null로 변환합니다.</summary>
        public static T OrNull<T>(this T value) where T : Object => value ? value : null;

        public static void DestroyChildren(this GameObject gameObject)
        {
            if (gameObject != null)
                gameObject.transform.DestroyChildren();
        }

        public static void DestroyChildrenImmediate(this GameObject gameObject)
        {
            if (gameObject != null)
                gameObject.transform.DestroyChildrenImmediate();
        }

        public static void EnableChildren(this GameObject gameObject)
        {
            if (gameObject != null)
                gameObject.transform.EnableChildren();
        }

        public static void DisableChildren(this GameObject gameObject)
        {
            if (gameObject != null)
                gameObject.transform.DisableChildren();
        }

        /// <summary>로컬 위치와 회전, 크기를 기본값으로 되돌립니다.</summary>
        public static void ResetTransformation(this GameObject gameObject)
        {
            if (gameObject != null)
                gameObject.transform.Reset();
        }

        /// <summary>루트부터 현재 GameObject까지의 Hierarchy 경로를 반환합니다.</summary>
        public static string Path(this GameObject gameObject)
        {
            if (gameObject == null)
                return string.Empty;

            int depth = 0;
            for (Transform item = gameObject.transform; item != null; item = item.parent)
                depth++;

            var names = new string[depth];
            int index = depth - 1;
            for (Transform item = gameObject.transform; item != null; item = item.parent)
                names[index--] = item.name;

            return "/" + string.Join("/", names);
        }

        /// <summary>현재 GameObject의 전체 Hierarchy 경로를 반환합니다.</summary>
        public static string PathFull(this GameObject gameObject) => gameObject.Path();

        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            if (gameObject == null)
                return;

            gameObject.layer = layer;
            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetLayerRecursively(layer);
        }

        /// <summary>현재 GameObject와 모든 자식의 레이어를 변경합니다.</summary>
        public static void SetLayersRecursively(this GameObject gameObject, int layer) =>
            gameObject.SetLayerRecursively(layer);

        public static T SetActive<T>(this T behaviour) where T : MonoBehaviour
        {
            if (behaviour != null)
                behaviour.gameObject.SetActive(true);

            return behaviour;
        }

        public static T SetInactive<T>(this T behaviour) where T : MonoBehaviour
        {
            if (behaviour != null)
                behaviour.gameObject.SetActive(false);

            return behaviour;
        }
    }
}