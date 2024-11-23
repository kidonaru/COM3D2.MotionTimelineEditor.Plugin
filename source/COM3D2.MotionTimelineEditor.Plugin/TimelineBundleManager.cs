using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineBundleManager
    {
        private static readonly string AssetBundleName = "mte_bundle";
        private static readonly string ShaderBasePath = "Assets/Shaders/";
        private static readonly string ResoucesBasePath = "Assets/Resources/";

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

        private Texture2D _lockIcon = null;
        public Texture2D lockIcon
        {
            get
            {
                if (_lockIcon == null) _lockIcon = LoadTexture("lock_icon");
                return _lockIcon;
            }
        }

        private Texture2D _unlockIcon = null;
        public Texture2D unlockIcon
        {
            get
            {
                if (_unlockIcon == null) _unlockIcon = LoadTexture("unlock_icon");
                return _unlockIcon;
            }
        }

        private Texture2D _changeIcon = null;
        public Texture2D changeIcon
        {
            get
            {
                if (_changeIcon == null) _changeIcon = LoadTexture("change_icon");
                return _changeIcon;
            }
        }

        private Texture2D _updateIcon = null;
        public Texture2D updateIcon
        {
            get
            {
                if (_updateIcon == null) _updateIcon = LoadTexture("update_icon");
                return _updateIcon;
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

        public Texture2D LoadTexture(string textureName)
        {
            if (!IsValid())
            {
                return null;
            }

            var path = ResoucesBasePath + textureName + ".png";
            var texture = _assetBundle.LoadAsset<Texture2D>(path);
            if (texture == null)
            {
                PluginUtils.LogError("テクスチャが見つかりません: {0}", path);
                return null;
            }

            return texture;
        }

        public Byte[] LoadBytes(string bytesName)
        {
            if (!IsValid())
            {
                return null;
            }

            var path = ResoucesBasePath + bytesName + ".bytes";
            var bytes = _assetBundle.LoadAsset<TextAsset>(path).bytes;
            if (bytes == null)
            {
                PluginUtils.LogError("バイナリが見つかりません: {0}", path);
                return null;
            }

            return bytes;
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