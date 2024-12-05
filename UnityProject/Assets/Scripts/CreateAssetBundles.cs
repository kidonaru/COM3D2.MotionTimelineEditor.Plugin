#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build Bundle")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/Bundles";
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);
        AssetDatabase.Refresh();

        Debug.Log("Asset bundles created at " + assetBundleDirectory);
    }
}
#endif
