using net.narazaka.avatarmenucreator.util;
using System.Collections.Generic;
using System.Linq;

namespace net.narazaka.avatarmenucreator.collections.instance
{
    [System.Serializable]
    public class RadialValueDictionary : SerializedTwoTupleDictionary<string, TypeMember, RadialValue>
    {
        public bool HasChild(string child) => Keys.Any(k => k.Item1 == child);
        public IEnumerable<TypeMember> Names(string child) => Keys.Where(k => k.Item1 == child).Select(k => k.Item2);
    }
}
