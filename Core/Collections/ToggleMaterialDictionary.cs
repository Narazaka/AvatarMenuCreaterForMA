using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.collections.instance
{
    [System.Serializable]
    public class ToggleMaterialDictionary : SerializedTwoTupleDictionary<string, int, ToggleMaterial>
    {
        public bool HasChild(string child) => Keys.Any(k => k.Item1 == child);
        public IEnumerable<int> Indexes(string child) => Keys.Where(k => k.Item1 == child).Select(k => k.Item2);
        public Material[] MaterialSlots(string child)
        {
            var nonNullMaterialSlots = Keys.Where(k => k.Item1 == child).Select(k => (k.Item2, this[k].Inactive ?? this[k].Active)).Where(kv => kv.Item2 != null).ToArray();
            if (nonNullMaterialSlots.Length == 0) return new Material[0];
            var slots = new Material[nonNullMaterialSlots.Max(k => k.Item1) + 1];
            foreach (var (index, material) in nonNullMaterialSlots)
            {
                slots[index] = material;
            }
            return slots;
        }
    }
}
