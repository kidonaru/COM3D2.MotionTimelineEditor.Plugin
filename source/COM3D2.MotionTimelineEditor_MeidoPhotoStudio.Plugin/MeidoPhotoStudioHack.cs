using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    using MPS = MeidoPhotoStudio.Plugin.MeidoPhotoStudio;

    public class MeidoPhotoStudioHack : StudioHackBase
    {
        private MPS meidoPhotoStudio = null;

        private FieldInfo fieldActive = null;
        private FieldInfo fieldMeidoManager = null;
        private FieldInfo fieldWindowManager = null;
        private FieldInfo fieldMaidIKPane = null;
        private FieldInfo fieldReleaseIKToggle = null;
        private FieldInfo fieldBoneIKToggle = null;

        private bool isActive
        {
            get
            {
                return (bool)fieldActive.GetValue(meidoPhotoStudio);
            }
        }

        private MeidoManager meidoManager
        {
            get
            {
                return (MeidoManager)fieldMeidoManager.GetValue(meidoPhotoStudio);
            }
        }

        private WindowManager windowManager
        {
            get
            {
                return (WindowManager)fieldWindowManager.GetValue(meidoPhotoStudio);
            }
        }

        private MainWindow mainWindow
        {
            get
            {
                if (windowManager == null)
                {
                    return null;
                }
                return windowManager[Constants.Window.Main] as MainWindow;
            }
        }

        private PoseWindowPane poseWindowPane
        {
            get
            {
                if (mainWindow == null)
                {
                    return null;
                }
                return mainWindow[Constants.Window.Pose] as PoseWindowPane;
            }
        }

        private MaidIKPane maidIKPane
        {
            get
            {
                return (MaidIKPane)fieldMaidIKPane.GetValue(poseWindowPane);
            }
        }

        private Toggle releaseIKToggle
        {
            get
            {
                return (Toggle)fieldReleaseIKToggle.GetValue(maidIKPane);
            }
        }

        private Toggle boneIKToggle
        {
            get
            {
                return (Toggle)fieldBoneIKToggle.GetValue(maidIKPane);
            }
        }

        private bool isReleaseIK
        {
            get
            {
                var toggle = this.releaseIKToggle;
                if (toggle == null)
                {
                    return false;
                }

                return toggle.Value;
            }
            set
            {
                var toggle = this.releaseIKToggle;
                if (toggle == null)
                {
                    return;
                }

                toggle.SetValueOnly(value);
            }
        }

        private bool isBoneIK
        {
            get
            {
                var toggle = this.boneIKToggle;
                if (toggle == null)
                {
                    return false;
                }

                return toggle.Value;
            }
            set
            {
                var toggle = this.boneIKToggle;
                if (toggle == null)
                {
                    return;
                }

                toggle.Value = value;
            }
        }

        private Meido activeMeido
        {
            get
            {
                var meidoManager = this.meidoManager;
                if (meidoManager == null)
                {
                    return null;
                }

                return meidoManager.ActiveMeido;
            }
        }

        private bool isStop
        {
            get
            {
                if (animationState != null)
                {
                    return !animationState.enabled;
                }
                return false;
            }
            set
            {
                if (animationState != null)
                {
                    animationState.enabled = !value;
                }
            }
        }

        public override int priority
        {
            get
            {
                return 50;
            }
        }

        public override Maid activeMaid
        {
            get
            {
                var activeMeido = this.activeMeido;
                if (activeMeido == null)
                {
                    return null;
                }

                return activeMeido.Maid;
            }
        }

        public override string outputAnmPath
        {
            get
            {
                var path = Path.GetFullPath(".\\") + "BepInEx\\config\\MeidoPhotoStudio\\Presets\\Custom Poses\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public override bool hasIkBoxVisible
        {
            get
            {
                return false;
            }
        }

        public override bool isIkBoxVisibleRoot
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

        public override bool isIkBoxVisibleBody
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

        public override bool isPoseEditing
        {
            get
            {
                return isReleaseIK;
            }
            set
            {
                if (value == isPoseEditing)
                {
                    return;
                }

                if (value && isMotionPlaying)
                {
                    isStop = true;
                }

                isBoneIK = value;
                isReleaseIK = value;
            }
        }

        public override bool isMotionPlaying
        {
            get
            {
                return !isStop;
            }
            set
            {
                if (value == isMotionPlaying)
                {
                    return;
                }

                if (value && isPoseEditing)
                {
                    isPoseEditing = false;
                }

                isStop = !value;
            }
        }

        private float _motionSliderRate = 0f;

        public override float motionSliderRate
        {
            get
            {
                return _motionSliderRate;
            }
            set
            {
                //Extensions.LogDebug("motionSliderRate update：" + value);
                _motionSliderRate = value;

                if (animationState != null)
                {
                    var isStop = this.isStop;
                    var maxNum = animationState.length;
                    var current = Mathf.Clamp01(value) * maxNum;
                    animationState.time = current;
                    animationState.enabled = true;
                    animation.Sample();
                    if (isStop)
                    {
                        animationState.enabled = false;
                    }
                }
            }
        }

        public override bool useMuneKeyL
        {
            get
            {
                if (maid != null)
                {
                    return !maid.body0.jbMuneL.enabled;
                }
                return false;
            }
            set
            {
                if (maid != null)
                {
                    maid.body0.jbMuneL.enabled = !value;
                    maid.body0.MuneYureL(maid.body0.jbMuneL.enabled ? 1 : 0);
                }
            }
        }

        public override bool useMuneKeyR
        {
            get
            {
                if (maid != null)
                {
                    return !maid.body0.jbMuneR.enabled;
                }
                return false;
            }
            set
            {
                if (maid != null)
                {
                    maid.body0.jbMuneR.enabled = !value;
                    maid.body0.MuneYureR(maid.body0.jbMuneR.enabled ? 1 : 0);
                }
            }
        }

        public MeidoPhotoStudioHack()
        {
        }

        public override bool Init()
        {
            PluginUtils.Log("初期化中...");

            if (!base.Init())
            {
                return false;
            }

            {
                GameObject gameObject = GameObject.Find("BepInEx_Manager");
                meidoPhotoStudio = gameObject.GetComponentInChildren<MPS>(true);
                PluginUtils.AssertNull(meidoPhotoStudio != null, "meidoPhotoStudio is null");
            }

            if (meidoPhotoStudio == null)
            {
                PluginUtils.LogError("MeidoPhotoStudioが見つかりませんでした");
                return false;
            }

            {
                BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

                fieldActive = typeof(MPS).GetField("active", bindingAttr);
                PluginUtils.AssertNull(fieldActive != null, "fieldActive is null");

                fieldMeidoManager = typeof(MPS).GetField("meidoManager", bindingAttr);
                PluginUtils.AssertNull(fieldMeidoManager != null, "fieldMeidoManager is null");

                fieldWindowManager = typeof(MPS).GetField("windowManager", bindingAttr);
                PluginUtils.AssertNull(fieldWindowManager != null, "fieldWindowManager is null");

                fieldMaidIKPane = typeof(PoseWindowPane).GetField("maidIKPane", bindingAttr);
                PluginUtils.AssertNull(fieldMaidIKPane != null, "fieldMaidIKPane is null");

                fieldReleaseIKToggle = typeof(MaidIKPane).GetField("releaseIKToggle", bindingAttr);
                PluginUtils.AssertNull(fieldReleaseIKToggle != null, "fieldReleaseIKToggle is null");

                fieldBoneIKToggle = typeof(MaidIKPane).GetField("boneIKToggle", bindingAttr);
                PluginUtils.AssertNull(fieldBoneIKToggle != null, "fieldBoneIKToggle is null");
            }

            return true;
        }

        public override void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(sceneName, sceneMode);
            isSceneActive = sceneName.name == "SceneEdit" || sceneName.name == "SceneDaily";
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (!isActive)
            {
                _errorMessage = "MeidoPhotoStudioを有効化してください";
                return false;
            }

            return true;
        }

        protected override void OnMaidChanged(Maid maid)
        {
            base.OnMaidChanged(maid);
            motionSliderRate = _motionSliderRate;
        }

        protected override void OnAnmChanged(string anmName)
        {
            base.OnAnmChanged(anmName);
            motionSliderRate = _motionSliderRate;
        }

        public override void Update()
        {
            base.Update();

            if (maid == null)
            {
                return;
            }

            // 再生位置更新
            var animationState = StudioHackBase.animationState;
            if (animationState != null && animationState.enabled && animationState.length > 0f)
            {
                float value = animationState.time;
                if (animationState.length < animationState.time)
                {
                    if (animationState.wrapMode == WrapMode.ClampForever)
                    {
                        value = animationState.length;
                    }
                    else
                    {
                        value = animationState.time - animationState.length * (float)((int)(animationState.time / animationState.length));
                    }
                }
                _motionSliderRate = value / animationState.length;
            }
        }
    }
}