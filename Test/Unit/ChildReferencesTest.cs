using NUnit.Framework;
using UnityEngine;
using net.narazaka.avatarmenucreator.collections.instance;

namespace net.narazaka.avatarmenucreator.test
{
    public class ChildReferencesTest
    {
        GameObject root, a, b;
        AvatarToggleMenu menu;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Avatar");
            a = new GameObject("A"); a.transform.SetParent(root.transform);
            b = new GameObject("B"); b.transform.SetParent(root.transform);
            menu = new AvatarToggleMenu();
            menu.ToggleObjects["A"] = ToggleType.ON;
            menu.ToggleObjects["B"] = ToggleType.ON;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void RenameFollowsReference()
        {
            Assert.IsFalse(menu.SyncChildReferences(root)); // 参照を補う
            Assert.AreEqual(a, menu.ChildReferences["A"]);
            a.name = "A2";
            Assert.IsTrue(menu.SyncChildReferences(root));
            Assert.IsTrue(menu.ToggleObjects.ContainsKey("A2"));
            Assert.IsFalse(menu.ToggleObjects.ContainsKey("A"));
            Assert.AreEqual(a, menu.ChildReferences["A2"]);
        }

        [Test]
        public void NoReferenceKeepsPath()
        {
            a.name = "A2";
            Assert.IsFalse(menu.SyncChildReferences(root));
            Assert.IsTrue(menu.ToggleObjects.ContainsKey("A"));
        }

        [Test]
        public void ChainedRenameConverges()
        {
            menu.SyncChildReferences(root);
            b.name = "C";
            a.name = "B";
            Assert.IsTrue(menu.SyncChildReferences(root));
            Assert.AreEqual(a, menu.ChildReferences["B"]);
            Assert.AreEqual(b, menu.ChildReferences["C"]);
            Assert.IsFalse(menu.ToggleObjects.ContainsKey("A"));
        }

        [Test]
        public void SwapFollowsReferences()
        {
            menu.ToggleObjects["B"] = ToggleType.OFF;
            menu.SyncChildReferences(root);
            a.name = "B";
            b.name = "A";
            Assert.IsTrue(menu.SyncChildReferences(root));
            Assert.AreEqual(a, menu.ChildReferences["B"]);
            Assert.AreEqual(b, menu.ChildReferences["A"]);
            Assert.AreEqual(ToggleType.ON, menu.ToggleObjects["B"]);
            Assert.AreEqual(ToggleType.OFF, menu.ToggleObjects["A"]);
            // serialize順が保たれる (元のA,Bの位置のままキーだけ替わる)
            CollectionAssert.AreEqual(new[] { "B", "A" }, menu.ToggleObjects.Keys);
        }

        [Test]
        public void RenameFollowsAllDictionaries()
        {
            menu.ToggleBlendShapes[("A", "shape")] = new ToggleBlendShape();
            menu.Positions["A"] = new ToggleVector3();
            menu.SyncChildReferences(root);
            a.name = "A2";
            Assert.IsTrue(menu.SyncChildReferences(root));
            Assert.IsTrue(menu.ToggleBlendShapes.ContainsKey(("A2", "shape")));
            Assert.IsTrue(menu.Positions.ContainsKey("A2"));
            Assert.IsFalse(menu.ToggleBlendShapes.ContainsPrimaryKey("A"));
        }

        [Test]
        public void ChooseMenuRenameFollows()
        {
            var choose = new AvatarChooseMenu();
            choose.ChooseObjects["A"] = new IntHashSet { 0 };
            choose.ChooseBlendShapes[("A", "shape")] = new IntFloatDictionary();
            choose.SyncChildReferences(root);
            a.name = "A2";
            Assert.IsTrue(choose.SyncChildReferences(root));
            Assert.IsTrue(choose.ChooseObjects.ContainsKey("A2"));
            Assert.IsTrue(choose.ChooseBlendShapes.ContainsKey(("A2", "shape")));
        }

        [Test]
        public void RadialMenuRenameFollows()
        {
            var radial = new AvatarRadialMenu();
            radial.RadialBlendShapes[("A", "shape")] = new RadialBlendShape();
            radial.SyncChildReferences(root);
            a.name = "A2";
            Assert.IsTrue(radial.SyncChildReferences(root));
            Assert.IsTrue(radial.RadialBlendShapes.ContainsKey(("A2", "shape")));
        }

        [Test]
        public void RemovedChildReferenceIsPruned()
        {
            menu.SyncChildReferences(root);
            Assert.IsTrue(menu.ChildReferences.ContainsKey("A"));
            menu.RemoveStoredChild("A");
            menu.SyncChildReferences(root);
            Assert.IsFalse(menu.ChildReferences.ContainsKey("A"));
        }

        [Test]
        public void CollisionKeepsPath()
        {
            menu.SyncChildReferences(root);
            a.name = "B";
            Assert.IsFalse(menu.SyncChildReferences(root));
            Assert.IsTrue(menu.ToggleObjects.ContainsKey("A"));
            Assert.IsTrue(menu.ToggleObjects.ContainsKey("B"));
        }
    }
}
