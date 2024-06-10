using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    using MPS = MeidoPhotoStudio.Plugin.MeidoPhotoStudio;
    using MPSWindowManager = MeidoPhotoStudio.Plugin.WindowManager;

    public class MeidoPhotoStudioHack : StudioHackBase
    {
        private MPS meidoPhotoStudio = null;

        private FieldInfo fieldActive = null;
        private FieldInfo fieldMeidoManager = null;
        private FieldInfo fieldWindowManager = null;
        private FieldInfo fieldPropManager = null;
        private FieldInfo fieldMaidIKPane = null;
        private FieldInfo fieldIKToggle = null;
        private FieldInfo fieldReleaseIKToggle = null;
        private FieldInfo fieldBoneIKToggle = null;
        private FieldInfo fieldPropList = null;

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

        private MPSWindowManager windowManager
        {
            get
            {
                return (MPSWindowManager) fieldWindowManager.GetValue(meidoPhotoStudio);
            }
        }

        private PropManager propManager
        {
            get
            {
                return (PropManager)fieldPropManager.GetValue(meidoPhotoStudio);
            }
        }

        private MeidoPhotoStudio.Plugin.MainWindow mainWindow
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

        private Toggle ikToggle
        {
            get
            {
                return (Toggle)fieldIKToggle.GetValue(maidIKPane);
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

        private List<DragPointProp> propList
        {
            get
            {
                return (List<DragPointProp>)fieldPropList.GetValue(propManager);
            }
        }

        private bool isIK
        {
            get
            {
                var toggle = this.ikToggle;
                if (toggle == null)
                {
                    return false;
                }

                return toggle.Value;
            }
            set
            {
                var toggle = this.ikToggle;
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

                toggle.SetValueOnly(value);

                if (activeMeido != null)
                {
                    activeMeido.Bone = value;
                }
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

        private List<Meido> activeMeidoList
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
                maidManager.SetMotionPlayingAll(!value);
            }
        }

        public override string pluginName
        {
            get
            {
                return "MeidoPhotoStudio";
            }
        }

        public override int priority
        {
            get
            {
                return 50;
            }
        }

        public override Maid selectedMaid
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

        private List<Maid> _allMaids = new List<Maid>();
        public override List<Maid> allMaids
        {
            get
            {
                _allMaids.Clear();
                foreach (var meido in activeMeidoList)
                {
                    if (meido != null)
                    {
                        _allMaids.Add(meido.Maid);
                    }
                }
                return _allMaids;
            }
        }

        private List<StudioModelStat> _modelList = new List<StudioModelStat>();
        public override List<StudioModelStat> modelList
        {
            get
            {
                _modelList.Clear();

                foreach (var prop in propList)
                {
                    var displayName = prop.Name;
                    var fileName = prop.Info.Filename;
                    var myRoomId = prop.Info.MyRoomID;
                    var bgObjectId = 0;
                    var transform = prop.MyObject;
                    var attachPoint = ConvertAttachPoint(prop.AttachPointInfo.AttachPoint);
                    var attachMaidSlotNo = GetMaidSlotNo(prop.AttachPointInfo.MaidGuid);

                    //PluginUtils.LogDebug("modelList name:{0} attachPoint:{1} attachMaidSlotNo:{2}", displayName, attachPoint, attachMaidSlotNo);

                    var model = modelManager.CreateModelStat(
                        displayName,
                        fileName,
                        myRoomId,
                        bgObjectId,
                        transform,
                        attachPoint,
                        attachMaidSlotNo,
                        prop,
                        pluginName);

                    _modelList.Add(model);
                }

                return _modelList;
            }
        }

        public override int selectedMaidSlotNo
        {
            get
            {
                return allMaids.IndexOf(selectedMaid);
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
                if (value && isMotionPlaying)
                {
                    isMotionPlaying = false;
                }

                isIK = value;
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
                if (value && isPoseEditing)
                {
                    isPoseEditing = false;
                }

                isStop = !value;
            }
        }

        public override float motionSliderRate
        {
            set
            {
                // do nothing
            }
        }

        public override bool useMuneKeyL
        {
            set
            {
                // do nothing
            }
        }

        public override bool useMuneKeyR
        {
           
            set
            {
                // do nothing
            }
        }

        public MeidoPhotoStudioHack()
        {
        }

        private static readonly Dictionary<AttachPoint, PhotoTransTargetObject.AttachPoint> MeidoAttachPointMap
            = new Dictionary<AttachPoint, PhotoTransTargetObject.AttachPoint>
            {
                {AttachPoint.None, PhotoTransTargetObject.AttachPoint.Null},
                {AttachPoint.Head, PhotoTransTargetObject.AttachPoint.Head},
                {AttachPoint.Neck, PhotoTransTargetObject.AttachPoint.Neck},
                {AttachPoint.UpperArmL, PhotoTransTargetObject.AttachPoint.UpperArm_L},
                {AttachPoint.UpperArmR, PhotoTransTargetObject.AttachPoint.UpperArm_R},
                {AttachPoint.ForearmL, PhotoTransTargetObject.AttachPoint.Forearm_L},
                {AttachPoint.ForearmR, PhotoTransTargetObject.AttachPoint.Forearm_R},
                {AttachPoint.MuneL, PhotoTransTargetObject.AttachPoint.Mune_L},
                {AttachPoint.MuneR, PhotoTransTargetObject.AttachPoint.Mune_R},
                {AttachPoint.HandL, PhotoTransTargetObject.AttachPoint.Hand_L},
                {AttachPoint.HandR, PhotoTransTargetObject.AttachPoint.Hand_R},
                {AttachPoint.Pelvis, PhotoTransTargetObject.AttachPoint.Root},
                {AttachPoint.ThighL, PhotoTransTargetObject.AttachPoint.Thigh_L},
                {AttachPoint.ThighR, PhotoTransTargetObject.AttachPoint.Thigh_R},
                {AttachPoint.CalfL, PhotoTransTargetObject.AttachPoint.Calf_L},
                {AttachPoint.CalfR, PhotoTransTargetObject.AttachPoint.Calf_R},
                {AttachPoint.FootL, PhotoTransTargetObject.AttachPoint.Foot_L},
                {AttachPoint.FootR, PhotoTransTargetObject.AttachPoint.Foot_R},
                {AttachPoint.Spine0, PhotoTransTargetObject.AttachPoint.Fix}, // 仮
            };

        private static readonly Dictionary<PhotoTransTargetObject.AttachPoint, AttachPoint> PhotoAttachPointMap
            = MeidoAttachPointMap.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static PhotoTransTargetObject.AttachPoint ConvertAttachPoint(AttachPoint attachPoint)
        {
            PhotoTransTargetObject.AttachPoint result;
            if (MeidoAttachPointMap.TryGetValue(attachPoint, out result))
            {
                return result;
            }
            return PhotoTransTargetObject.AttachPoint.Null;
        }

        public static AttachPoint ConvertAttachPoint(PhotoTransTargetObject.AttachPoint attachPoint)
        {
            AttachPoint result;
            if (PhotoAttachPointMap.TryGetValue(attachPoint, out result))
            {
                return result;
            }
            return AttachPoint.None;
        }

        public override bool Init()
        {
            PluginUtils.Log("MeidoPhotoStudioHack: 初期化中...");

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

                fieldPropManager = typeof(MPS).GetField("propManager", bindingAttr);
                PluginUtils.AssertNull(fieldPropManager != null, "fieldPropManager is null");

                fieldMaidIKPane = typeof(PoseWindowPane).GetField("maidIKPane", bindingAttr);
                PluginUtils.AssertNull(fieldMaidIKPane != null, "fieldMaidIKPane is null");

                fieldIKToggle = typeof(MaidIKPane).GetField("ikToggle", bindingAttr);
                PluginUtils.AssertNull(fieldIKToggle != null, "fieldIKToggle is null");

                fieldReleaseIKToggle = typeof(MaidIKPane).GetField("releaseIKToggle", bindingAttr);
                PluginUtils.AssertNull(fieldReleaseIKToggle != null, "fieldReleaseIKToggle is null");

                fieldBoneIKToggle = typeof(MaidIKPane).GetField("boneIKToggle", bindingAttr);
                PluginUtils.AssertNull(fieldBoneIKToggle != null, "fieldBoneIKToggle is null");

                fieldPropList = typeof(PropManager).GetField("propList", bindingAttr);
                PluginUtils.AssertNull(fieldPropList != null, "fieldPropList is null");
            }

            return true;
        }

        public override void ChangeMaid(Maid maid)
        {
            var targetMaidSlotNo = allMaids.IndexOf(maid);
            PluginUtils.LogDebug("ChangeMaid: " + targetMaidSlotNo);
            meidoManager.ChangeMaid(targetMaidSlotNo);
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

        private readonly static Dictionary<StudioModelType, PropInfo.PropType> propTypeMap =
            new Dictionary<StudioModelType, PropInfo.PropType>
        {
            { StudioModelType.Asset, PropInfo.PropType.Odogu },
            { StudioModelType.Prefab, PropInfo.PropType.Odogu },
            { StudioModelType.Mod, PropInfo.PropType.Mod },
            { StudioModelType.MyRoom, PropInfo.PropType.MyRoom },
        };

        public override void DeleteAllModels()
        {
            propManager.DeleteAllProps();
        }

        public override void DeleteModel(StudioModelStat model)
        {
            var index = propList.FindIndex(p => p.MyObject == model.transform);
            if (index >= 0)
            {
                propManager.RemoveProp(index);
            }
        }

        public override void CreateModel(StudioModelStat model)
        {
            var propType = propTypeMap[model.info.type];
            var isMyRoom = model.info.type == StudioModelType.MyRoom;
            var propInfo = new PropInfo(propType)
            {
                Filename = isMyRoom ? model.info.prefabName : model.info.fileName,
                MyRoomID = model.info.myRoomId,
            };

            if (!propManager.AddFromPropInfo(propInfo))
            {
                PluginUtils.LogError("CreateModel: モデルの追加に失敗しました" + model.name);
                return;
            }

            var propList = this.propList;
            var prop = propList.Last();

            model.transform = prop.MyObject;
            model.obj = prop;

            UpdateAttachPoint(model);
        }

        public override void UpdateAttachPoint(StudioModelStat model)
        {
            var prop = model.obj as DragPointProp;
            if (prop == null)
            {
                PluginUtils.LogError("UpdateAttachPoint: モデルが見つかりません" + model.name);
                return;
            }

            var attachPoint = ConvertAttachPoint(model.attachPoint);
            var attachMaidSlotNo = model.attachMaidSlotNo;
            var attachMeido = GetMeido(attachMaidSlotNo);
            prop.AttachTo(attachMeido, attachPoint, false);
        }

        private Meido GetMeido(int slotNo)
        {
            var activeMeidoList = this.activeMeidoList;
            if (slotNo < 0 || slotNo >= activeMeidoList.Count)
            {
                return null;
            }

            return activeMeidoList[slotNo];
        }

        protected override void OnMaidChanged(int maidSlotNo, Maid maid)
        {
            base.OnMaidChanged(maidSlotNo, maid);
        }

        protected override void OnAnmChanged(int maidSlotNo, string anmName)
        {
            base.OnAnmChanged(maidSlotNo, anmName);
        }

        public override void Update()
        {
            base.Update();
        }
    }
}