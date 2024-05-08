using NUnit.Framework;
using UnityEditor;

namespace net.narazaka.avatarmenucreator.test
{
    /// <summary>
    /// This tests compilation error in runtime build with building asset bundle.
    /// 
    /// cf. https://github.com/bdunderscore/ndmf/blob/main/UnitTests~/BuildAssetBundle.cs
    /// </summary>
    public class BuildAssetBundle
    {
        [Test]
        public void Build()
        {
            BuildPipeline.BuildAssetBundles("Packages/net.narazaka.vrchat.avatar-menu-creater-for-ma/Test/gen~/",
                new[]
                {
                    new AssetBundleBuild
                    {
                        assetNames = new[] { "Packages/net.narazaka.vrchat.avatar-menu-creater-for-ma/Test/Empty.prefab" },
                        assetBundleName = "asset.unity3d"
                    }
                },
                BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget
                );
        }
    }
}
