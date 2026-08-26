using System.Reflection;
using NUnit.Framework;
using net.narazaka.avatarmenucreator.collections;

namespace net.narazaka.avatarmenucreator.test
{
    public class SerializedDictionaryTest
    {
        internal static void SetField(object obj, string name, object value) =>
            obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);

        [Test]
        public void DuplicateKeysLastWins()
        {
            var dic = new SerializedDictionary<int, string>();
            SetField(dic, "keys", new[] { 0, 1, 1 });
            SetField(dic, "values", new[] { "a", "b", "c" });
            dic.OnAfterDeserialize();
            Assert.AreEqual(2, dic.Count);
            Assert.AreEqual("a", dic[0]);
            Assert.AreEqual("c", dic[1]);
        }

        [Test]
        public void BrokenArraysLoadAvailablePart()
        {
            var dic = new SerializedDictionary<int, string>();
            SetField(dic, "keys", new[] { 0, 1 });
            SetField(dic, "values", new[] { "a" });
            dic.OnAfterDeserialize();
            Assert.AreEqual(1, dic.Count);
            SetField(dic, "keys", null);
            dic.OnAfterDeserialize();
            Assert.AreEqual(0, dic.Count);
        }

        [Test]
        public void TwoTupleDuplicateKeysLastWins()
        {
            var dic = new SerializedTwoTupleDictionary<string, int, string>();
            SetField(dic, "keys1", new[] { "a", "a" });
            SetField(dic, "keys2", new[] { 0, 0 });
            SetField(dic, "values", new[] { "x", "y" });
            dic.OnAfterDeserialize();
            Assert.AreEqual(1, dic.Count);
            Assert.AreEqual("y", dic[("a", 0)]);
        }

        [Test]
        public void ThreeTupleDuplicateKeysLastWins()
        {
            var dic = new SerializedThreeTupleDictionary<string, int, int, string>();
            SetField(dic, "keys1", new[] { "a", "a" });
            SetField(dic, "keys2", new[] { 0, 0 });
            SetField(dic, "keys3", new[] { 1, 1 });
            SetField(dic, "values", new[] { "x", "y" });
            dic.OnAfterDeserialize();
            Assert.AreEqual(1, dic.Count);
            Assert.AreEqual("y", dic[("a", 0, 1)]);
        }

        [Test]
        public void SerializeRoundTripPreservesEntries()
        {
            var dic = new SerializedDictionary<int, string> { [1] = "a", [2] = "b" };
            dic.OnBeforeSerialize();
            dic.Clear();
            dic.OnAfterDeserialize();
            Assert.AreEqual(2, dic.Count);
            Assert.AreEqual("a", dic[1]);
            Assert.AreEqual("b", dic[2]);
        }

        [Test]
        public void ReplaceKeyMovesValue()
        {
            var dic = new SerializedDictionary<int, string> { [1] = "a" };
            dic.ReplaceKey(1, 2);
            Assert.IsFalse(dic.ContainsKey(1));
            Assert.AreEqual("a", dic[2]);
        }

        [Test]
        public void SwapKeySwapsValues()
        {
            var dic = new SerializedDictionary<int, string> { [1] = "a", [2] = "b" };
            dic.SwapKey(1, 2);
            Assert.AreEqual("b", dic[1]);
            Assert.AreEqual("a", dic[2]);
            dic.SwapKey(1, 3);
            Assert.IsFalse(dic.ContainsKey(1));
            Assert.AreEqual("b", dic[3]);
        }

        [Test]
        public void HashSetNullValuesLoadsEmpty()
        {
            var set = new SerializedHashSet<string>();
            SetField(set, "values", null);
            set.OnAfterDeserialize();
            Assert.AreEqual(0, set.Count);
        }
    }
}
