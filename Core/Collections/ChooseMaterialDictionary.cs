using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.collections.instance
{
    [System.Serializable]
    public class ChooseMaterialDictionary : SerializedTwoTupleDictionary<string, int, IntMaterialDictionary>
    {
        public bool HasChild(string child) => Keys.Any(k => k.Item1 == child);
        public IEnumerable<int> Indexes(string child) => Keys.Where(k => k.Item1 == child).Select(k => k.Item2);
        public Material[] MaterialSlots(string child)
        {
            var firstNonNullMaterials = Keys.Where(k => k.Item1 == child).Select(k => (k.Item2, this[k].OrderBy(kv => kv.Key).FirstOrDefault(kv => kv.Value != null).Value)).ToArray();
            if (firstNonNullMaterials.Length == 0) return new Material[0];
            var slots = new Material[firstNonNullMaterials.Max(k => k.Item1) + 1];
            foreach (var (index, material) in firstNonNullMaterials)
            {
                slots[index] = material;
            }
            return slots;
        }
    }
}
