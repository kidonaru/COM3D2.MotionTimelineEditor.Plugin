using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineBundleManager
    {
        public static readonly string AssetBundleName = "mte_bundle";
        public static readonly string ShaderBasePath = "Assets/Shaders/MTE/";

        public static TimelineBundleManager _instance = null;
        public static TimelineBundleManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimelineBundleManager();
                }
                return _instance;
            }
        }

        private AssetBundle _assetBundle = null;

        public TimelineBundleManager()
        {
            LoadAssetBundle();
        }

        public bool IsValid()
        {
            return _assetBundle != null;
        }

        private Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();

        public Material LoadMaterial(string shaderName)
        {
            if (!IsValid())
            {
                return null;
            }

            Material material;
            if (_materialCache.TryGetValue(shaderName, out material))
            {
                return new Material(material);
            }

            var path = ShaderBasePath + shaderName + ".mat";
            material = _assetBundle.LoadAsset<Material>(path);
            if (material == null)
            {
                PluginUtils.LogError("マテリアルが見つかりません: {0}", path);
                return null;
            }

            _materialCache.Add(shaderName, material);

            return new Material(material);
        }

        private void LoadAssetBundle()
        {
            if (_assetBundle != null)
            {
                _assetBundle.Unload(true);
                _assetBundle = null;
            }

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(AssetBundleName))
            {
                if (stream == null)
                {
                    PluginUtils.LogError("アセットバンドルが見つかりません: {0}", AssetBundleName);
                    return;
                }

                byte[] binary = new byte[stream.Length];
                stream.Read(binary, 0, binary.Length);
                _assetBundle = AssetBundle.LoadFromMemory(binary);
            }

            if (_assetBundle == null)
            {
                PluginUtils.LogError("アセットバンドルのロードに失敗しました: {0}", AssetBundleName);
                return;
            }
        }
    }
}