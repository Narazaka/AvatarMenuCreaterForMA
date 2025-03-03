using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace net.narazaka.avatarmenucreator.collections
{
    [Serializable]
    public class SerializedHashSet<V> : ISerializationCallbackReceiver, IEnumerable<V>
    {
        [SerializeField]
        V[] values;
        [NonSerialized]
        HashSet<V> hashSet = new HashSet<V>();

        public int Count => hashSet.Count;
        public bool Add(V value) => hashSet.Add(value);
        public bool Remove(V value) => hashSet.Remove(value);
        public void Clear() => hashSet.Clear();
        public bool Contains(V value) => hashSet.Contains(value);
        public void UnionWith(IEnumerable<V> other) => hashSet.UnionWith(other);

        public void OnAfterDeserialize()
        {
            hashSet = new HashSet<V>();
            hashSet.UnionWith(values);
        }

        public void OnBeforeSerialize()
        {
            values = new V[hashSet.Count];
            hashSet.CopyTo(values);
        }

        public IEnumerator<V> GetEnumerator()
        {
            return hashSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return hashSet.GetEnumerator();
        }
    }
}
