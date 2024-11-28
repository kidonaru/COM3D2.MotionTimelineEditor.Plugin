using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("モデルシェイプ", 23)]
    public partial class ModelShapeKeyTimelineLayer : ModelTimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(ModelShapeKeyTimelineLayer).Name;
            }
        }

        public override List<string> allBoneNames
        {
            get
            {
                return modelManager.blendShapeNames;
            }
        }

        private ModelShapeKeyTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static ModelShapeKeyTimelineLayer Create(int slotNo)
        {
            return new ModelShapeKeyTimelineLayer(0);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var model in modelManager.models)
            {
                if (model.blendShapes.Count == 0)
                {
                    continue;
                }

                var setMenuItem = new BoneSetMenuItem(model.name, model.displayName);
                allMenuItems.Add(setMenuItem);

                foreach (var blendShape in model.blendShapes)
                {
                    var menuItem = new BoneMenuItem(blendShape.name, blendShape.shapeKeyName);
                    setMenuItem.AddChild(menuItem);
                }
            }
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

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyPlayData()
        {
            base.ApplyPlayData();

            var models = modelManager.models;
            foreach (var model in models)
            {
                model.FixBlendValues();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var blendShape = modelManager.GetBlendShape(motion.name);
            if (blendShape == null)
            {
                return;
            }

            var start = motion.start as TransformDataShapeKey;
            var end = motion.end as TransformDataShapeKey;

            float easingTime = CalcEasingValue(t, start.easing);
            var weight = Mathf.Lerp(start.weight, end.weight, easingTime);
            blendShape.weight = weight;
        }

        public override void OnModelAdded(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.blendShapes.Select(x => x.name).ToList();
            AddFirstBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnModelRemoved(StudioModelStat model)
        {
            InitMenuItems();

            var boneNames = model.blendShapes.Select(x => x.name).ToList();
            RemoveAllBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel)
        {
            var blendShapes = sourceModel.blendShapes;
            var newModelName = newModel.name;
            foreach (var keyFrame in keyFrames)
            {
                foreach (var blendShape in blendShapes)
                {
                    var sourceBone = keyFrame.GetBone(blendShape.name);
                    if (sourceBone == null)
                    {
                        continue;
                    }

                    var baseName = blendShape.shapeKeyName;
                    var newBoneName = string.Format("{0}/{1}", newModelName, baseName);

                    var newBone = keyFrame.GetOrCreateBone(sourceBone.transform.type, newBoneName);
                    newBone.transform.FromTransformData(sourceBone.transform);
                }
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var blendShape in modelManager.blendShapeMap.Values)
            {
                var boneName = blendShape.name;

                var trans = CreateTransformData<TransformDataShapeKey>(boneName);
                trans.easing = GetEasing(frame.frameNo, boneName);
                trans.weight = blendShape.weight;

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
                    DrawBlendShapes(view);
                    break;
                case TabType.管理:
                    DrawModelManage(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawBlendShapes(GUIView view)
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

            var blendShapes = model.blendShapes;
            if (blendShapes.Count == 0)
            {
                view.DrawLabel("シェイプキーが存在しません", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();
            {
                view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

                for (var i = 0; i < blendShapes.Count; i++)
                {
                    var blendShape = blendShapes[i];
                    var weight = blendShape.weight;
                    var updateTransform = false;

                    view.DrawLabel(blendShape.shapeKeyName, -1, 20);

                    updateTransform |= view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            min = -1f,
                            max = 2f,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = weight,
                            onChanged = x => weight = x,
                        });

                    if (updateTransform)
                    {
                        blendShape.weight = weight;
                        model.FixBlendValues();
                    }
                }
            }
            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.ShapeKey;
        }
    }
}