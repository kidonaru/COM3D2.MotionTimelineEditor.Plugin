using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;
using System.Collections.Generic;
using System.Collections;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginUtils.PluginFullName),
        PluginVersion(PluginUtils.PluginVersion)
    ]
    public class MotionTimelineEditor : PluginBase
    {
        private bool _isEnable = false;
        public bool isEnable
        {
            get
            {
                return _isEnable;
            }
            set
            {
                if (_isEnable == value)
                {
                    return;
                }

                _isEnable = value;
                UpdateGearMenu();

                if (value)
                {
                    OnPluginEnable();
                }
                else
                {
                    OnPluginDisable();
                }

                if (value)
                {
                    isVisible = true;
                }
            }
        }

        public bool isVisible;

        private List<StudioHackBase> studioHacks = new List<StudioHackBase>();
        private List<StudioHackBase> activeStudioHacks = new List<StudioHackBase>();

        public static MotionTimelineEditor instance { get; private set; }

        private static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        private static Maid maid
        {
            get
            {
                return maidManager.maid;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        protected static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        private static TimelineHistoryManager historyManager
        {
            get
            {
                return TimelineHistoryManager.instance;
            }
        }

        private static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        private static BGMManager bgmManager
        {
            get
            {
                return BGMManager.instance;
            }
        }

        private static MovieManager movieManager
        {
            get
            {
                return MovieManager.instance;
            }
        }

        public static Rect rc_stgw
        {
            get
            {
                return MainWindow.rc_stgw;
            }
        }

        public static MainWindow mainWindow = new MainWindow();
        public static SubWindow subWindow = new SubWindow();
        public static Config config = new Config();
        public static StudioHackBase studioHack = null;

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        private static ITimelineLayer currentLayer
        {
            get
            {
                return timelineManager.currentLayer;
            }
        }

        public void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
            instance = this;
        }

        public void Start()
        {
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public void Update()
        {
            try
            {
                if (!config.pluginEnabled)
                {
                    return;
                }

                UpdateStudioHack();

                if (studioHack == null)
                {
                    return;
                }

                if (config.GetKeyDown(KeyBindType.PluginToggle))
                {
                    isEnable = !isEnable;
                }

                if (isEnable)
                {
                    if (config.GetKeyDown(KeyBindType.Visible))
                    {
                        isVisible = !isVisible;
                    }

                    if (!studioHack.IsValid())
                    {
                        return;
                    }

                    maidManager.Update();

                    if (maid == null)
                    {
                        return;
                    }
                    if (!timelineManager.IsValidData())
                    {
                        return;
                    }

                    if (config.GetKeyDown(KeyBindType.AddKeyFrame))
                    {
                        currentLayer.AddKeyFrameDiff();
                    }
                    if (config.GetKeyDown(KeyBindType.RemoveKeyFrame))
                    {
                        timelineManager.RemoveSelectedFrame();
                    }
                    if (config.GetKeyDownRepeat(KeyBindType.PrevFrame))
                    {
                        timelineManager.SeekCurrentFrame(timelineManager.currentFrameNo - 1);
                        mainWindow.FixScrollPosition();
                    }
                    if (config.GetKeyDownRepeat(KeyBindType.NextFrame))
                    {
                        timelineManager.SeekCurrentFrame(timelineManager.currentFrameNo + 1);
                        mainWindow.FixScrollPosition();
                    }
                    if (config.GetKeyDownRepeat(KeyBindType.PrevKeyFrame))
                    {
                        var prevFrame = timelineManager.GetPrevFrame(timelineManager.currentFrameNo);
                        if (prevFrame != null)
                        {
                            timelineManager.SeekCurrentFrame(prevFrame.frameNo);
                            mainWindow.FixScrollPosition();
                        }
                    }
                    if (config.GetKeyDownRepeat(KeyBindType.NextKeyFrame))
                    {
                        var nextFrame = timelineManager.GetNextFrame(timelineManager.currentFrameNo);
                        if (nextFrame != null)
                        {
                            timelineManager.SeekCurrentFrame(nextFrame.frameNo);
                            mainWindow.FixScrollPosition();
                        }
                    }
                    if (config.GetKeyDown(KeyBindType.Play))
                    {
                        if (currentLayer.isAnmPlaying)
                        {
                            timelineManager.Stop();
                        }
                        else
                        {
                            timelineManager.Play();
                        }
                    }
                    if (config.GetKeyDown(KeyBindType.EditMode))
                    {
                        studioHack.isPoseEditing = !studioHack.isPoseEditing;
                    }
                    if (config.GetKeyDown(KeyBindType.Copy))
                    {
                        timelineManager.CopyFramesToClipboard();
                    }
                    if (config.GetKeyDown(KeyBindType.Paste))
                    {
                        timelineManager.PasteFramesFromClipboard(false);
                    }
                    if (config.GetKeyDown(KeyBindType.FlipPaste))
                    {
                        timelineManager.PasteFramesFromClipboard(true);
                    }
                    if (config.GetKeyDown(KeyBindType.PoseCopy))
                    {
                        timelineManager.CopyPoseToClipboard();
                    }
                    if (config.GetKeyDown(KeyBindType.PosePaste))
                    {
                        timelineManager.PastePoseFromClipboard();
                    }
                    if (config.GetKeyDown(KeyBindType.Undo))
                    {
                        historyManager.Undo();
                    }
                    if (config.GetKeyDown(KeyBindType.Redo))
                    {
                        historyManager.Redo();
                    }

                    mainWindow.isMultiSelect = config.GetKey(KeyBindType.MultiSelect);

                    studioHack.Update();
                    timelineManager.Update();
                    mainWindow.Update();
                    subWindow.Update();
                    bgmManager.Update();
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        private void UpdateStudioHack()
        {
            if (studioHacks.Count == 0)
            {
                return;
            }

            studioHack = null;
            activeStudioHacks.Clear();

            foreach (var hack in studioHacks)
            {
                if (hack.isSceneActive)
                {
                    activeStudioHacks.Add(hack);
                }
            }

            foreach (var hack in activeStudioHacks)
            {
                if (hack.IsValid())
                {
                    studioHack = hack;
                    break;
                }
            }

            if (studioHack == null && activeStudioHacks.Count > 0)
            {
                studioHack = activeStudioHacks[0];
            }
        }

        public void LateUpdate()
        {
            try
            {
                if (!config.pluginEnabled)
                {
                    return;
                }
                if (studioHack == null || maid == null)
                {
                    return;
                }
                if (!studioHack.IsValid())
                {
                    return;
                }

                if (isEnable)
                {
                    subWindow.LateUpdate();

                    if (!timelineManager.IsValidData())
                    {
                        return;
                    }

                    modelManager.LateUpdate(false);
                    timelineManager.LateUpdate();
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            try
            {
                if (!config.pluginEnabled)
                {
                    return;
                }

                if (sceneName.name == "SceneTitle")
                {
                    this.isEnable = false;
                }

                foreach (var studioHack in studioHacks)
                {
                    studioHack.OnChangedSceneLevel(sceneName, sceneMode);
                }

                BlendShapeLoader.ClearCache();
                ModMenuLoader.ClearCache();
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        void OnApplicationQuit()
        {
            SaveConfigXml();
        }

        public void OnRefreshTimeline()
        {
            mainWindow.UpdateTexture();
        }

        public static void LoadConfigXml()
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

        public static void SaveConfigXml()
        {
            config.dirty = false;

            PluginUtils.Log("設定保存中...");
            try
            {
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

        public static void ResetConfig()
        {
            config = new Config();
            SaveConfigXml();
        }

        private void Initialize()
        {
            try
            {
                PluginUtils.Log("初期化中...");

                LoadConfigXml();
                SaveConfigXml();

                if (!config.pluginEnabled)
                {
                    PluginUtils.Log("プラグインが無効になっています");
                    return;
                }

                SceneManager.sceneLoaded += OnChangedSceneLevel;
                TimelineManager.onRefresh += OnRefreshTimeline;

                AddStudioHack(new StudioHack());

                timelineManager.RegisterLayer(
                    typeof(MotionTimelineLayer), MotionTimelineLayer.Create
                );

                mainWindow.Init();
                subWindow.Init();
                boneMenuManager.Init();
                bgmManager.Init();
                movieManager.Init();

                AddGearMenu();
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        GameObject gearMenuIcon = null;

        public void AddGearMenu()
        {
            gearMenuIcon = GUIExtBase.GUIExt.Add(
                PluginUtils.PluginName,
                PluginUtils.PluginName,
                PluginUtils.Icon,
                (go) => {
                    if (studioHack == null)
                    {
                        return;
                    }

                    isEnable = !isEnable;
                });
        }

        public void RemoveGearMenu()
        {
            if (gearMenuIcon != null)
            {
                GUIExtBase.GUIExt.Destroy(gearMenuIcon);
                gearMenuIcon = null;
            }
        }

        private void UpdateGearMenu()
        {
            if (gearMenuIcon != null)
            {
                GUIExtBase.GUIExt.SetFrameColor(gearMenuIcon, isEnable ? Color.blue : Color.white);
            }
        }

        public void AddStudioHack(StudioHackBase studioHack)
        {
            if (studioHack == null || !studioHack.Init())
            {
                return;
            }

            studioHacks.Add(studioHack);
            studioHacks.Sort((a, b) => b.priority - a.priority);
        }

        public void SaveScreenShot(string filePath, int width, int height)
        {
            StartCoroutine(SaveScreenShotInternal(filePath, width, height));
        }

        private IEnumerator SaveScreenShotInternal(string filePath, int width, int height)
        {
            PluginUtils.UIHide();
            var subWindowType = subWindow.subWindowType;
            isVisible = false;
            yield return new WaitForEndOfFrame();
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            yield return new WaitForEndOfFrame();
            isVisible = true;
            subWindow.subWindowType = subWindowType;
            PluginUtils.UIResume();

            texture.ResizeTexture(width, height);
            UTY.SaveImage(texture, filePath);

            UnityEngine.Object.Destroy(texture);

            yield break;
        }

        public void OnGUI()
        {
            try
            {
                if (isEnable && isVisible)
                {
                    mainWindow.OnGUI();
                    subWindow.OnGUI();
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        private void OnPluginEnable()
        {
            UpdateStudioHack();

            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            maidManager.OnPluginEnable();
            modelManager.OnPluginEnable();
            movieManager.OnPluginEnable();
            timelineManager.OnPluginEnable();
        }

        private void OnPluginDisable()
        {
            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            maidManager.OnPluginDisable();
            modelManager.OnPluginDisable();
            movieManager.OnPluginDisable();
            timelineManager.OnPluginDisable();
        }
    }
}