using System.Linq;
using NUnit.Framework;
using UnityEngine;
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator.test
{
    public class PropAccessorTest
    {
        static readonly string[] ToggleItemProps = new[] { "Inactive", "Active", "TransitionOffsetPercent", "TransitionDurationPercent", "UseInactive", "UseActive", "UseTransitionToInactive", "UseTransitionToActive" };
        static readonly string[] RadialProps = new[] { "Start", "End", "StartOffsetPercent", "EndOffsetPercent" };

        [Test]
        public void ToggleItemChangedPropsCoversAllPropsAndSetPropRestores()
        {
            var a = new ToggleBlendShape();
            var b = new ToggleBlendShape { Inactive = 1, Active = 2, TransitionOffsetPercent = 10, TransitionDurationPercent = 50, UseInactive = false, UseActive = false, UseTransitionToInactive = false, UseTransitionToActive = false };
            var changed = a.ChangedProps(b).ToArray();
            CollectionAssert.AreEquivalent(ToggleItemProps, changed);
            foreach (var prop in changed) a.SetProp(prop, b.GetProp(prop));
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void ToggleMaterialChangedPropsCoversAllPropsAndSetPropRestores()
        {
            var shader = Shader.Find("Hidden/InternalErrorShader");
            var inactive = new Material(shader);
            var active = new Material(shader);
            try
            {
                var a = new ToggleMaterial();
                var b = new ToggleMaterial { Inactive = inactive, Active = active, TransitionOffsetPercent = 10, UseInactive = false, UseActive = false, UseTransitionToInactive = false, UseTransitionToActive = false };
                var changed = a.ChangedProps(b).ToArray();
                CollectionAssert.AreEquivalent(ToggleItemProps.Where(p => p != "TransitionDurationPercent"), changed);
                foreach (var prop in changed) a.SetProp(prop, b.GetProp(prop));
                Assert.IsTrue(a.Equals(b));
            }
            finally
            {
                Object.DestroyImmediate(inactive);
                Object.DestroyImmediate(active);
            }
        }

        [Test]
        public void RadialBlendShapeChangedPropsCoversAllPropsAndSetPropRestores()
        {
            var a = new RadialBlendShape();
            var b = new RadialBlendShape { Start = 1, End = 2, StartOffsetPercent = 10, EndOffsetPercent = 90 };
            var changed = a.ChangedProps(b).ToArray();
            CollectionAssert.AreEquivalent(RadialProps, changed);
            foreach (var prop in changed) a.SetProp(prop, b.GetProp(prop));
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void RadialVector3ChangedPropsCoversAllPropsAndSetPropRestores()
        {
            var a = new RadialVector3();
            var b = new RadialVector3 { Start = Vector3.one, End = Vector3.up, StartOffsetPercent = 10, EndOffsetPercent = 90 };
            var changed = a.ChangedProps(b).ToArray();
            CollectionAssert.AreEquivalent(RadialProps, changed);
            foreach (var prop in changed) a.SetProp(prop, b.GetProp(prop));
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void RadialVector4ChangedPropsCoversAllPropsAndSetPropRestores()
        {
            var a = new RadialVector4();
            var b = new RadialVector4 { Start = Vector4.one, End = new Vector4(1, 2, 3, 4), StartOffsetPercent = 10, EndOffsetPercent = 90 };
            var changed = a.ChangedProps(b).ToArray();
            CollectionAssert.AreEquivalent(RadialProps, changed);
            foreach (var prop in changed) a.SetProp(prop, b.GetProp(prop));
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void RadialValueChangedPropsCoversAllPropsAndSetPropRestores()
        {
            var a = new RadialValue { Start = 0f, End = 0f };
            var b = new RadialValue { Start = (Value)1f, End = (Value)2f, StartOffsetPercent = 10, EndOffsetPercent = 90 };
            var changed = a.ChangedProps(b).ToArray();
            CollectionAssert.AreEquivalent(RadialProps, changed);
            foreach (var prop in changed) a.SetProp(prop, b.GetProp(prop));
            Assert.IsTrue(a.Equals(b));
        }
    }
}
