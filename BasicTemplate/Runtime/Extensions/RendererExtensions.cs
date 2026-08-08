using System;
using System.Collections.Generic;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class RendererExtensions
    {
        private static readonly List<Material> MaterialBuffer = new();

        /// <summary>Renderer의 인스턴스 머티리얼에 ZWrite 설정을 적용합니다.</summary>
        public static void SetZWrite(this Renderer renderer, bool enabled, int? renderQueue = null)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            MaterialBuffer.Clear();
            renderer.GetMaterials(MaterialBuffer);
            for (int i = 0; i < MaterialBuffer.Count; i++)
            {
                Material material = MaterialBuffer[i];
                if (material == null || !material.HasProperty("_ZWrite"))
                    continue;

                material.SetInt("_ZWrite", enabled ? 1 : 0);
                if (renderQueue.HasValue)
                    material.renderQueue = renderQueue.Value;
            }

            MaterialBuffer.Clear();
        }

        public static void EnableZWrite(this Renderer renderer) =>
            renderer.SetZWrite(true, (int)UnityEngine.Rendering.RenderQueue.Transparent);

        public static void DisableZWrite(this Renderer renderer) =>
            renderer.SetZWrite(false, (int)UnityEngine.Rendering.RenderQueue.Transparent + 100);
    }
}