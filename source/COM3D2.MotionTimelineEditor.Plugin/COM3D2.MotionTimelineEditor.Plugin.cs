using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class GUIOption : GUIOptionBase
    {
        public override float keyRepeatTimeFirst => config.keyRepeatTimeFirst;
        public override float keyRepeatTime => config.keyRepeatTime;
        public override bool useHSVColor
        {
            get => config.useHSVColor;
            set
            {
                config.useHSVColor = value;
                config.dirty = true;
            }
        }
        public override Texture2D changeIcon => bundleManager.changeIcon;

        private static Config config => ConfigManager.instance.config;
        private static TimelineBundleManager bundleManager => TimelineBundleManager.instance;
    }

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

        private static ManagerRegistry managerRegistry => ManagerRegistry.instance;
        private static MaidManager maidManager => MaidManager.instance;
        private static Maid maid => maidManager.maid;
        private static TimelineManager timelineManager => TimelineManager.instance;
        private static TimelineData timeline => TimelineManager.instance.timeline;
        private static TimelineHistoryManager historyManager => TimelineHistoryManager.instance;
        private static StudioHackManager studioHackManager => StudioHackManager.instance;
        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;
        private static WindowManager windowManager => WindowManager.instance;
        private static ConfigManager configManager => ConfigManager.instance;
        private static Config config => ConfigManager.instance.config;
        private static ITimelineLayer currentLayer => timelineManager.currentLayer;
        private static MainWindow mainWindow => windowManager.mainWindow;
        private static TimelineBundleManager bundleManager => TimelineBundleManager.instance;

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
                MTEUtils.LogException(e);
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

                studioHackManager.PreUpdate();

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

                    maidManager.PreUpdate();

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
                    if (config.GetKeyDown(KeyBindType.AddKeyFrameAll))
                    {
                        currentLayer.AddKeyFrameAll();
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
                        studioHackManager.isPoseEditing = !studioHackManager.isPoseEditing;
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

                    managerRegistry.Update();
                }
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
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

                    managerRegistry.LateUpdate();
                }
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        public void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            try
            {
                if (!config.pluginEnabled)
                {
                    return;
                }

                if (scene.name == "SceneTitle")
                {
                    this.isEnable = false;
                }

                BinaryLoader.ClearCache();
                BlendShapeLoader.ClearCache();
                ModMenuLoader.ClearCache();
                TextureLoader.ClearCache();

                managerRegistry.OnChangedSceneLevel(scene, sceneMode);
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
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
                MTEUtils.Log("初期化中...");
                MTEUtils.LogDebug("Unity Version: " + Application.unityVersion);

                configManager.Init();

                GUIView.option = new GUIOption();

                if (!config.pluginEnabled)
                {
                    MTEUtils.Log("プラグインが無効になっています");
                    return;
                }

                SceneManager.sceneLoaded += OnChangedSceneLevel;

                studioHackManager.Register(new StudioHack());

                timelineManager.RegisterLayer(
                    typeof(BGColorTimelineLayer), BGColorTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(BGModelTimelineLayer), BGModelTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(BGTimelineLayer), BGTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(CameraTimelineLayer), CameraTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(EyesTimelineLayer), EyesTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(LightTimelineLayer), LightTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(ModelBoneTimelineLayer), ModelBoneTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(ModelShapeKeyTimelineLayer), ModelShapeKeyTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(ModelTimelineLayer), ModelTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(MotionTimelineLayer), MotionTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(MoveTimelineLayer), MoveTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(PostEffectTimelineLayer), PostEffectTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(ShapeKeyTimelineLayer), ShapeKeyTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(StageLaserTimelineLayer), StageLaserTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(StageLightTimelineLayer), StageLightTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(UndressTimelineLayer), UndressTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(VoiceTimelineLayer), VoiceTimelineLayer.Create
                );
                timelineManager.RegisterLayer(
                    typeof(PsylliumTimelineLayer), PsylliumTimelineLayer.Create
                );

                timelineManager.RegisterTransform(
                    TransformType.BG, TimelineManager.CreateTransform<TransformDataBG>
                );
                timelineManager.RegisterTransform(
                    TransformType.BGColor, TimelineManager.CreateTransform<TransformDataBGColor>
                );
                timelineManager.RegisterTransform(
                    TransformType.BGGroundColor, TimelineManager.CreateTransform<TransformDataBGGroundColor>
                );
                timelineManager.RegisterTransform(
                    TransformType.BGModel, TimelineManager.CreateTransform<TransformDataBGModel>
                );
                timelineManager.RegisterTransform(
                    TransformType.Camera, TimelineManager.CreateTransform<TransformDataCamera>
                );
                timelineManager.RegisterTransform(
                    TransformType.DepthOfField, TimelineManager.CreateTransform<TransformDataDepthOfField>
                );
                timelineManager.RegisterTransform(
                    TransformType.DistanceFog, TimelineManager.CreateTransform<TransformDataDistanceFog>
                );
                timelineManager.RegisterTransform(
                    TransformType.ExtendBone, TimelineManager.CreateTransform<TransformDataExtendBone>
                );
                timelineManager.RegisterTransform(
                    TransformType.Eyes, TimelineManager.CreateTransform<TransformDataEyes>
                );
                timelineManager.RegisterTransform(
                    TransformType.FingerBlend, TimelineManager.CreateTransform<TransformDataFingerBlend>
                );
                timelineManager.RegisterTransform(
                    TransformType.Grounding, TimelineManager.CreateTransform<TransformDataGrounding>
                );
                timelineManager.RegisterTransform(
                    TransformType.IKHold, TimelineManager.CreateTransform<TransformDataIKHold>
                );
                timelineManager.RegisterTransform(
                    TransformType.Light, TimelineManager.CreateTransform<TransformDataLight>
                );
                timelineManager.RegisterTransform(
                    TransformType.LookAtTarget, TimelineManager.CreateTransform<TransformDataLookAtTarget>
                );
                timelineManager.RegisterTransform(
                    TransformType.Model, TimelineManager.CreateTransform<TransformDataModel>
                );
                timelineManager.RegisterTransform(
                    TransformType.ModelBone, TimelineManager.CreateTransform<TransformDataModelBone>
                );
                timelineManager.RegisterTransform(
                    TransformType.Move, TimelineManager.CreateTransform<TransformDataMove>
                );
                timelineManager.RegisterTransform(
                    TransformType.Paraffin, TimelineManager.CreateTransform<TransformDataParaffin>
                );
                timelineManager.RegisterTransform(
                    TransformType.PsylliumPattern, TimelineManager.CreateTransform<TransformDataPsylliumPattern>
                );
                timelineManager.RegisterTransform(
                    TransformType.PsylliumTransform, TimelineManager.CreateTransform<TransformDataPsylliumTransform>
                );
                timelineManager.RegisterTransform(
                    TransformType.PsylliumArea, TimelineManager.CreateTransform<TransformDataPsylliumArea>
                );
                timelineManager.RegisterTransform(
                    TransformType.PsylliumBar, TimelineManager.CreateTransform<TransformDataPsylliumBar>
                );
                timelineManager.RegisterTransform(
                    TransformType.PsylliumController, TimelineManager.CreateTransform<TransformDataPsylliumController>
                );
                timelineManager.RegisterTransform(
                    TransformType.PsylliumHand, TimelineManager.CreateTransform<TransformDataPsylliumHand>
                );
                timelineManager.RegisterTransform(
                    TransformType.Rimlight, TimelineManager.CreateTransform<TransformDataRimlight>
                );
                timelineManager.RegisterTransform(
                    TransformType.Root, TimelineManager.CreateTransform<TransformDataRoot>
                );
                timelineManager.RegisterTransform(
                    TransformType.Rotation, TimelineManager.CreateTransform<TransformDataRotation>
                );
                timelineManager.RegisterTransform(
                    TransformType.ShapeKey, TimelineManager.CreateTransform<TransformDataShapeKey>
                );
                timelineManager.RegisterTransform(
                    TransformType.StageLaser, TimelineManager.CreateTransform<TransformDataStageLaser>
                );
                timelineManager.RegisterTransform(
                    TransformType.StageLaserController, TimelineManager.CreateTransform<TransformDataStageLaserController>
                );
                timelineManager.RegisterTransform(
                    TransformType.StageLight, TimelineManager.CreateTransform<TransformDataStageLight>
                );
                timelineManager.RegisterTransform(
                    TransformType.StageLightController, TimelineManager.CreateTransform<TransformDataStageLightController>
                );
                timelineManager.RegisterTransform(
                    TransformType.Undress, TimelineManager.CreateTransform<TransformDataUndress>
                );
                timelineManager.RegisterTransform(
                    TransformType.Voice, TimelineManager.CreateTransform<TransformDataVoice>
                );

                managerRegistry.RegisterManager(StudioHackManager.instance);
                managerRegistry.RegisterManager(MaidManager.instance);
                managerRegistry.RegisterManager(ModelHackManager.instance);
                managerRegistry.RegisterManager(PartsEditHackManager.instance);
                managerRegistry.RegisterManager(LightHackManager.instance);

                managerRegistry.RegisterManager(StudioModelManager.instance);
                managerRegistry.RegisterManager(BGModelManager.instance);
                managerRegistry.RegisterManager(StudioLightManager.instance);
                managerRegistry.RegisterManager(StageLaserManager.instance);
                managerRegistry.RegisterManager(StageLightManager.instance);
                managerRegistry.RegisterManager(PsylliumManager.instance);
                managerRegistry.RegisterManager(MovieManager.instance);
                managerRegistry.RegisterManager(GridViewManager.instance);
                managerRegistry.RegisterManager(PostEffectManager.instance);

                managerRegistry.RegisterManager(TimelineManager.instance);
                managerRegistry.RegisterManager(TimelineLoadManager.instance);
                managerRegistry.RegisterManager(TimelineHistoryManager.instance);

                managerRegistry.RegisterManager(TimelineBundleManager.instance);
                managerRegistry.RegisterManager(BoneMenuManager.Instance);
                managerRegistry.RegisterManager(WindowManager.instance);
                managerRegistry.RegisterManager(PhotoBGManager.instance);

                managerRegistry.RegisterManager(ConfigManager.instance);
                managerRegistry.RegisterManager(BGMManager.instance);
                managerRegistry.RegisterManager(CameraManager.instance);

                AddGearMenu();
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        GameObject gearMenuIcon = null;

        public void AddGearMenu()
        {
            gearMenuIcon = GUIExtBase.GUIExt.Add(
                PluginInfo.PluginName,
                PluginInfo.PluginName,
                bundleManager.LoadBytes("icon.png"),
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
            MTEUtils.UIHide();
            isVisible = false;
            yield return new WaitForEndOfFrame();
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            yield return new WaitForEndOfFrame();
            isVisible = true;
            MTEUtils.UIResume();

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
                MTEUtils.LogException(e);
            }
        }

        public void OnLoad()
        {
            MTEUtils.LogDebug("MotionTimelineEditor.OnLoad");

            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }
            if (timeline == null)
            {
                return;
            }

            managerRegistry.OnLoad();
        }

        private void OnPluginEnable()
        {
            MTEUtils.Log("プラグインが有効になりました");

            DumpAllCameraInfo();
            DumpLayerInfo();
            DumpBGObject();
            DumpAllShaders();

            studioHackManager.PreUpdate();

            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            if (timeline != null)
            {
                studioHack.SetBackgroundVisible(timeline.isBackgroundVisible);
            }

            OnLoad();

            DumpDoFInfo();
        }

        private void OnPluginDisable()
        {
            MTEUtils.Log("プラグインが無効になりました");

            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            managerRegistry.OnPluginDisable();
        }

        [Conditional("DEBUG")]
        private void DumpAllCameraInfo()
        {
            Camera[] allCameras = Camera.allCameras;
            MTEUtils.LogDebug("カメラの総数: " + allCameras.Length);

            for (int i = 0; i < allCameras.Length; i++)
            {
                Camera cam = allCameras[i];
                MTEUtils.LogDebug("カメラ " + (i + 1) + ":");
                MTEUtils.LogDebug("  名前: " + cam.name);
                MTEUtils.LogDebug("  有効: " + cam.enabled);
                MTEUtils.LogDebug("  平行投影: " + cam.orthographic);
                MTEUtils.LogDebug("  平行投影サイズ: " + cam.orthographicSize);
                MTEUtils.LogDebug("  位置: " + cam.transform.position);
                MTEUtils.LogDebug("  回転: " + cam.transform.rotation.eulerAngles);
                MTEUtils.LogDebug("  視野角: " + cam.fieldOfView);
                MTEUtils.LogDebug("  ニアクリップ: " + cam.nearClipPlane);
                MTEUtils.LogDebug("  ファークリップ: " + cam.farClipPlane);
                MTEUtils.LogDebug("  深度: " + cam.depth);
                MTEUtils.LogDebug("  カリングマスク: " + cam.cullingMask);
                MTEUtils.LogDebug("  レンダリングパス: " + cam.renderingPath);
                MTEUtils.LogDebug("  クリアフラグ: " + cam.clearFlags);
                MTEUtils.LogDebug("  描画レイヤー: " + GetLayerNames(cam.cullingMask));
                MTEUtils.LogDebug("  ---");
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
            MTEUtils.LogDebug("レイヤー情報:");

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    MTEUtils.LogDebug("レイヤー " + i + ":");
                    MTEUtils.LogDebug("  名前: " + layerName);
                    MTEUtils.LogDebug("  ビットマスク: " + (1 << i));

                    // レイヤーの衝突マトリックス情報を取得
                    string collisions = GetLayerCollisions(i);
                    MTEUtils.LogDebug("  衝突するレイヤー: " + collisions);

                    MTEUtils.LogDebug("  ---");
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
            MTEUtils.LogDebug("DoF情報:");

            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }

            var dof = studioHack.depthOfField;
            if (dof != null)
            {
                MTEUtils.LogDebug("  有効: " + dof.enabled);
                MTEUtils.LogDebug("  焦点距離: " + dof.focalLength);
                MTEUtils.LogDebug("  焦点サイズ: " + dof.focalSize);
                MTEUtils.LogDebug("  絞り値: " + dof.aperture);
                MTEUtils.LogDebug("  ブラーサイズ: " + dof.maxBlurSize);
                MTEUtils.LogDebug("  高解像度: " + dof.highResolution);
                MTEUtils.LogDebug("  サンプル数: " + dof.blurSampleCount);
                MTEUtils.LogDebug("  近距離: " + dof.nearBlur);
            }
        }

        [Conditional("DEBUG")]
        private void DumpBGObject()
        {
            MTEUtils.LogDebug("背景階層構造:");
            BgMgr bgMgr = GameMain.Instance.BgMgr;
            GameObject bgObject = bgMgr.BgObject;

            if (bgObject != null)
            {
                DumpGameObject(bgObject, 0);
            }
        }

        [Conditional("DEBUG")]
        private void DumpGameObject(GameObject obj, int depth)
        {
            string indent = new string(' ', depth * 2);
            MTEUtils.LogDebug($"{indent}└─ {obj.name}");
            
            foreach (Transform child in obj.transform)
            {
                DumpGameObject(child.gameObject, depth + 1);
            }
        }

        [Conditional("DEBUG")]
        private void DumpAllShaders()
        {
            MTEUtils.LogDebug("使用中のシェーダー一覧:");

            // シーン内のすべてのRendererを取得
            foreach (Renderer renderer in GameObject.FindObjectsOfType<Renderer>())
            {
                foreach (Material material in renderer.materials)
                {
                    if (material != null && material.shader != null)
                    {
                        MTEUtils.LogDebug($"  - {material.shader.name} ({renderer.name})");
                    }
                }
            }
        }
    }
}