using UnityEngine;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;
using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    using MPS = MeidoPhotoStudio.Plugin.MeidoPhotoStudio;
    using MPSWindowManager = MeidoPhotoStudio.Plugin.WindowManager;

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

        public Meido GetMeido(int slotNo)
        {
            var activeMeidoList = this.activeMeidoList;
            if (slotNo < 0 || slotNo >= activeMeidoList.Count)
            {
                return null;
            }

            return activeMeidoList[slotNo];
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