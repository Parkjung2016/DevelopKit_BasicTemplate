using System.Collections.Generic;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public class TimeScaleLayerManager : Singleton<TimeScaleLayerManager>
    {
        private class LayerData
        {
            public float Scale;
            public int Priority;
        }

        private Dictionary<string, LayerData> timeScaleLayers = new Dictionary<string, LayerData>();

        /// <param name="key">Layer 이름</param>
        /// <param name="scale">적용할 TimeScale 값</param>
        /// <param name="priority">우선순위 (높을수록 우선)</param>
        public void SetLayer(string key, float scale, int priority = 0)
        {
            timeScaleLayers[key] = new LayerData { Scale = scale, Priority = priority };
            Apply();
        }

        public void RemoveLayer(string key)
        {
            timeScaleLayers.Remove(key);

            Apply();
        }

        private void Apply()
        {
            if (timeScaleLayers.Count == 0)
            {
                Time.timeScale = 1f;
                return;
            }

            int maxPriority = int.MinValue;
            foreach (var kv in timeScaleLayers)
                maxPriority = Mathf.Max(maxPriority, kv.Value.Priority);

            float finalScale = 1f;
            foreach (var kv in timeScaleLayers)
            {
                if (kv.Value.Priority == maxPriority)
                    finalScale = Mathf.Min(finalScale, kv.Value.Scale);
            }

            Time.timeScale = finalScale;
        }
    }
}