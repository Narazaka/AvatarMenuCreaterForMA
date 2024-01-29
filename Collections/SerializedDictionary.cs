using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.collections
{
    // cf. https://qiita.com/kat_out/items/98420ae6dcdfee58dd07
    [Serializable]
    class SerializedDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        K[] keys;
        [SerializeField]
        V[] values;

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

