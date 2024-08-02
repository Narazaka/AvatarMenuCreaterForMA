using System.Collections.Generic;
using System.Linq;

namespace net.narazaka.avatarmenucreator.collections.instance
{
    [System.Serializable]
    public class ToggleValueDictionary : SerializedThreeTupleDictionary<string, string, string, ToggleValue>
    {
        public bool HasChild(string child) => Keys.Any(k => k.Item1 == child);
        public IEnumerable<string> Names(string child) => Keys.Where(k => k.Item1 == child).Select(k => k.Item2);
        public IEnumerable<(string, string)> NamePairs(string child) => Keys.Where(k => k.Item1 == child).Select(k => (k.Item2, k.Item3));
        public IEnumerable<(string, string)> NamePairs() => Keys.Select(k => (k.Item2, k.Item3));
    }
}
