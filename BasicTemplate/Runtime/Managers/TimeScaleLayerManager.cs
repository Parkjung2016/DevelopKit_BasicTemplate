using System;
using System.Collections.Generic;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public readonly struct TimeScaleLayerSnapshot
    {
        public TimeScaleLayerSnapshot(string key, float scale, int priority, bool isEffective)
        {
            Key = key;
            Scale = scale;
            Priority = priority;
            IsEffective = isEffective;
        }

        public string Key { get; }
        public float Scale { get; }
        public int Priority { get; }
        public bool IsEffective { get; }
    }
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

        public int LayerCount => timeScaleLayers.Count;
        public float EffectiveScale => CalculateEffectiveScale();

        public TimeScaleLayerSnapshot[] GetLayerSnapshots()
        {
            if (timeScaleLayers.Count == 0)
                return Array.Empty<TimeScaleLayerSnapshot>();

            int highestPriority = GetHighestPriority();
            float effectiveScale = CalculateEffectiveScale();
            var snapshots = new TimeScaleLayerSnapshot[timeScaleLayers.Count];
            int index = 0;
            foreach (var layer in timeScaleLayers)
            {
                LayerData data = layer.Value;
                bool isEffective = data.Priority == highestPriority && Mathf.Approximately(data.Scale, effectiveScale);
                snapshots[index] = new TimeScaleLayerSnapshot(layer.Key, data.Scale, data.Priority, isEffective);
                index++;
            }

            return snapshots;
        }

        /// <summary>
        /// 吏?뺥븳 ?ㅼ쓽 TimeScale ?덉씠?대? 異붽??섍굅??媛깆떊?⑸땲??
        /// 媛???믪? ?곗꽑?쒖쐞???덉씠?대뱾???곸슜?섎ŉ,
        /// ?곗꽑?쒖쐞媛 媛숇떎硫?媛????? TimeScale 媛믪씠 ?곸슜?⑸땲??
        /// </summary>
        /// <param name="key">?덉씠?대? ?앸퀎??怨좎쑀 ?ㅼ엯?덈떎.</param>
        /// <param name="scale">?곸슜??TimeScale 媛믪엯?덈떎.</param>
        /// <param name="priority">?덉씠?댁쓽 ?곗꽑?쒖쐞?낅땲?? 媛믪씠 ?믪쓣?섎줉 ?곗꽑?⑸땲??</param>
        public void SetLayer(string key, float scale, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("TimeScale ?덉씠???ㅻ뒗 鍮꾩뼱 ?덉쓣 ???놁뒿?덈떎.", nameof(key));

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
        /// 吏?뺥븳 ?ㅼ쓽 TimeScale ?덉씠?대? ?쒓굅?⑸땲??
        /// </summary>
        /// <param name="key">?쒓굅???덉씠?댁쓽 怨좎쑀 ?ㅼ엯?덈떎.</param>
        /// <returns>?덉씠?닿? 議댁옱?섏뿬 ?쒓굅?섏뿀?ㅻ㈃ true瑜?諛섑솚?⑸땲??</returns>
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
        /// 吏?뺥븳 ?ㅼ쓽 TimeScale ?덉씠?닿? ?깅줉?섏뼱 ?덈뒗吏 ?뺤씤?⑸땲??
        /// </summary>
        /// <param name="key">?뺤씤???덉씠?댁쓽 怨좎쑀 ?ㅼ엯?덈떎.</param>
        public bool HasLayer(string key)
        {
            return !string.IsNullOrWhiteSpace(key)
                   && timeScaleLayers.ContainsKey(key);
        }

        /// <summary>
        /// 吏?뺥븳 ?ㅼ쓽 TimeScale ?덉씠???뺣낫瑜?媛?몄샃?덈떎.
        /// </summary>
        /// <param name="key">媛?몄삱 ?덉씠?댁쓽 怨좎쑀 ?ㅼ엯?덈떎.</param>
        /// <param name="scale">?깅줉??TimeScale 媛믪엯?덈떎.</param>
        /// <param name="priority">?깅줉???곗꽑?쒖쐞?낅땲??</param>
        /// <returns>?덉씠?닿? 議댁옱?쒕떎硫?true瑜?諛섑솚?⑸땲??</returns>
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
        /// ?깅줉??紐⑤뱺 TimeScale ?덉씠?대? ?쒓굅?섍퀬 TimeScale??湲곕낯媛믪쑝濡?蹂듭썝?⑸땲??
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
        /// ?꾩옱 ?깅줉???덉씠?대? 湲곗??쇰줈 理쒖쥌 TimeScale??怨꾩궛?섍퀬 ?곸슜?⑸땲??
        /// </summary>
        private void Apply()
        {
            ApplyTimeScale(CalculateEffectiveScale());
        }

        private float CalculateEffectiveScale()
        {
            if (timeScaleLayers.Count == 0)
                return 1f;

            int highestPriority = GetHighestPriority();
            float finalScale = float.MaxValue;

            foreach (var layer in timeScaleLayers.Values)
            {
                if (layer.Priority != highestPriority)
                    continue;

                if (layer.Scale < finalScale)
                    finalScale = layer.Scale;
            }

            return finalScale;
        }

        private int GetHighestPriority()
        {
            int highestPriority = int.MinValue;
            foreach (var layer in timeScaleLayers.Values)
            {
                if (layer.Priority > highestPriority)
                    highestPriority = layer.Priority;
            }

            return highestPriority;
        }

        /// <summary>
        /// 怨꾩궛??TimeScale 媛믪쓣 ?곸슜?⑸땲??
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

