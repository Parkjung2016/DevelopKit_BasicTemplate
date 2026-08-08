using System;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>메인 카메라를 사용하는 간단한 Raycast 보조 함수입니다.</summary>
    public static class RayCastUtil
    {
        public static Ray GetScreenPointToRay(Vector3 position) =>
            RaycastUtility.ScreenPointToRay(GetMainCamera(), position);

        public static Vector3 GetWorldToScreenPoint(Vector3 position) =>
            RaycastUtility.WorldToScreenPoint(GetMainCamera(), position);

        public static bool GetMousePositionRaycast(
            Vector3 screenPosition,
            LayerMask layerMask,
            out RaycastHit hit) =>
            RaycastUtility.TryRaycastFromScreen(
                GetMainCamera(),
                screenPosition,
                out hit,
                Mathf.Infinity,
                layerMask);

        public static Vector3 GetMouseRayPoint(Vector3 screenPosition, LayerMask layerMask) =>
            GetMousePositionRaycast(screenPosition, layerMask, out RaycastHit hit)
                ? hit.point
                : default;

        public static Vector3 GetWorldPointOnPlane(Vector3 screenPosition) =>
            RaycastUtility.TryGetPointOnGround(GetMainCamera(), screenPosition, out Vector3 point)
                ? point
                : default;

        private static Camera GetMainCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
                throw new InvalidOperationException("MainCamera 태그가 지정된 Camera가 필요합니다.");

            return camera;
        }
    }
}
