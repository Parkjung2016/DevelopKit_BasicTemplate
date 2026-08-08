using System;
using System.Collections.Generic;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    [Serializable]
    public class SerializableKeyValue<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }

    /// <summary>Unity 직렬화를 지원하는 Dictionary입니다.</summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializableKeyValue<TKey, TValue>> _keyValueList = new();

        public void OnBeforeSerialize()
        {
            _keyValueList ??= new List<SerializableKeyValue<TKey, TValue>>(Count);
            _keyValueList.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                _keyValueList.Add(new SerializableKeyValue<TKey, TValue>
                {
                    Key = pair.Key,
                    Value = pair.Value
                });
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            if (_keyValueList == null)
                return;

            for (int i = 0; i < _keyValueList.Count; i++)
            {
                SerializableKeyValue<TKey, TValue> pair = _keyValueList[i];
                if (pair == null)
                {
                    Debug.LogWarning("SerializableDictionary에서 비어 있는 항목을 건너뛰었습니다.");
                    continue;
                }

                if (!TryAdd(pair.Key, pair.Value))
                    Debug.LogWarning($"SerializableDictionary에서 중복 키를 건너뛰었습니다: {pair.Key}");
            }
        }
    }
}
