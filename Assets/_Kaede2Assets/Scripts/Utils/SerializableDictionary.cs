using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.Utils
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys;

        [SerializeField]
        private List<TValue> values;

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys ??= new();
            values ??= new();

            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
                throw new Exception($"Count mismatch between keys and values ({keys.Count} keys vs {values.Count} values)");

            for (int i = 0; i < keys.Count; i++)
                Add(keys[i], values[i]);

            keys.Clear();
            values.Clear();
        }
    }
}