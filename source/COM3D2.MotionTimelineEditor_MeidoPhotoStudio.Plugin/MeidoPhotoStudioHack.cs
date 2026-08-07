using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterController = MeidoPhotoStudio.Plugin.Core.Character.CharacterController;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    public class MeidoPhotoStudioHack : StudioHackBase
    {
        private readonly Dictionary<CharacterController, bool> editingCharacters = new Dictionary<CharacterController, bool>();

        private MeidoPhotoStudio.Plugin.Api.Api api;

        private CharacterController ActiveCharacter
        {
            get
            {
                if (!api.Active)
                {
                    return null;
                }

                if (api.Character.Busy)
                {
                    return null;
                }

                return api.Character.SelectedCharacter;
            }
        }

        public override string pluginName => "MeidoPhotoStudio";

        public override int priority => 50;

        public override Maid selectedMaid
        {
            get
            {
                var activeMeido = ActiveCharacter;
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
                _allMaids.AddRange(api.Character.ActiveCharacters.Select(character => character.Maid));

                return _allMaids;
            }
        }

        private List<StudioModelStat> _modelList = new List<StudioModelStat>();
        public override List<StudioModelStat> modelList
        {
            get
            {
                _modelList.Clear();

                foreach (var prop in api.Prop.Props)
                {
                    var displayName = prop.PropModel.Name;
                    var fileName = GetPropFileName(prop.PropModel);
                    var myRoomId = prop.PropModel is MyRoomPropModel myRoomModel ? myRoomModel.ID : 0;
                    var bgObjectId = prop.PropModel is PhotoBgPropModel photoBgProp ? photoBgProp.ID : 0;
                    var transform = prop.Transform;

                    var attachPoint = PhotoTransTargetObject.AttachPoint.Null;
                    var attachMaidSlotNo = -1;

                    if (api.Prop.TryGetAttachmentInfo(prop, out var attachPointInfo))
                    {
                        attachPoint = ConvertAttachPoint(attachPointInfo.AttachPoint);
                        attachMaidSlotNo = attachPointInfo.MaidIndex;
                    }

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
                foreach (var lightController in api.Light.Lights)
                {
                    if (lightController == null)
                    {
                        continue;
                    }

                    var light = lightController.Light;
                    var transform = light.transform;

                    var stat = new StudioLightStat(light, transform, lightController, index++);
                    _lightList.Add(stat);
                }

                return _lightList;
            }
        }

        public override int selectedMaidSlotNo
        {
            get
            {
                if (api.Character.Busy)
                {
                    return -1;
                }

                return api.Character.SelectedCharacterIndex;
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

        public override bool isPoseEditing
        {
            get
            {
                if (!api.Active || api.Character.Busy)
                {
                    return false;
                }

                if (!(ActiveCharacter is CharacterController character))
                {
                    return false;
                }

                return (editingCharacters.TryGetValue(character, out var poseEditing) && poseEditing)
                    || api.Character.GetIKDragHandleController(character).IKEnabled;
            }

            set
            {
                if (!api.Active || api.Character.Busy)
                {
                    return;
                }

                if (!(ActiveCharacter is CharacterController character))
                {
                    return;
                }

                if (value && isAnmPlaying)
                {
                    isAnmPlaying = false;
                }

                editingCharacters[character] = value;
                api.Character.GetIKDragHandleController(character).IKEnabled = value;
            }
        }

        public override bool isIKVisible
        {
            get
            {
                if (!api.Active || api.Character.Busy)
                {
                    return false;
                }

                if (!(api.Character.SelectedCharacter is CharacterController character))
                {
                    return false;
                }

                return api.Character.GetIKDragHandleController(character).IKEnabled;
            }

            set
            {
                if (!api.Active || api.Character.Busy)
                {
                    return;
                }

                if (!(api.Character.SelectedCharacter is CharacterController character))
                {
                    return;
                }

                api.Character.GetIKDragHandleController(character).IKEnabled = value;
            }
        }

        public override bool isAnmEnabled
        {
            get
            {
                if (!api.Active || api.Character.Busy)
                {
                    return false;
                }

                if (!(ActiveCharacter is CharacterController character))
                {
                    return false;
                }

                return character.Animation.Playing;
            }

            set
            {
                if (!api.Active || api.Character.Busy)
                {
                    return;
                }

                if (!(ActiveCharacter is CharacterController character))
                {
                    return;
                }

                character.Animation.Playing = value;
                maidManager.SetAnmEnabledAll(value);
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

        public override Camera subCamera => null;

        public override bool isUIVisible
        {
            get => api.UI.Visible;
            set => api.UI.Visible = value;
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

        private static string GetPropFileName(IPropModel propModel)
        {
            var fileName = string.Empty;
            if (propModel is DeskPropModel deskProp)
            {
                fileName = string.IsNullOrEmpty(deskProp.AssetName)
                    ? deskProp.PrefabName
                    : deskProp.AssetName;
            }
            else if (propModel is MenuFilePropModel menuFilePropModel)
            {
                fileName = menuFilePropModel.Filename;
            }
            else if (propModel is MyRoomPropModel myRoomPropModel)
            {
                fileName = myRoomPropModel.AssetName;
            }
            else if (propModel is PhotoBgPropModel photoBgPropModel)
            {
                fileName = string.IsNullOrEmpty(photoBgPropModel.AssetName)
                    ? photoBgPropModel.PrefabName
                    : photoBgPropModel.AssetName;
            }
            else if (propModel is BackgroundPropModel backgroundPropModel)
            {
                fileName = backgroundPropModel.AssetName;
            }
            else if (propModel is OtherPropModel otherPropModel)
            {
                fileName = otherPropModel.AssetName;
            }

            return fileName;
        }

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

            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(MeidoPhotoStudio.Plugin.Plugin.PluginGuid))
            {
                MTEUtils.LogError("MeidoPhotoStudio is not installed");
                return false;
            }

            api = MeidoPhotoStudio.Plugin.Plugin.Api;
            MTEUtils.AssertNull(api != null, "MeidoPhotoStudio's API is null");

            if (api == null)
            {
                MTEUtils.LogError("MeidoPhotoStudioのAPIが見つかりませんでした");
                return false;
            }

            api.Activating += OnMeidoPhotoStudioActivating;
            api.Character.CalledCharacters += OnCharactersCalled;

            return true;
        }

        public override void ChangeMaid(Maid maid)
        {
            var characterIndex = api.Character.ActiveCharacters.FindIndex(character => maid.ValueEquals(character.Maid));
            MTEUtils.LogDebug("ChangeMaid: " + characterIndex);
            if (characterIndex == -1)
            {
                return;
            }

            api.Character.SelectedCharacterIndex = characterIndex;
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

            if (!api.Active)
            {
                _errorMessage = "MeidoPhotoStudioを有効化してください";
                return false;
            }

            return true;
        }

        public override void DeleteAllModels()
        {
            api.Prop.RemoveAllProps();
        }

        public override void DeleteModel(StudioModelStat model)
        {
            var prop = model.obj as PropController;
            if (prop == null)
            {

                return;
            }

            api.Prop.RemoveProp(prop);
        }

        public override void CreateModel(StudioModelStat model)
        {
            IPropModel propModel;

            if (model.info.type == StudioModelType.MyRoom)
            {
                propModel = new MyRoomPropModel(MyRoomCustom.PlacementData.GetData(model.info.myRoomId));
            }
            else if (model.info.bgObjectId != 0)
            {
                propModel = new PhotoBgPropModel(PhotoBGObjectData.Get(model.info.bgObjectId));
            }
            else if (model.info.type == StudioModelType.Mod)
            {
                propModel = new MenuFileParser().ParseMenuFile(model.info.fileName, false);
            }
            else
            {
                propModel = new OtherPropModel(model.info.fileName);
            }

            try
            {
                var prop = api.Prop.AddProp(propModel);
                model.transform = prop.Transform;
                model.obj = prop;

                UpdateAttachPoint(model);
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("CreateModel: モデルの追加に失敗しました" + model.name);
            }
        }

        private void OnMeidoPhotoStudioActivating(object sender, EventArgs e)
        {
            editingCharacters.Clear();
        }

        private void OnCharactersCalled(object sender, MeidoPhotoStudio.Plugin.Core.Character.CharacterServiceEventArgs e)
        {
            var oldCharacters = editingCharacters.Keys.ToArray();

            foreach (var character in oldCharacters.Except(e.LoadedCharacters))
            {
                editingCharacters.Remove(character);
            }
        }

        private CharacterController GetCharacter(int slotNo)
        {
            if (slotNo < 0 || slotNo >= api.Character.CharacterCount)
            {
                return null;
            }

            return api.Character[slotNo];
        }

        public override void UpdateAttachPoint(StudioModelStat model)
        {
            var prop = model.obj as PropController;
            if (prop == null)
            {
                MTEUtils.LogError("UpdateAttachPoint: モデルが見つかりません" + model.name);
                return;
            }

            if (model.attachMaidSlotNo == -1)
            {
                return;
            }

            var attachPoint = ConvertAttachPoint(model.attachPoint);
            var attachMaidSlotNo = model.attachMaidSlotNo;
            var attachCharacter = GetCharacter(attachMaidSlotNo);

            if (attachCharacter == null)
            {
                MTEUtils.LogError("UpdateAttachPoint: Could not get character at index " + attachMaidSlotNo);
                return;
            }

            api.Prop.AttachPropTo(prop, attachCharacter, attachPoint, false);
        }

        public override void SetModelVisible(StudioModelStat model, bool visible)
        {
            var prop = model.obj as PropController;
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
            api.Light.RemoveAllLights();
        }

        public override void DeleteLight(StudioLightStat light)
        {
            var lightController = light.obj as LightController;
            if (lightController == null)
            {
                MTEUtils.LogError("DeleteLight: ライトが見つかりません" + light.name);
                return;
            }

            api.Light.RemoveLight(lightController);
        }

        public override void CreateLight(StudioLightStat stat)
        {
            try
            {
                var light = api.Light.AddLight();

                stat.light = light.Light;
                stat.transform = stat.light.transform;
                stat.obj = light;

                ChangeLight(stat);
                ApplyLight(stat);
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("CreateLight: Light could not be created");
            }
        }

        public override void ChangeLight(StudioLightStat stat)
        {
            var lightController = stat.obj as LightController;
            if (lightController == null || stat.light == null || stat.transform == null)
            {
                MTEUtils.LogError("ChangeLight: ライトが見つかりません" + stat.name);
                return;
            }

            var lightType = stat.type;
            if (lightType != lightController.Type)
            {
                lightController.Type = lightType;
            }
        }

        public override void ApplyLight(StudioLightStat stat)
        {
            var lightController = stat.obj as LightController;
            if (lightController == null || stat.light == null || stat.transform == null)
            {
                MTEUtils.LogError("ApplyLight: ライトが見つかりません" + stat.name);
                return;
            }

            var light = stat.light;
            var transform = stat.transform;

            lightController.Rotation = transform.rotation;
            lightController.Colour = light.color;
            lightController.Range = light.range;
            lightController.Intensity = light.intensity;
            lightController.SpotAngle = light.spotAngle;
            lightController.ShadowStrength = light.shadowStrength;
            //light.shadowBias = light.shadowBias;
            light.enabled = stat.visible;
        }

        public override void UpdateUndress(Maid maid, DressSlotID slotId, bool isVisible)
        {
            if (maid != selectedMaid)
            {
                return;
            }

            ActiveCharacter.Clothing[DressUtils.GetBodySlotId(slotId)] = isVisible;
        }

        private IKDragHandleController.HandleType ConvertMeidoBoneType(IKHoldType iKHoldType)
        {
            switch (iKHoldType)
            {
                case IKHoldType.Arm_L_Tip:
                    return IKDragHandleController.HandleType.HandL;
                case IKHoldType.Arm_R_Tip:
                    return IKDragHandleController.HandleType.HandR;
                case IKHoldType.Arm_L_Joint:
                    return IKDragHandleController.HandleType.ForearmL;
                case IKHoldType.Arm_R_Joint:
                    return IKDragHandleController.HandleType.ForearmR;
                case IKHoldType.Foot_L_Tip:
                    return IKDragHandleController.HandleType.FootL;
                case IKHoldType.Foot_R_Tip:
                    return IKDragHandleController.HandleType.FootR;
                case IKHoldType.Foot_L_Joint:
                    return IKDragHandleController.HandleType.CalfL;
                case IKHoldType.Foot_R_Joint:
                    return IKDragHandleController.HandleType.CalfR;
            }

            return IKDragHandleController.HandleType.Body;
        }

        public override bool IsIKDragging(IKHoldType iKHoldType)
        {
            var handleType = ConvertMeidoBoneType(iKHoldType);
            var controller = api.Character.GetIKDragHandleController(ActiveCharacter)[handleType];

            return controller.DragHandleDragging || controller.GizmoDragging;
        }
    }
}
