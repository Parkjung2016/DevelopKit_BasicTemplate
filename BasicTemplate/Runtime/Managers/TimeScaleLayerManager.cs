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

    /// <summary>우선순위별 시간 배율을 합쳐 Unity Time.timeScale에 적용합니다.</summary>
    public sealed class TimeScaleLayerManager : Singleton<TimeScaleLayerManager>
    {
        private readonly struct Layer
        {
            public Layer(float scale, int priority)
            {
                Scale = scale;
                Priority = priority;
            }

            public float Scale { get; }
            public int Priority { get; }
        }

        private readonly Dictionary<string, Layer> layers = new();

        public int LayerCount => layers.Count;
        public float EffectiveScale => CalculateEffectiveScale();

        public void SetLayer(string key, float scale, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Layer key cannot be empty.", nameof(key));

            var layer = new Layer(Mathf.Max(0f, scale), priority);
            if (layers.TryGetValue(key, out Layer current)
                && current.Priority == layer.Priority
                && Mathf.Approximately(current.Scale, layer.Scale))
            {
                return;
            }

            layers[key] = layer;
            Apply();
        }

        public bool RemoveLayer(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !layers.Remove(key))
                return false;

            Apply();
            return true;
        }

        public bool HasLayer(string key) =>
            !string.IsNullOrWhiteSpace(key) && layers.ContainsKey(key);

        public bool TryGetLayer(string key, out float scale, out int priority)
        {
            if (!string.IsNullOrWhiteSpace(key) && layers.TryGetValue(key, out Layer layer))
            {
                scale = layer.Scale;
                priority = layer.Priority;
                return true;
            }

            scale = 1f;
            priority = 0;
            return false;
        }

        public TimeScaleLayerSnapshot[] GetLayerSnapshots()
        {
            if (layers.Count == 0)
                return Array.Empty<TimeScaleLayerSnapshot>();

            int highestPriority = GetHighestPriority();
            float effectiveScale = CalculateEffectiveScale();
            var snapshots = new TimeScaleLayerSnapshot[layers.Count];
            int index = 0;

            foreach (KeyValuePair<string, Layer> pair in layers)
            {
                Layer layer = pair.Value;
                bool isEffective = layer.Priority == highestPriority
                                   && Mathf.Approximately(layer.Scale, effectiveScale);
                snapshots[index++] = new TimeScaleLayerSnapshot(
                    pair.Key,
                    layer.Scale,
                    layer.Priority,
                    isEffective);
            }

            return snapshots;
        }

        public void ClearLayers()
        {
            layers.Clear();
            ApplyTimeScale(1f);
        }

        protected override void OnDispose()
        {
            layers.Clear();
            ApplyTimeScale(1f);
        }

        private void Apply()
        {
            ApplyTimeScale(CalculateEffectiveScale());
        }

        private float CalculateEffectiveScale()
        {
            int highestPriority = int.MinValue;
            float scale = 1f;

            foreach (Layer layer in layers.Values)
            {
                if (layer.Priority > highestPriority)
                {
                    highestPriority = layer.Priority;
                    scale = layer.Scale;
                }
                else if (layer.Priority == highestPriority && layer.Scale < scale)
                {
                    scale = layer.Scale;
                }
            }

            return scale;
        }

        private int GetHighestPriority()
        {
            int highestPriority = int.MinValue;
            foreach (Layer layer in layers.Values)
            {
                if (layer.Priority > highestPriority)
                    highestPriority = layer.Priority;
            }

            return highestPriority;
        }

        private static void ApplyTimeScale(float scale)
        {
            scale = Mathf.Max(0f, scale);
            if (!Mathf.Approximately(Time.timeScale, scale))
                Time.timeScale = scale;
        }
    }
}