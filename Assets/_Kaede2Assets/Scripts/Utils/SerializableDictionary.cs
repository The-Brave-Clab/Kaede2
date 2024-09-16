using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.Utils
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        private class Pair
        {
            public TKey key;
            public TValue value;
        }

        [SerializeField]
        private List<Pair> pairs;

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            pairs ??= new();

            pairs.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                pairs.Add(new Pair { key = pair.Key, value = pair.Value });
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            Clear();

            foreach (var pair in pairs)
            {
                Add(pair.key, pair.value);
            }

            pairs.Clear();
        }

        public SerializableDictionary()
        {
        }

        public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }
    }
}