using System;
using System.Collections.Generic;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public class TimeScaleLayerManager : Singleton<TimeScaleLayerManager>
    {
        private struct LayerData
        {
            public float Scale;
            public int Priority;

            public LayerData(float scale, int priority)
            {
                Scale = scale;
                Priority = priority;
            }
        }

        private readonly Dictionary<string, LayerData> timeScaleLayers = new();

        /// <summary>
        /// 지정한 키의 TimeScale 레이어를 추가하거나 갱신합니다.
        /// 가장 높은 우선순위의 레이어들이 적용되며,
        /// 우선순위가 같다면 가장 낮은 TimeScale 값이 적용됩니다.
        /// </summary>
        /// <param name="key">레이어를 식별할 고유 키입니다.</param>
        /// <param name="scale">적용할 TimeScale 값입니다.</param>
        /// <param name="priority">레이어의 우선순위입니다. 값이 높을수록 우선됩니다.</param>
        public void SetLayer(string key, float scale, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("TimeScale 레이어 키는 비어 있을 수 없습니다.", nameof(key));

            scale = Mathf.Max(0f, scale);

            var newLayerData = new LayerData(scale, priority);

            if (timeScaleLayers.TryGetValue(key, out var currentLayerData)
                && Mathf.Approximately(currentLayerData.Scale, newLayerData.Scale)
                && currentLayerData.Priority == newLayerData.Priority)
            {
                return;
            }

            timeScaleLayers[key] = newLayerData;
            Apply();
        }

        /// <summary>
        /// 지정한 키의 TimeScale 레이어를 제거합니다.
        /// </summary>
        /// <param name="key">제거할 레이어의 고유 키입니다.</param>
        /// <returns>레이어가 존재하여 제거되었다면 true를 반환합니다.</returns>
        public bool RemoveLayer(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (!timeScaleLayers.Remove(key))
                return false;

            Apply();
            return true;
        }

        /// <summary>
        /// 지정한 키의 TimeScale 레이어가 등록되어 있는지 확인합니다.
        /// </summary>
        /// <param name="key">확인할 레이어의 고유 키입니다.</param>
        public bool HasLayer(string key)
        {
            return !string.IsNullOrWhiteSpace(key)
                   && timeScaleLayers.ContainsKey(key);
        }

        /// <summary>
        /// 지정한 키의 TimeScale 레이어 정보를 가져옵니다.
        /// </summary>
        /// <param name="key">가져올 레이어의 고유 키입니다.</param>
        /// <param name="scale">등록된 TimeScale 값입니다.</param>
        /// <param name="priority">등록된 우선순위입니다.</param>
        /// <returns>레이어가 존재한다면 true를 반환합니다.</returns>
        public bool TryGetLayer(string key, out float scale, out int priority)
        {
            if (!string.IsNullOrWhiteSpace(key)
                && timeScaleLayers.TryGetValue(key, out var layerData))
            {
                scale = layerData.Scale;
                priority = layerData.Priority;
                return true;
            }

            scale = 1f;
            priority = 0;
            return false;
        }

        /// <summary>
        /// 등록된 모든 TimeScale 레이어를 제거하고 TimeScale을 기본값으로 복원합니다.
        /// </summary>
        public void ClearLayers()
        {
            if (timeScaleLayers.Count == 0)
            {
                ApplyTimeScale(1f);
                return;
            }

            timeScaleLayers.Clear();
            ApplyTimeScale(1f);
        }

        /// <summary>
        /// 현재 등록된 레이어를 기준으로 최종 TimeScale을 계산하고 적용합니다.
        /// </summary>
        private void Apply()
        {
            if (timeScaleLayers.Count == 0)
            {
                ApplyTimeScale(1f);
                return;
            }

            int highestPriority = int.MinValue;

            foreach (var layer in timeScaleLayers.Values)
            {
                if (layer.Priority > highestPriority)
                    highestPriority = layer.Priority;
            }

            float finalScale = float.MaxValue;

            foreach (var layer in timeScaleLayers.Values)
            {
                if (layer.Priority != highestPriority)
                    continue;

                if (layer.Scale < finalScale)
                    finalScale = layer.Scale;
            }

            ApplyTimeScale(finalScale);
        }

        /// <summary>
        /// 계산된 TimeScale 값을 적용합니다.
        /// </summary>
        private static void ApplyTimeScale(float scale)
        {
            scale = Mathf.Max(0f, scale);

            if (Mathf.Approximately(Time.timeScale, scale))
                return;

            Time.timeScale = scale;
        }

        protected override void OnDispose()
        {
            if (Instance == this)
                Time.timeScale = 1f;
        }
    }
}