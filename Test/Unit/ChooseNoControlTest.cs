using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using net.narazaka.avatarmenucreator.collections.instance;
using net.narazaka.avatarmenucreator.editor;
using net.narazaka.avatarmenucreator.util;

namespace net.narazaka.avatarmenucreator.test
{
    public class ChooseNoControlTest
    {
        static IntHashSet Set(params int[] values)
        {
            var set = new IntHashSet();
            foreach (var value in values) set.Add(value);
            return set;
        }

        static AvatarChooseMenu NewMenu() => new AvatarChooseMenu { ChooseCount = 3 };

        static AnimationClip[] Clips(AvatarChooseMenu menu)
        {
            var root = new GameObject("ChooseNoControlTestRoot");
            try
            {
                var assets = new CreateAvatarChooseMenu(root.transform, menu).CreateAssets("Test");
                return Enumerable.Range(0, menu.ChooseCount).Select(i => assets.Clips.First(c => c.name == $"Test_{i}")).ToArray();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static bool HasCurve(AnimationClip clip, string path, System.Type type, string propertyPrefix) =>
            AnimationUtility.GetCurveBindings(clip).Any(b => b.path == path && b.type == type && b.propertyName.StartsWith(propertyPrefix));

        static bool HasObjectReferenceCurve(AnimationClip clip, string path, string propertyName) =>
            AnimationUtility.GetObjectReferenceCurveBindings(clip).Any(b => b.path == path && b.propertyName == propertyName);

        [Test]
        public void ObjectNoControlSkipsCurve()
        {
            var menu = NewMenu();
            menu.ChooseObjects["A"] = Set(0);
            menu.ChooseObjectNoControls["A"] = Set(2);
            var clips = Clips(menu);
            Assert.IsTrue(HasCurve(clips[0], "A", typeof(GameObject), "m_IsActive"));
            Assert.IsTrue(HasCurve(clips[1], "A", typeof(GameObject), "m_IsActive"));
            Assert.IsFalse(HasCurve(clips[2], "A", typeof(GameObject), "m_IsActive"));
        }

        [Test]
        public void BlendShapeNoControlSkipsCurve()
        {
            var menu = NewMenu();
            menu.ChooseBlendShapes[("A", "shape")] = new IntFloatDictionary { [0] = 100f };
            menu.ChooseBlendShapeNoControls[("A", "shape")] = Set(2);
            var clips = Clips(menu);
            Assert.IsTrue(HasCurve(clips[0], "A", typeof(SkinnedMeshRenderer), "blendShape.shape"));
            Assert.IsTrue(HasCurve(clips[1], "A", typeof(SkinnedMeshRenderer), "blendShape.shape"));
            Assert.IsFalse(HasCurve(clips[2], "A", typeof(SkinnedMeshRenderer), "blendShape.shape"));
        }

        [Test]
        public void ShaderParameterNoControlSkipsCurve()
        {
            var menu = NewMenu();
            menu.ChooseShaderParameters[("A", "_P")] = new IntFloatDictionary { [0] = 1f };
            menu.ChooseShaderParameterNoControls[("A", "_P")] = Set(2);
            var clips = Clips(menu);
            Assert.IsTrue(HasCurve(clips[0], "A", typeof(Renderer), "material._P"));
            Assert.IsTrue(HasCurve(clips[1], "A", typeof(Renderer), "material._P"));
            Assert.IsFalse(HasCurve(clips[2], "A", typeof(Renderer), "material._P"));
        }

        [Test]
        public void ShaderVectorParameterNoControlSkipsCurve()
        {
            var menu = NewMenu();
            menu.ChooseShaderVectorParameters[("A", "_V")] = new IntVector4Dictionary { [0] = Vector4.one };
            menu.ChooseShaderVectorParameterNoControls[("A", "_V")] = Set(2);
            var clips = Clips(menu);
            Assert.IsTrue(HasCurve(clips[0], "A", typeof(Renderer), "material._V"));
            Assert.IsTrue(HasCurve(clips[1], "A", typeof(Renderer), "material._V"));
            Assert.IsFalse(HasCurve(clips[2], "A", typeof(Renderer), "material._V"));
        }

        [Test]
        public void MaterialNoControlSkipsCurve()
        {
            var menu = NewMenu();
            menu.ChooseMaterials[("A", 0)] = new IntMaterialDictionary();
            menu.ChooseMaterialNoControls[("A", 0)] = Set(2);
            var clips = Clips(menu);
            Assert.IsTrue(HasObjectReferenceCurve(clips[0], "A", "m_Materials.Array.data[0]"));
            Assert.IsTrue(HasObjectReferenceCurve(clips[1], "A", "m_Materials.Array.data[0]"));
            Assert.IsFalse(HasObjectReferenceCurve(clips[2], "A", "m_Materials.Array.data[0]"));
        }

        [Test]
        public void ValueNoControlSkipsCurve()
        {
            var menu = NewMenu();
            var member = new TypeMember(typeof(AudioSource), "volume");
            menu.ChooseValues[("A", member)] = new IntValueDictionary { [0] = 0.5f };
            menu.ChooseValueNoControls[("A", member)] = Set(2);
            var clips = Clips(menu);
            Assert.IsTrue(HasCurve(clips[0], "A", typeof(AudioSource), "m_Volume"));
            Assert.IsTrue(HasCurve(clips[1], "A", typeof(AudioSource), "m_Volume"));
            Assert.IsFalse(HasCurve(clips[2], "A", typeof(AudioSource), "m_Volume"));
        }

        [Test]
        public void TransformNoControlSkipsCurve()
        {
            var menu = NewMenu();
            menu.Positions["A"] = new IntVector3Dictionary { [0] = Vector3.one };
            menu.TransformNoControls[("A", "Position")] = Set(2);
            var clips = Clips(menu);
            Assert.IsTrue(HasCurve(clips[0], "A", typeof(Transform), "m_LocalPosition") || HasCurve(clips[0], "A", typeof(Transform), "localPosition"));
            Assert.IsFalse(HasCurve(clips[2], "A", typeof(Transform), "m_LocalPosition") || HasCurve(clips[2], "A", typeof(Transform), "localPosition"));
        }

        static AvatarChooseMenu MenuWithAllNoControls()
        {
            var menu = NewMenu();
            menu.ChooseObjects["A"] = Set(0);
            menu.ChooseObjectNoControls["A"] = Set(2);
            menu.ChooseBlendShapes[("A", "shape")] = new IntFloatDictionary { [0] = 100f };
            menu.ChooseBlendShapeNoControls[("A", "shape")] = Set(2);
            menu.ChooseShaderParameters[("A", "_P")] = new IntFloatDictionary { [0] = 1f };
            menu.ChooseShaderParameterNoControls[("A", "_P")] = Set(2);
            menu.ChooseShaderVectorParameters[("A", "_V")] = new IntVector4Dictionary { [0] = Vector4.one };
            menu.ChooseShaderVectorParameterNoControls[("A", "_V")] = Set(2);
            menu.ChooseMaterials[("A", 0)] = new IntMaterialDictionary();
            menu.ChooseMaterialNoControls[("A", 0)] = Set(2);
            menu.ChooseValues[("A", new TypeMember(typeof(AudioSource), "volume"))] = new IntValueDictionary { [0] = 0.5f };
            menu.ChooseValueNoControls[("A", new TypeMember(typeof(AudioSource), "volume"))] = Set(2);
            menu.Positions["A"] = new IntVector3Dictionary { [0] = Vector3.one };
            menu.TransformNoControls[("A", "Position")] = Set(2);
            return menu;
        }

        [Test]
        public void ReplaceStoredChildMovesNoControls()
        {
            var menu = MenuWithAllNoControls();
            menu.ReplaceStoredChild("A", "C");
            Assert.IsTrue(menu.ChooseObjectNoControls.ContainsKey("C"));
            Assert.IsTrue(menu.ChooseBlendShapeNoControls.ContainsKey(("C", "shape")));
            Assert.IsTrue(menu.ChooseShaderParameterNoControls.ContainsKey(("C", "_P")));
            Assert.IsTrue(menu.ChooseShaderVectorParameterNoControls.ContainsKey(("C", "_V")));
            Assert.IsTrue(menu.ChooseMaterialNoControls.ContainsKey(("C", 0)));
            Assert.IsTrue(menu.ChooseValueNoControls.ContainsKey(("C", new TypeMember(typeof(AudioSource), "volume"))));
            Assert.IsTrue(menu.TransformNoControls.ContainsKey(("C", "Position")));
            Assert.IsFalse(menu.ChooseObjectNoControls.ContainsKey("A"));
        }

        [Test]
        public void RemoveStoredChildRemovesNoControls()
        {
            var menu = MenuWithAllNoControls();
            menu.RemoveStoredChild("A");
            Assert.AreEqual(0, menu.ChooseObjectNoControls.Count);
            Assert.AreEqual(0, menu.ChooseBlendShapeNoControls.Count);
            Assert.AreEqual(0, menu.ChooseShaderParameterNoControls.Count);
            Assert.AreEqual(0, menu.ChooseShaderVectorParameterNoControls.Count);
            Assert.AreEqual(0, menu.ChooseMaterialNoControls.Count);
            Assert.AreEqual(0, menu.ChooseValueNoControls.Count);
            Assert.AreEqual(0, menu.TransformNoControls.Count);
        }

        [Test]
        public void FilterStoredTargetsFiltersNoControls()
        {
            var menu = MenuWithAllNoControls();
            menu.ChooseObjectNoControls["B"] = Set(1);
            menu.FilterStoredTargets(new[] { "B" });
            Assert.IsFalse(menu.ChooseObjectNoControls.ContainsKey("A"));
            Assert.IsTrue(menu.ChooseObjectNoControls.ContainsKey("B"));
            Assert.AreEqual(0, menu.ChooseBlendShapeNoControls.Count);
            Assert.AreEqual(0, menu.ChooseMaterialNoControls.Count);
        }

        [Test]
        public void RestoreRestoresNoControls()
        {
            var menu = NewMenu();
            menu.ChooseObjects["A"] = Set(0);
            menu.ChooseObjectNoControls["A"] = Set(2);
            menu.ChooseBlendShapes[("A", "shape")] = new IntFloatDictionary { [0] = 100f };
            menu.ChooseBlendShapeNoControls[("A", "shape")] = Set(2);
            menu.ChooseShaderParameters[("A", "_P")] = new IntFloatDictionary { [0] = 1f };
            menu.ChooseShaderParameterNoControls[("A", "_P")] = Set(2);
            menu.ChooseMaterials[("A", 0)] = new IntMaterialDictionary();
            menu.ChooseMaterialNoControls[("A", 0)] = Set(2);

            var root = new GameObject("ChooseNoControlTestRoot");
            try
            {
                var assets = new CreateAvatarChooseMenu(root.transform, menu).CreateAssets("Test");
                var restored = new RestoreAvatarChooseMenu(assets.Parameters.First(), assets.Controller, assets.ParentMenu).RestoreAssets();
                CollectionAssert.AreEquivalent(new[] { 0 }, restored.ChooseObjects["A"].ToArray());
                CollectionAssert.AreEquivalent(new[] { 2 }, restored.ChooseObjectNoControls["A"].ToArray());
                CollectionAssert.AreEquivalent(new[] { 2 }, restored.ChooseBlendShapeNoControls[("A", "shape")].ToArray());
                CollectionAssert.AreEquivalent(new[] { 2 }, restored.ChooseShaderParameterNoControls[("A", "_P")].ToArray());
                CollectionAssert.AreEquivalent(new[] { 2 }, restored.ChooseMaterialNoControls[("A", 0)].ToArray());
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
