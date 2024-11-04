using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;
using System.Collections;
using System.Diagnostics;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginInfo.PluginFullName),
        PluginVersion(PluginInfo.PluginVersion)
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

        private static TimelineLoadManager timelineLoadManager
        {
            get
            {
                return TimelineLoadManager.instance;
            }
        }

        protected static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        protected static StudioLightManager lightManager
        {
            get
            {
                return StudioLightManager.instance;
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

        private static StudioHackManager studioHackManager
        {
            get
            {
                return StudioHackManager.instance;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private static ModelHackManager modelHackManager
        {
            get
            {
                return ModelHackManager.instance;
            }
        }

        private static WindowManager windowManager
        {
            get
            {
                return WindowManager.instance;
            }
        }

        private static ConfigManager configManager
        {
            get
            {
                return ConfigManager.instance;
            }
        }

        private static CameraManager cameraManager
        {
            get
            {
                return CameraManager.instance;
            }
        }

        private static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        private static ITimelineLayer currentLayer
        {
            get
            {
                return timelineManager.currentLayer;
            }
        }

        private static MainWindow mainWindow
        {
            get
            {
                return windowManager.mainWindow;
            }
        }

        private static GridViewManager gridViewManager
        {
            get
            {
                return GridViewManager.instance;
            }
        }

        private static PostEffectManager postEffectManager
        {
            get
            {
                return PostEffectManager.instance;
            }
        }

        public MotionTimelineEditor()
        {
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

                studioHackManager.Update();
                modelHackManager.Update();
                windowManager.Update();

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
                    bgmManager.Update();
                    configManager.Update();
                    cameraManager.Update();
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
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
                    if (!timelineManager.IsValidData())
                    {
                        return;
                    }

                    maidManager.LateUpdate();
                    modelManager.LateUpdate(false);
                    lightManager.LateUpdate(false);
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

                BinaryLoader.ClearCache();
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
            configManager.SaveConfigXml();
        }

        private void Initialize()
        {
            try
            {
                PluginUtils.Log("初期化中...");

                configManager.Init();

                if (!config.pluginEnabled)
                {
                    PluginUtils.Log("プラグインが無効になっています");
                    return;
                }

                SceneManager.sceneLoaded += OnChangedSceneLevel;

                studioHackManager.Register(new StudioHack());

                timelineManager.RegisterLayer(
                    typeof(MotionTimelineLayer), MotionTimelineLayer.Create
                );

                windowManager.Init();
                boneMenuManager.Init();
                bgmManager.Init();
                movieManager.Init();
                timelineLoadManager.Init();
                gridViewManager.Init();
                postEffectManager.Init();
                cameraManager.Init();

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
                PluginInfo.PluginName,
                PluginInfo.PluginName,
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

        public void SaveScreenShot(string filePath, int width, int height)
        {
            StartCoroutine(SaveScreenShotInternal(filePath, width, height));
        }

        private IEnumerator SaveScreenShotInternal(string filePath, int width, int height)
        {
            PluginUtils.UIHide();
            isVisible = false;
            yield return new WaitForEndOfFrame();
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            yield return new WaitForEndOfFrame();
            isVisible = true;
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
                    windowManager.OnGUI();
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        private void OnPluginEnable()
        {
            DumpAllCameraInfo();
            DumpLayerInfo();

            studioHackManager.Update();
            modelHackManager.Update();

            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            DumpDoFInfo();

            maidManager.OnPluginEnable();
            modelManager.OnPluginEnable();
            lightManager.OnPluginEnable();
            movieManager.OnPluginEnable();
            timelineManager.OnPluginEnable();
            gridViewManager.OnPluginEnable();
            postEffectManager.OnPluginEnable();
            cameraManager.OnPluginEnable();
        }

        private void OnPluginDisable()
        {
            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            maidManager.OnPluginDisable();
            modelManager.OnPluginDisable();
            lightManager.OnPluginDisable();
            movieManager.OnPluginDisable();
            timelineManager.OnPluginDisable();
            gridViewManager.OnPluginDisable();
            postEffectManager.OnPluginDisable();
            cameraManager.OnPluginDisable();
        }

        [Conditional("DEBUG")]
        private void DumpAllCameraInfo()
        {
            Camera[] allCameras = Camera.allCameras;
            PluginUtils.LogDebug("カメラの総数: " + allCameras.Length);

            for (int i = 0; i < allCameras.Length; i++)
            {
                Camera cam = allCameras[i];
                PluginUtils.LogDebug("カメラ " + (i + 1) + ":");
                PluginUtils.LogDebug("  名前: " + cam.name);
                PluginUtils.LogDebug("  有効: " + cam.enabled);
                PluginUtils.LogDebug("  平行投影: " + cam.orthographic);
                PluginUtils.LogDebug("  平行投影サイズ: " + cam.orthographicSize);
                PluginUtils.LogDebug("  位置: " + cam.transform.position);
                PluginUtils.LogDebug("  回転: " + cam.transform.rotation.eulerAngles);
                PluginUtils.LogDebug("  視野角: " + cam.fieldOfView);
                PluginUtils.LogDebug("  ニアクリップ: " + cam.nearClipPlane);
                PluginUtils.LogDebug("  ファークリップ: " + cam.farClipPlane);
                PluginUtils.LogDebug("  深度: " + cam.depth);
                PluginUtils.LogDebug("  カリングマスク: " + cam.cullingMask);
                PluginUtils.LogDebug("  レンダリングパス: " + cam.renderingPath);
                PluginUtils.LogDebug("  クリアフラグ: " + cam.clearFlags);
                PluginUtils.LogDebug("  描画レイヤー: " + GetLayerNames(cam.cullingMask));
                PluginUtils.LogDebug("  ---");
            }
        }

        private string GetLayerNames(int cullingMask)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                if ((cullingMask & (1 << i)) != 0)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(LayerMask.LayerToName(i));
                }
            }
            return sb.ToString();
        }

        [Conditional("DEBUG")]
        private void DumpLayerInfo()
        {
            PluginUtils.LogDebug("レイヤー情報:");

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    PluginUtils.LogDebug("レイヤー " + i + ":");
                    PluginUtils.LogDebug("  名前: " + layerName);
                    PluginUtils.LogDebug("  ビットマスク: " + (1 << i));

                    // レイヤーの衝突マトリックス情報を取得
                    string collisions = GetLayerCollisions(i);
                    PluginUtils.LogDebug("  衝突するレイヤー: " + collisions);

                    PluginUtils.LogDebug("  ---");
                }
            }
        }

        private string GetLayerCollisions(int layer)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(LayerMask.LayerToName(i));
                }
            }
            return sb.ToString();
        }

        [Conditional("DEBUG")]
        private void DumpDoFInfo()
        {
            PluginUtils.LogDebug("DoF情報:");

            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            var dof = studioHack.depthOfField;
            if (dof != null)
            {
                PluginUtils.LogDebug("  有効: " + dof.enabled);
                PluginUtils.LogDebug("  焦点距離: " + dof.focalLength);
                PluginUtils.LogDebug("  焦点サイズ: " + dof.focalSize);
                PluginUtils.LogDebug("  絞り値: " + dof.aperture);
                PluginUtils.LogDebug("  ブラーサイズ: " + dof.maxBlurSize);
                PluginUtils.LogDebug("  高解像度: " + dof.highResolution);
                PluginUtils.LogDebug("  サンプル数: " + dof.blurSampleCount);
                PluginUtils.LogDebug("  近距離: " + dof.nearBlur);
            }
        }
    }
}