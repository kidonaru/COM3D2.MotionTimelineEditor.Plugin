using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("モデルボーン", 22)]
    public partial class ModelBoneTimelineLayer : ModelTimelineLayerBase
    {
        public override Type layerType => typeof(ModelBoneTimelineLayer);
        public override string layerName => nameof(ModelBoneTimelineLayer);

        public override List<string> allBoneNames => modelManager.boneNames;

        private ModelBoneTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ModelBoneTimelineLayer Create(int slotNo)
        {
            return new ModelBoneTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();

            StudioModelManager.onModelAdded += OnModelAdded;
            StudioModelManager.onModelRemoved += OnModelRemoved;
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var model in modelManager.models)
            {
                if (model.bones.Count == 0)
                {
                    continue;
                }

                var setMenuItem = new BoneSetMenuItem(model.name, model.displayName);
                allMenuItems.Add(setMenuItem);

                foreach (var bone in model.bones)
                {
                    var menuItem = new ModelBoneMenuItem(bone.name, bone.transform.name);
                    setMenuItem.AddChild(menuItem);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            StudioModelManager.onModelAdded -= OnModelAdded;
            StudioModelManager.onModelRemoved -= OnModelRemoved;
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var bone = modelManager.GetBone(motion.name);
            if (bone == null)
            {
                return;
            }

            var transform = bone.transform;
            if (transform == null)
            {
                return;
            }

            var start = motion.start;
            var end = motion.end;

            if (timeline.isTangentModelBone)
            {
                var t0 = motion.stFrame * timeline.frameDuration;
                var t1 = motion.edFrame * timeline.frameDuration;

                transform.localPosition = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    t);

                transform.localRotation = PluginUtils.HermiteQuaternion(
                    t0,
                    t1,
                    start.rotationValues,
                    end.rotationValues,
                    t);

                transform.localScale = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.scaleValues,
                    end.scaleValues,
                    t);
            }
            else
            {
                float easingTime = CalcEasingValue(t, motion.easing);
                transform.localPosition = Vector3.Lerp(start.position, end.position, easingTime);
                transform.localRotation = Quaternion.Lerp(start.rotation, end.rotation, easingTime);
                transform.localScale = Vector3.Lerp(start.scale, end.scale, easingTime);
            }
        }

        public void OnModelAdded(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.bones.Select(x => x.name).ToList();
            AddFirstBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.bones.Select(x => x.name).ToList();
            RemoveAllBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel)
        {
            var sourceModelBones = sourceModel.bones;
            var newModelName = newModel.name;
            foreach (var keyFrame in keyFrames)
            {
                foreach (var sourceModelBone in sourceModelBones)
                {
                    var sourceBone = keyFrame.GetBone(sourceModelBone.name);
                    if (sourceBone == null)
                    {
                        continue;
                    }

                    var baseName = sourceModelBone.transform.name;
                    var newBoneName = string.Format("{0}/{1}", newModelName, baseName);

                    var newBone = keyFrame.GetOrCreateBone(sourceBone.transform.type, newBoneName);
                    newBone.transform.FromTransformData(sourceBone.transform);
                }
            }
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit)
        {
            foreach (var sourceBone in modelManager.boneMap.Values)
            {
                var boneName = sourceBone.name;

                var trans = CreateTransformData<TransformDataModelBone>(boneName);
                trans.position = sourceBone.transform.localPosition;
                trans.rotation = sourceBone.transform.localRotation;
                trans.scale = sourceBone.transform.localScale;
                trans.easing = GetEasing(frame.frameNo, boneName);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        private GUIComboBox<StudioModelStat> _modelComboBox = new GUIComboBox<StudioModelStat>
        {
            getName = (model, index) => model.displayName,
            buttonSize = new Vector2(200, 20),
            contentSize = new Vector2(200, 300),
        };

        private GUIComboBox<TransformEditType> _transComboBox = new GUIComboBox<TransformEditType>
        {
            items = Enum.GetValues(typeof(TransformEditType)).Cast<TransformEditType>().ToList(),
            getName = (type, index) => type.ToString(),
        };

        private enum TabType
        {
            操作,
            管理,
        }

        private TabType _tabType = TabType.操作;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.操作:
                    DrawBone(view);
                    break;
                case TabType.管理:
                    DrawModelManage(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawBone(GUIView view)
        {
            _modelComboBox.items = modelManager.models;

            if (modelManager.models.Count == 0)
            {
                view.DrawLabel("モデルが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawLabel("モデル選択", 200, 20);
            _modelComboBox.DrawButton(view);

            var model = _modelComboBox.currentItem;
            if (model == null || model.transform == null)
            {
                view.DrawLabel("モデルが見つかりません", 200, 20);
                return;
            }

            var bones = model.bones;
            if (bones.Count == 0)
            {
                view.DrawLabel("ボーンが存在しません", 200, 20);
                return;
            }

            _transComboBox.DrawButton("操作種類", view);

            var editType = _transComboBox.currentItem;

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();
            {
                view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

                foreach (var bone in bones)
                {
                    view.DrawLabel(bone.transform.name, 200, 20);

                    DrawTransform(
                        view,
                        bone.transform,
                        editType,
                        DrawMaskAll,
                        bone.name,
                        bone.initialPosition,
                        bone.initialEulerAngles,
                        bone.initialScale);

                    view.DrawHorizontalLine(Color.gray);
                }
            }
            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.ModelBone;
        }
    }
}