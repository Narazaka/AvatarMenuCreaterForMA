using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.collections
{
    [System.Serializable]
    public class SerializedTwoTupleDictionary<K1, K2, V> : Dictionary<(K1, K2), V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        K1[] keys1;
        [SerializeField]
        K2[] keys2;
        [SerializeField]
        V[] values;

        public bool ContainsPrimaryKey(K1 key) => Keys.Any(k => k.Item1.Equals(key));
        public void ReplacePrimaryKey(K1 oldKey, K1 newKey)
        {
            foreach (var key in Keys.Where(k => k.Item1.Equals(oldKey)).ToList())
            {
                var value = this[key];
                Remove(key);
                this[(newKey, key.Item2)] = value;
            }
        }

        public void ReplaceKey((K1, K2) oldKey, (K1, K2) newKey)
        {
            if (!ContainsKey(oldKey)) return;
            var value = this[oldKey];
            Remove(oldKey);
            this[newKey] = value;
        }

        public virtual (K1, K2) DefaultKey => default;
        public void OnAfterDeserialize()
        {
            Clear();
            for (var i = 0; i < keys1.Length; i++)
            {
                var key = (keys1[i], keys2[i]);
                this[ContainsKey(key) ? DefaultKey : key] = values[i];
            }
        }

        public void OnBeforeSerialize()
        {
            keys1 = new K1[Count];
            keys2 = new K2[Count];
            values = new V[Count];
            var i = 0;
            foreach (var kvp in this)
            {
                keys1[i] = kvp.Key.Item1;
                keys2[i] = kvp.Key.Item2;
                values[i] = kvp.Value;
                i++;
            }
        }
    }
}
