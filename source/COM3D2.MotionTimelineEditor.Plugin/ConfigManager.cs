using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ConfigManager : ManagerBase
    {
        private Config _config = new Config();
        public override Config config => _config;

        private static ConfigManager _instance = null;
        public static ConfigManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                }
                return _instance;
            }
        }

        private bool _isLoaded = false;

        private ConfigManager()
        {
        }

        public override void Init()
        {
            if (!_isLoaded)
            {
                LoadConfigXml();
                SaveConfigXml();
            }
        }

        public override void Update()
        {
            if (config.dirty && Input.GetMouseButtonUp(0))
            {
                SaveConfigXml();
            }
        }

        public void LoadConfigXml()
        {
            try
            {
                var path = PluginUtils.ConfigPath;
                if (!File.Exists(path))
                {
                    return;
                }

                var serializer = new XmlSerializer(typeof(Config));
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    _config = (Config)serializer.Deserialize(stream);
                    _config.ConvertVersion();
                }

                _isLoaded = true;
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public void SaveConfigXml()
        {
            PluginUtils.Log("設定保存中...");
            try
            {
                // サブウィンドウの情報を更新
                windowManager.UpdateConfig();
                config.dirty = false;

                var path = PluginUtils.ConfigPath;
                var serializer = new XmlSerializer(typeof(Config));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(stream, config);
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public void ResetConfig()
        {
            _config = new Config();
            SaveConfigXml();
        }
    }
}