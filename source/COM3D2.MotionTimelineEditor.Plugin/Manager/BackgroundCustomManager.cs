using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BackgroundCustomManager : ManagerBase
    {
        public class PartsData
        {
            public string Name { get; private set; }
            public string Menu { get; private set; } = "";

            public PartsData(string menu_, string name_)
            {
                this.Menu = menu_;
                this.Name = name_;
            }

            public PartsData(string text_)
            {
                string[] array = text_.Split(new char[]
                {
                    ','
                });
                this.Menu = array[0];
                this.Name = this.Menu;
                if (array.Length > 1)
                {
                    this.Name = array[1];
                }
            }
        }

        public class ManageObjectData
        {
            public string Name { get; private set; }
            public long ID { get; private set; }
            public string Menu { get; private set; } = "";

            public ManageObjectData(string menu_, long id_, string name_)
            {
                this.Menu = menu_;
                this.ID = id_;
                this.Name = name_;
            }

            public ManageObjectData(string text_)
            {
                string[] array = text_.Split(new char[]
                {
                    ','
                });
                this.Menu = array[0];
                this.Name = this.Menu;
                if (array.Length > 1)
                {
                    this.ID = (long)int.Parse(array[1]);
                }
                if (array.Length > 2)
                {
                    this.Name = array[2];
                }
            }
        }

        private BackgroundCustomWrapper _wrapper = null;
        private StudioExBackgroundCorrectorManagerWrapper _backgroundCorrectorWrapper = null;
        private Dictionary<string, PartsData> _partsDataMap = null;
        private Dictionary<string, ManageObjectData> _manageObjectDataMap = null;
        private ObjectManagerWindow _objectManagerWindow = null;

        private string categoryName => config.backgroundCustomCategoryName;

        private static BackgroundCustomManager _instance;
        public static BackgroundCustomManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BackgroundCustomManager();
                }

                return _instance;
            }
        }

        private BackgroundCustomManager()
        {
        }

        public override void Init()
        {
            _wrapper = new BackgroundCustomWrapper();
            _wrapper.Init();

            _backgroundCorrectorWrapper = new StudioExBackgroundCorrectorManagerWrapper();
            _backgroundCorrectorWrapper.Init();
        }

        public override void OnLoad()
        {
        }

        public override void OnPluginDisable()
        {
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            _objectManagerWindow = null;

            if (scene.name == "ScenePhotoMode")
            {
                _backgroundCorrectorWrapper.flgDoUpdateObjectIndexTable = true;
            }
        }

        public bool IsValid()
        {
            return _wrapper.IsValid();
        }

        public void RegisterObject(string name, string fileName)
        {
            if (!IsValid())
            {
                return;
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            LoadPartsData();
            LoadManageObjectData();

            var menuFileName = Path.GetFileNameWithoutExtension(fileName);

            var isNew = false;

            if (!_partsDataMap.ContainsKey(menuFileName))
            {
                var partsData = new PartsData(menuFileName, name);
                _partsDataMap[menuFileName] = partsData;
                isNew = true;
            }

            if (!_manageObjectDataMap.ContainsKey(menuFileName))
            {
                var id = (long) UnityEngine.Random.Range(int.MinValue, 0);
                var manageObjectData = new ManageObjectData(menuFileName, id, name);
                _manageObjectDataMap[menuFileName] = manageObjectData;
                isNew = true;
            }

            if (isNew)
            {
                SavePartsData();
                SaveManageObjectData();

                _wrapper.CreateCategory();
                _wrapper.CreateObjectCategory();

                UpdatePhotoBGObjectData();
            }
        }

        public void LoadPartsData()
        {
            if (_partsDataMap != null)
            {
                return;
            }

            try
            {
                _partsDataMap = new Dictionary<string, PartsData>(32);
                var filePath = PluginUtils.BackgroundCustomPartsCsvPath;
                if (!File.Exists(filePath))
                {
                    return;
                }

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        while (streamReader.Peek() >= 0)
                        {
                            var partsData = new PartsData(streamReader.ReadLine());
                            _partsDataMap[partsData.Menu] = partsData;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        public void SavePartsData()
        {
            try
            {
                var filePath = PluginUtils.BackgroundCustomPartsCsvPath;
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        foreach (var partsData in _partsDataMap.Values)
                        {
                            streamWriter.WriteLine($"{partsData.Menu},{partsData.Name}");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        public void LoadManageObjectData()
        {
            if (_manageObjectDataMap != null)
            {
                return;
            }

            try
            {
                _manageObjectDataMap = new Dictionary<string, ManageObjectData>(32);
                var filePath = PluginUtils.BackgroundCustomManageObjectCsvPath;
                if (!File.Exists(filePath))
                {
                    return;
                }

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        while (streamReader.Peek() >= 0)
                        {
                            var manageObjectData = new ManageObjectData(streamReader.ReadLine());
                            _manageObjectDataMap[manageObjectData.Menu] = manageObjectData;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        public void SaveManageObjectData()
        {
            try
            {
                var filePath = PluginUtils.BackgroundCustomManageObjectCsvPath;
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        foreach (var manageObjectData in _manageObjectDataMap.Values)
                        {
                            streamWriter.WriteLine($"{manageObjectData.Menu},{manageObjectData.ID},{manageObjectData.Name}");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        private FieldInfo _popupValueListField = null;

        public void UpdatePhotoBGObjectData()
        {
            if (_objectManagerWindow == null)
            {
                _objectManagerWindow = UnityEngine.Object.FindObjectsOfType<ObjectManagerWindow>().First<ObjectManagerWindow>();
            }

            if (_popupValueListField == null)
            {
                _popupValueListField = typeof(WindowPartsPopUpList).GetField("popup_value_list_", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _popupValueListField.SetValue(_objectManagerWindow.createBgObjectWindow.addButtonList.PopUpList, null);

            _objectManagerWindow.createBgObjectWindow.addButtonList.onClickEventList.Clear();

            foreach (var data in _manageObjectDataMap.Values)
            {
                AddManageObjectData(categoryName, data.Menu, data.Name, data.ID);
            }
            _objectManagerWindow.createBgObjectWindow.Initizalize(_objectManagerWindow);
        }

        public void AddManageObjectData(string category_, string menu_, string name_, long id_)
        {
            if (!PhotoBGObjectData.category_list.ContainsKey(category_))
            {
                PhotoBGObjectData.category_list.Add(category_, new List<PhotoBGObjectData>());
                PhotoBGObjectData.popup_category_list.Add(new KeyValuePair<string, UnityEngine.Object>(category_, null));
                PhotoBGObjectData.popup_category_term_list.Add("ScenePhotoMode/背景オブジェクト/カテゴリー/" + category_);
            }

            if (PhotoBGObjectData.category_list[category_].Any(x => x.id == id_))
            {
                return;
            }

            var data = Activator.CreateInstance(typeof(PhotoBGObjectData), true) as PhotoBGObjectData;
            data.category = category_;
            data.create_asset_bundle_name = "";
            data.create_prefab_name = menu_;
            data.id = id_;
            data.name = name_;
            PhotoBGObjectData.category_list[category_].Add(data);
            PhotoBGObjectData.data.Add(data);

            _backgroundCorrectorWrapper.flgDoUpdateObjectIndexTable = true;

            modelManager.RegisterPhotoBGObject(data);
        }

    }
}