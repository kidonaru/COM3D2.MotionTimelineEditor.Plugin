using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ConfigManager
    {
        public static Config config = new Config();

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

        public static WindowManager windowManager
        {
            get
            {
                return WindowManager.instance;
            }
        }

        private ConfigManager()
        {
        }

        public void Init()
        {
            LoadConfigXml();
            SaveConfigXml();
        }

        public void Update()
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
                    config = (Config)serializer.Deserialize(stream);
                    config.ConvertVersion();
                }
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
            config = new Config();
            SaveConfigXml();
        }
    }
}