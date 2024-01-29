using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator.collections
{
    public static class DictionarySerializer
    {
        public class KeyValues<K, V>
        {
            public K[] Keys;
            public V[] Values;
        }

        public class KeyValues<K1, K2, V>
        {
            public K1[] Keys1;
            public K2[] Keys2;
            public V[] Values;
        }

        public static (K[], V[]) Serialize<K, V>(Dictionary<K, V> dictionary)
        {
            var keys = new K[dictionary.Count];
            var values = new V[dictionary.Count];
            var i = 0;
            foreach (var kvp in dictionary)
            {
                keys[i] = kvp.Key;
                values[i] = kvp.Value;
                i++;
            }
            return (keys, values);
        }

        public static Dictionary<K, V> Deserialize<K, V>(K[] keys, V[] values)
        {
            var dictionary = new Dictionary<K, V>();
            for (var i = 0; i < keys.Length; i++)
            {
                dictionary[keys[i]] = values[i];
            }
            return dictionary;
        }

        public static (K1[], K2[], V[]) Serialize<K1, K2, V>(Dictionary<(K1, K2), V> dictionary)
        {
            var keys1 = new K1[dictionary.Count];
            var keys2 = new K2[dictionary.Count];
            var values = new V[dictionary.Count];
            var i = 0;
            foreach (var kvp in dictionary)
            {
                keys1[i] = kvp.Key.Item1;
                keys2[i] = kvp.Key.Item2;
                values[i] = kvp.Value;
                i++;
            }
            return (keys1, keys2, values);
        }

        public static Dictionary<(K1, K2), V> Deserialize<K1, K2, V>(K1[] keys1, K2[] keys2, V[] values)
        {
            var dictionary = new Dictionary<(K1, K2), V>();
            for (var i = 0; i < keys1.Length; i++)
            {
                dictionary[(keys1[i], keys2[i])] = values[i];
            }
            return dictionary;
        }
    }
}
