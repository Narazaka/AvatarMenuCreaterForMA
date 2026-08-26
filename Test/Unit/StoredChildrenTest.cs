using System.Linq;
using NUnit.Framework;
using UnityEngine;
using net.narazaka.avatarmenucreator.collections.instance;

namespace net.narazaka.avatarmenucreator.test
{
    public class StoredChildrenTest
    {
        AvatarToggleMenu ToggleMenu()
        {
            var menu = new AvatarToggleMenu();
            menu.ToggleObjects["A"] = ToggleType.ON;
            menu.ToggleBlendShapes[("A", "shape")] = new ToggleBlendShape();
            menu.Positions["A"] = new ToggleVector3();
            menu.ToggleObjects["B"] = ToggleType.OFF;
            return menu;
        }

        [Test]
        public void ToggleReplaceStoredChildMovesAllStores()
        {
            var menu = ToggleMenu();
            menu.ReplaceStoredChild("A", "C");
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, menu.GetStoredChildren().ToArray());
            Assert.IsTrue(menu.ToggleBlendShapes.ContainsKey(("C", "shape")));
            Assert.IsTrue(menu.Positions.ContainsKey("C"));
            Assert.IsFalse(menu.ToggleObjects.ContainsKey("A"));
        }

        [Test]
        public void ToggleRemoveStoredChildRemovesAllStores()
        {
            var menu = ToggleMenu();
            menu.RemoveStoredChild("A");
            CollectionAssert.AreEquivalent(new[] { "B" }, menu.GetStoredChildren().ToArray());
        }

        [Test]
        public void ToggleFilterStoredTargetsKeepsOnlyGivenChildren()
        {
            var menu = ToggleMenu();
            menu.FilterStoredTargets(new[] { "A" });
            CollectionAssert.AreEquivalent(new[] { "A" }, menu.GetStoredChildren().ToArray());
            Assert.IsTrue(menu.ToggleBlendShapes.ContainsKey(("A", "shape")));
        }

        [Test]
        public void ChooseReplaceStoredChildMovesAllStores()
        {
            var menu = new AvatarChooseMenu();
            var indexes = new IntHashSet();
            indexes.Add(0);
            menu.ChooseObjects["A"] = indexes;
            menu.ChooseBlendShapes[("A", "shape")] = new IntFloatDictionary();
            menu.ReplaceStoredChild("A", "C");
            CollectionAssert.AreEquivalent(new[] { "C" }, menu.GetStoredChildren().ToArray());
            Assert.IsTrue(menu.ChooseBlendShapes.ContainsKey(("C", "shape")));
        }

        [Test]
        public void RadialReplaceStoredChildMovesAllStores()
        {
            var menu = new AvatarRadialMenu();
            menu.RadialBlendShapes[("A", "shape")] = new RadialBlendShape();
            menu.Positions["A"] = new RadialVector3();
            menu.ReplaceStoredChild("A", "C");
            CollectionAssert.AreEquivalent(new[] { "C" }, menu.GetStoredChildren().ToArray());
            Assert.IsTrue(menu.RadialBlendShapes.ContainsKey(("C", "shape")));
            Assert.IsTrue(menu.Positions.ContainsKey("C"));
        }
    }
}
