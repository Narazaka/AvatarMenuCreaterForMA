using System.Collections.Generic;
using System.Linq;

namespace net.narazaka.avatarmenucreator.collections.instance
{
    [System.Serializable]
    public class ChooseShaderVectorParameterDictionary : SerializedTwoTupleDictionary<string, string, IntVector4Dictionary>
    {
        public bool HasChild(string child) => Keys.Any(k => k.Item1 == child);
        public IEnumerable<string> Names(string child) => Keys.Where(k => k.Item1 == child).Select(k => k.Item2);
    }
}
