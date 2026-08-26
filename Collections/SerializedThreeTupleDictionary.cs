using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.collections
{
    [System.Serializable]
    public class SerializedThreeTupleDictionary<K1, K2, K3, V> : Dictionary<(K1, K2, K3), V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        K1[] keys1;
        [SerializeField]
        K2[] keys2;
        [SerializeField]
        K3[] keys3;
        [SerializeField]
        V[] values;

        public bool ContainsPrimaryKey(K1 key) => Keys.Any(k => k.Item1.Equals(key));
        public void ReplacePrimaryKey(K1 oldKey, K1 newKey)
        {
            foreach (var key in Keys.Where(k => k.Item1.Equals(oldKey)).ToList())
            {
                var value = this[key];
                Remove(key);
                this[(newKey, key.Item2, key.Item3)] = value;
            }
        }

        public void ReplaceKey((K1, K2, K3) oldKey, (K1, K2, K3) newKey)
        {
            if (!ContainsKey(oldKey)) return;
            var value = this[oldKey];
            Remove(oldKey);
            this[newKey] = value;
        }

        public void OnAfterDeserialize()
        {
            Clear();
            if (keys1 == null || keys2 == null || keys3 == null || values == null) return;
            var length = Mathf.Min(Mathf.Min(keys1.Length, keys2.Length), Mathf.Min(keys3.Length, values.Length));
            for (var i = 0; i < length; i++)
            {
                this[(keys1[i], keys2[i], keys3[i])] = values[i];
            }
        }

        public void OnBeforeSerialize()
        {
            keys1 = new K1[Count];
            keys2 = new K2[Count];
            keys3 = new K3[Count];
            values = new V[Count];
            var i = 0;
            foreach (var kvp in this)
            {
                keys1[i] = kvp.Key.Item1;
                keys2[i] = kvp.Key.Item2;
                keys3[i] = kvp.Key.Item3;
                values[i] = kvp.Value;
                i++;
            }
        }
    }
}
