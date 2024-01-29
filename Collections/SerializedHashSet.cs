using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.collections
{
    [Serializable]
    public class SerializedHashSet<V> : HashSet<V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        V[] values;

        public void OnAfterDeserialize()
        {
            Clear();
            UnionWith(values);
        }

        public void OnBeforeSerialize()
        {
            values = this.ToArray();
        }
    }
}

