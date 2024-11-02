using UnityEngine;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    using MPS = MeidoPhotoStudio.Plugin.MeidoPhotoStudio;
    using MPSWindowManager = MeidoPhotoStudio.Plugin.WindowManager;

    public enum MeidoBoneType
    {
        Head,
        HeadNub,
        ClavicleL,
        ClavicleR,
        UpperArmL,
        UpperArmR,
        ForearmL,
        ForearmR,
        HandL,
        HandR,
        MuneL,
        MuneSubL,
        MuneR,
        MuneSubR,
        Neck,
        Spine,
        Spine0a,
        Spine1,
        Spine1a,
        ThighL,
        ThighR,
        Pelvis,
        Hip,
        CalfL,
        CalfR,
        FootL,
        FootR,
        Cube,
        Body,
        Torso,
        Finger0L,
        Finger01L,
        Finger02L,
        Finger0NubL,
        Finger1L,
        Finger11L,
        Finger12L,
        Finger1NubL,
        Finger2L,
        Finger21L,
        Finger22L,
        Finger2NubL,
        Finger3L,
        Finger31L,
        Finger32L,
        Finger3NubL,
        Finger4L,
        Finger41L,
        Finger42L,
        Finger4NubL,
        Finger0R,
        Finger01R,
        Finger02R,
        Finger0NubR,
        Finger1R,
        Finger11R,
        Finger12R,
        Finger1NubR,
        Finger2R,
        Finger21R,
        Finger22R,
        Finger2NubR,
        Finger3R,
        Finger31R,
        Finger32R,
        Finger3NubR,
        Finger4R,
        Finger41R,
        Finger42R,
        Finger4NubR,
        Toe0L,
        Toe01L,
        Toe0NubL,
        Toe1L,
        Toe11L,
        Toe1NubL,
        Toe2L,
        Toe21L,
        Toe2NubL,
        Toe0R,
        Toe01R,
        Toe0NubR,
        Toe1R,
        Toe11R,
        Toe1NubR,
        Toe2R,
        Toe21R,
        Toe2NubR
    }

    public class MeidoPhotoStudioWrapper
    {
        private MPS meidoPhotoStudio = null;
        private MeidoPhotoStudioField field = new MeidoPhotoStudioField();

        public bool active
        {
            get
            {
                return (bool)field.active.GetValue(meidoPhotoStudio);
            }
        }

        public MeidoManager meidoManager
        {
            get
            {
                return (MeidoManager)field.meidoManager.GetValue(meidoPhotoStudio);
            }
        }

        public MPSWindowManager windowManager
        {
            get
            {
                return (MPSWindowManager) field.windowManager.GetValue(meidoPhotoStudio);
            }
        }

        public PropManager propManager
        {
            get
            {
                return (PropManager)field.propManager.GetValue(meidoPhotoStudio);
            }
        }

        public LightManager lightManager
        {
            get
            {
                return (LightManager)field.lightManager.GetValue(meidoPhotoStudio);
            }
        }

        public CameraManager cameraManager
        {
            get
            {
                return (CameraManager)field.cameraManager.GetValue(meidoPhotoStudio);
            }
        }

        public EffectManager effectManager
        {
            get
            {
                return (EffectManager)field.effectManager.GetValue(meidoPhotoStudio);
            }
        }

        public DepthOfFieldEffectManager depthOfFieldEffectManager
        {
            get
            {
                if (effectManager == null)
                {
                    return null;
                }
                return effectManager.Get<DepthOfFieldEffectManager>();
            }
        }

        public MeidoPhotoStudio.Plugin.MainWindow mainWindow
        {
            get
            {
                if (windowManager == null)
                {
                    return null;
                }
                return windowManager[Constants.Window.Main] as MeidoPhotoStudio.Plugin.MainWindow;
            }
        }

        public PoseWindowPane poseWindowPane
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

        public MaidIKPane maidIKPane
        {
            get
            {
                if (poseWindowPane == null)
                {
                    return null;
                }
                return (MaidIKPane)field.maidIKPane.GetValue(poseWindowPane);
            }
        }

        public MaidDressingPane maidDressingPane
        {
            get
            {
                if (poseWindowPane == null)
                {
                    return null;
                }
                return (MaidDressingPane)field.maidDressingPane.GetValue(poseWindowPane);
            }
        }

        public Toggle ikToggle
        {
            get
            {
                if (maidIKPane == null)
                {
                    return null;
                }
                return (Toggle)field.ikToggle.GetValue(maidIKPane);
            }
        }

        public Toggle releaseIKToggle
        {
            get
            {
                if (maidIKPane == null)
                {
                    return null;
                }
                return (Toggle)field.releaseIKToggle.GetValue(maidIKPane);
            }
        }

        public Toggle boneIKToggle
        {
            get
            {
                if (maidIKPane == null)
                {
                    return null;
                }
                return (Toggle)field.boneIKToggle.GetValue(maidIKPane);
            }
        }

        public List<DragPointProp> propList
        {
            get
            {
                if (propManager == null)
                {
                    return new List<DragPointProp>();
                }
                return (List<DragPointProp>)field.propList.GetValue(propManager);
            }
        }

        public List<DragPointLight> lightList
        {
            get
            {
                if (lightManager == null)
                {
                    return new List<DragPointLight>();
                }
                return (List<DragPointLight>)field.lightList.GetValue(lightManager);
            }
        }

        public bool isIK
        {
            get
            {
                var toggle = ikToggle;
                if (toggle == null)
                {
                    return false;
                }

                return toggle.Value;
            }
            set
            {
                var toggle = ikToggle;
                if (toggle == null)
                {
                    return;
                }

                toggle.SetValueOnly(value);

                if (activeMeido != null)
                {
                    activeMeido.IK = value;
                }
            }
        }

        public bool isReleaseIK
        {
            get
            {
                var toggle = releaseIKToggle;
                if (toggle == null)
                {
                    return false;
                }

                return toggle.Value;
            }
            set
            {
                var toggle = releaseIKToggle;
                if (toggle == null)
                {
                    return;
                }

                toggle.SetValueOnly(value);
            }
        }

        public bool isBoneIK
        {
            get
            {
                var toggle = boneIKToggle;
                if (toggle == null)
                {
                    return false;
                }

                return toggle.Value;
            }
            set
            {
                var toggle = boneIKToggle;
                if (toggle == null)
                {
                    return;
                }

                toggle.SetValueOnly(value);

                if (activeMeido != null)
                {
                    activeMeido.Bone = value;
                }
            }
        }

        public Meido activeMeido
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

        public List<Meido> activeMeidoList
        {
            get
            {
                var meidoManager = this.meidoManager;
                if (meidoManager == null)
                {
                    return null;
                }

                return meidoManager.ActiveMeidoList;
            }
        }

        public Camera subCamera
        {
            get
            {
                if (cameraManager == null)
                {
                    return null;
                }
                return (Camera)field.subCamera.GetValue(cameraManager);
            }
        }

        public Meido GetMeido(int slotNo)
        {
            var activeMeidoList = this.activeMeidoList;
            if (slotNo < 0 || slotNo >= activeMeidoList.Count)
            {
                return null;
            }

            return activeMeidoList[slotNo];
        }

        public System.Collections.IDictionary GetDragPoints(Meido meido)
        {
            if (meido == null)
            {
                return null;
            }
            var ikManager = meido.IKManager;
            return (System.Collections.IDictionary) field.dragPoints.GetValue(ikManager);
        }

        private static readonly System.Type boneEnumType = typeof(MeidoDragPointManager).GetNestedType("Bone", BindingFlags.NonPublic);

        public DragPointMeido GetDragPoint(Meido meido, MeidoBoneType boneType)
        {
            var dragPoints = GetDragPoints(meido);
            if (dragPoints == null)
            {
                PluginUtils.LogError("dragPoints is null");
                return null;
            }

            var boneValue = Enum.ToObject(boneEnumType, (int)boneType);
            return (DragPointMeido) dragPoints[boneValue];
        }

        public bool Init()
        {
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

            if (!field.Init())
            {
                return false;
            }

            return true;
        }
    }
}