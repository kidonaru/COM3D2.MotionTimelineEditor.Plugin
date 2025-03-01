using System;
using System.IO;
using UnityEngine.SceneManagement;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using COM3D2.MotionTimelineEditor;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    public class MeidoPhotoStudioHack : StudioHackBase
    {
        private MeidoPhotoStudioWrapper mps = new MeidoPhotoStudioWrapper();

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
            set => maidManager.SetMotionPlayingAll(!value);
        }

        public override string pluginName => "MeidoPhotoStudio";

        public override int priority => 50;

        public override Maid selectedMaid
        {
            get
            {
                var activeMeido = mps.activeMeido;
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
                foreach (var meido in mps.activeMeidoList)
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

                foreach (var prop in mps.propList)
                {
                    var displayName = prop.Name;
                    var fileName = prop.Info.Filename;
                    var myRoomId = prop.Info.MyRoomID;
                    var bgObjectId = 0;
                    var transform = prop.MyObject;
                    var attachPoint = ConvertAttachPoint(prop.AttachPointInfo.AttachPoint);
                    var attachMaidSlotNo = GetMaidSlotNo(prop.AttachPointInfo.MaidGuid);
                    var visible = prop.Visible;

                    //MTEUtils.LogDebug("modelList name:{0} attachPoint:{1} attachMaidSlotNo:{2}", displayName, attachPoint, attachMaidSlotNo);

                    var model = modelManager.CreateModelStat(
                        displayName,
                        fileName,
                        myRoomId,
                        bgObjectId,
                        transform,
                        attachPoint,
                        attachMaidSlotNo,
                        prop,
                        pluginName,
                        visible);

                    _modelList.Add(model);
                }

                return _modelList;
            }
        }

        private List<StudioLightStat> _lightList = new List<StudioLightStat>();
        public override List<StudioLightStat> lightList
        {
            get
            {
                _lightList.Clear();

                int index = 0;
                foreach (var dragPointLight in mps.lightList)
                {
                    if (dragPointLight == null)
                    {
                        continue;
                    }

                    var light = dragPointLight.GetLight();
                    var transform = light.transform;

                    var stat = new StudioLightStat(light, transform, dragPointLight, index++);
                    _lightList.Add(stat);
                }

                return _lightList;
            }
        }

        public override int selectedMaidSlotNo => allMaids.IndexOf(selectedMaid);

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

        public override bool hasIkBoxVisible => false;

        public override bool isIkBoxVisibleRoot
        {
            get => false;
            set { } // do nothing
        }

        public override bool isIkBoxVisibleBody
        {
            get => false;
            set { } // do nothing
        }

        public override bool isPoseEditing
        {
            get => mps.isReleaseIK;
            set
            {
                if (value && isMotionPlaying)
                {
                    isMotionPlaying = false;
                }

                mps.isIK = value;
                mps.isBoneIK = value;
                mps.isReleaseIK = value;
            }
        }

        public override bool isMotionPlaying
        {
            get => !isStop;
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
            set { } // do nothing
        }

        public override bool useMuneKeyL
        {
            set { } // do nothing
        }

        public override bool useMuneKeyR
        {
            set { } // do nothing
        }

        public override Camera subCamera
        {
            get => mps.subCamera;
        }

        public override bool isUIVisible
        {
            get => mps.mainWindow?.Visible ?? false;
            set
            {
                if (mps.mainWindow != null)
                {
                    mps.mainWindow.Visible = value;
                }
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
            MTEUtils.Log("MeidoPhotoStudioHack: 初期化中...");

            if (!base.Init())
            {
                return false;
            }

            if (!mps.Init())
            {
                return false;
            }

            return true;
        }

        public override void ChangeMaid(Maid maid)
        {
            var targetMaidSlotNo = allMaids.IndexOf(maid);
            MTEUtils.LogDebug("ChangeMaid: " + targetMaidSlotNo);
            mps.meidoManager.ChangeMaid(targetMaidSlotNo);
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(scene, sceneMode);
            isSceneActive = scene.name == "SceneEdit" || scene.name == "SceneDaily";
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (!mps.active)
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
            mps.propManager.DeleteAllProps();
        }

        public override void DeleteModel(StudioModelStat model)
        {
            var index = mps.propList.FindIndex(p => p.MyObject == model.transform);
            if (index >= 0)
            {
                mps.propManager.RemoveProp(index);
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

            if (!mps.propManager.AddFromPropInfo(propInfo))
            {
                MTEUtils.LogError("CreateModel: モデルの追加に失敗しました" + model.name);
                return;
            }

            var propList = mps.propList;
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
                MTEUtils.LogError("UpdateAttachPoint: モデルが見つかりません" + model.name);
                return;
            }

            var attachPoint = ConvertAttachPoint(model.attachPoint);
            var attachMaidSlotNo = model.attachMaidSlotNo;
            var attachMeido = mps.GetMeido(attachMaidSlotNo);
            prop.AttachTo(attachMeido, attachPoint, false);
        }

        public override void SetModelVisible(StudioModelStat model, bool visible)
        {
            var prop = model.obj as DragPointProp;
            if (prop == null)
            {
                MTEUtils.LogError("SetModelVisible: モデルが見つかりません" + model.name);
                return;
            }

            if (prop.Visible != visible)
            {
                prop.Visible = visible;
            }
        }

        public override bool CanCreateLight()
        {
            return true;
        }

        public override void DeleteAllLights()
        {
            mps.lightManager.ClearLights();
        }

        public override void DeleteLight(StudioLightStat light)
        {
            var dragPointLight = light.obj as DragPointLight;
            if (dragPointLight == null)
            {
                MTEUtils.LogError("DeleteLight: ライトが見つかりません" + light.name);
                return;
            }

            var lightList = mps.lightList;
			for (int i = 1; i < lightList.Count; i++)
			{
				if (lightList[i] == dragPointLight)
				{
					mps.lightManager.DeleteLight(i, false);
					return;
				}
			}
        }

        private DragPointLight.MPSLightType ConvertLightType(LightType lightType)
        {
            switch (lightType)
            {
                case LightType.Directional:
                    return DragPointLight.MPSLightType.Normal;
                case LightType.Point:
                    return DragPointLight.MPSLightType.Point;
                case LightType.Spot:
                    return DragPointLight.MPSLightType.Spot;
                default:
                    return DragPointLight.MPSLightType.Normal;
            }
        }

        public override void CreateLight(StudioLightStat stat)
        {
            mps.lightManager.AddLight(null, false);

            var dragPointLight = mps.lightManager.CurrentLight;
            stat.light = dragPointLight.GetLight();
            stat.transform = stat.light.transform;
            stat.obj = dragPointLight;

            ChangeLight(stat);
            ApplyLight(stat);
        }

        public override void ChangeLight(StudioLightStat stat)
        {
            var dragPointLight = stat.obj as DragPointLight;
            if (dragPointLight == null || stat.light == null || stat.transform == null)
            {
                MTEUtils.LogError("ChangeLight: ライトが見つかりません" + stat.name);
                return;
            }

            var lightType = ConvertLightType(stat.type);
            if (lightType != dragPointLight.SelectedLightType)
            {
                dragPointLight.SetLightType(lightType);
            }
        }

        public override void ApplyLight(StudioLightStat stat)
        {
            var dragPointLight = stat.obj as DragPointLight;
            if (dragPointLight == null || stat.light == null || stat.transform == null)
            {
                MTEUtils.LogError("ApplyLight: ライトが見つかりません" + stat.name);
                return;
            }

            var light = stat.light;
            var transform = stat.transform;

            dragPointLight.Rotation = transform.rotation;
            dragPointLight.LightColour = light.color;
            dragPointLight.Range = light.range;
            dragPointLight.Intensity = light.intensity;
            dragPointLight.SpotAngle = light.spotAngle;
            dragPointLight.ShadowStrength = light.shadowStrength;
            //light.shadowBias = light.shadowBias;
            light.enabled = stat.visible;
        }

        public override void UpdateUndress(Maid maid, DressSlotID slotId, bool isVisible)
        {
            if (maid != selectedMaid)
            {
                return;
            }

            mps.maidDressingPane.UpdatePane();
        }

        public override void Update()
        {
            base.Update();
        }

        private MeidoBoneType ConvertMeidoBoneType(IKHoldType iKHoldType)
        {
            switch (iKHoldType)
            {
                case IKHoldType.Arm_L_Tip:
                    return MeidoBoneType.HandL;
                case IKHoldType.Arm_R_Tip:
                    return MeidoBoneType.HandR;
                case IKHoldType.Arm_L_Joint:
                    return MeidoBoneType.ForearmL;
                case IKHoldType.Arm_R_Joint:
                    return MeidoBoneType.ForearmR;
                case IKHoldType.Foot_L_Tip:
                    return MeidoBoneType.FootL;
                case IKHoldType.Foot_R_Tip:
                    return MeidoBoneType.FootR;
                case IKHoldType.Foot_L_Joint:
                    return MeidoBoneType.CalfL;
                case IKHoldType.Foot_R_Joint:
                    return MeidoBoneType.CalfR;
            }

            return MeidoBoneType.Body;
        }

        public override bool IsIKDragging(IKHoldType iKHoldType)
        {
            var activeMeido = mps.activeMeido;
            var dragPoint = mps.GetDragPoint(activeMeido, ConvertMeidoBoneType(iKHoldType));
            if (dragPoint == null)
            {
                MTEUtils.LogError("dragPoint is null");
                return false;
            }

            return dragPoint.IsDragging();
        }
    }
}