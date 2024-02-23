using System;
using System.Collections.Generic;
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

        public virtual K DefaultKey => default;
        public void OnAfterDeserialize()
        {
            Clear();
            for(var i = 0; i < keys.Length; i++)
            {
                this[ContainsKey(keys[i]) ? DefaultKey : keys[i]] = values[i];
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

