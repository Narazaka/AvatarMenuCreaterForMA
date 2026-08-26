using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.collections
{
    // cf. https://qiita.com/kat_out/items/98420ae6dcdfee58dd07
    [Serializable]
    public class SerializedDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        K[] keys;
        [SerializeField]
        V[] values;

        public void ReplaceKey(K oldKey, K newKey)
        {
            if (!ContainsKey(oldKey)) return;
            var value = this[oldKey];
            Remove(oldKey);
            this[newKey] = value;
        }

        public void ReplaceKeys(Dictionary<K, K> mapping)
        {
            if (!mapping.Keys.Any(ContainsKey)) return;
            var pairs = this.ToArray();
            Clear();
            foreach (var pair in pairs)
            {
                this[mapping.TryGetValue(pair.Key, out var newKey) ? newKey : pair.Key] = pair.Value;
            }
        }

        public void SwapKey(K key1, K key2)
        {
            var hasValue1 = TryGetValue(key1, out var value1);
            var hasValue2 = TryGetValue(key2, out var value2);
            if (!hasValue1 && !hasValue2) return;
            if (hasValue1)
            {
                this[key2] = value1;
                if (!hasValue2) Remove(key1);
            }
            if (hasValue2)
            {
                this[key1] = value2;
                if (!hasValue1) Remove(key2);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            if (keys == null || values == null) return;
            var length = Mathf.Min(keys.Length, values.Length);
            for(var i = 0; i < length; i++)
            {
                this[keys[i]] = values[i];
            }
        }

        public void OnBeforeSerialize()
        {
            keys = new K[Count];
            values = new V[Count];
            var i = 0;
            foreach (var kvp in this)
            {
                keys[i] = kvp.Key;
                values[i] = kvp.Value;
                i++;
            }
        }
    }
}

