using System;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>지정한 Camera를 기준으로 화면 좌표 Raycast를 수행하는 유틸리티입니다.</summary>
    public static class RaycastUtility
    {
        /// <summary>화면 좌표를 Camera에서 시작하는 Ray로 변환합니다.</summary>
        public static Ray ScreenPointToRay(Camera camera, Vector2 screenPosition)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            return camera.ScreenPointToRay(screenPosition);
        }

        /// <summary>월드 좌표를 지정한 Camera의 화면 좌표로 변환합니다.</summary>
        public static Vector3 WorldToScreenPoint(Camera camera, Vector3 worldPosition)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            return camera.WorldToScreenPoint(worldPosition);
        }

        /// <summary>화면 좌표에서 Physics Raycast를 실행합니다.</summary>
        public static bool TryRaycastFromScreen(
            Camera camera,
            Vector2 screenPosition,
            out RaycastHit hit,
            float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Ray ray = ScreenPointToRay(camera, screenPosition);
            return Physics.Raycast(ray, out hit, maxDistance, layerMask, triggerInteraction);
        }

        /// <summary>화면 좌표의 Ray와 지정한 평면이 만나는 월드 좌표를 구합니다.</summary>
        public static bool TryGetPointOnPlane(
            Camera camera,
            Vector2 screenPosition,
            in Plane plane,
            out Vector3 point)
        {
            Ray ray = ScreenPointToRay(camera, screenPosition);
            if (plane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = default;
            return false;
        }

        /// <summary>화면 좌표와 지정한 높이의 수평면이 만나는 월드 좌표를 구합니다.</summary>
        public static bool TryGetPointOnGround(
            Camera camera,
            Vector2 screenPosition,
            out Vector3 point,
            float groundHeight = 0f)
        {
            var plane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
            return TryGetPointOnPlane(camera, screenPosition, plane, out point);
        }
    }
}
