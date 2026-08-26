using NUnit.Framework;
using UnityEngine;
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator.test
{
    public class ValueTest
    {
        [Test]
        public void CastsRoundTrip()
        {
            Assert.AreEqual(1.5f, (float)(Value)1.5f);
            Assert.AreEqual(3, (int)(Value)3);
            Assert.AreEqual(true, (bool)(Value)true);
            Assert.AreEqual(new Vector3(1, 2, 3), (Vector3)(Value)new Vector3(1, 2, 3));
            Assert.AreEqual(Quaternion.Euler(10, 20, 30), (Quaternion)(Value)Quaternion.Euler(10, 20, 30));
            Assert.AreEqual(new Color(0.1f, 0.2f, 0.3f, 0.4f), (Color)(Value)new Color(0.1f, 0.2f, 0.3f, 0.4f));
        }

        [Test]
        public void EqualValuesHaveEqualHashCodes()
        {
            Value a = new Vector3(1, 2, 3);
            Value b = new Vector3(1, 2, 3);
            Assert.IsTrue(a.Equals(b));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void NaNEqualsNaN()
        {
            Value a = float.NaN;
            Value b = float.NaN;
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void NullArrayBehavesAsEmpty()
        {
            Assert.IsTrue(new Value(null).Equals(new Value()));
        }
    }
}
