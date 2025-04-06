using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("背景モデル", 33)]
    public class BGModelTimelineLayer : BGModelTimelineLayerBase
    {
        public override Type layerType => typeof(BGModelTimelineLayer);
        public override string layerName => nameof(BGModelTimelineLayer);

        public override List<string> allBoneNames => bgModelManager.modelNames;

        private BGModelTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static BGModelTimelineLayer Create(int slotNo)
        {
            return new BGModelTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();

            BGModelManager.onSetup += OnBGModelSetup;
            BGModelManager.onModelAdded += OnBGModelAdded;
            BGModelManager.onModelRemoved += OnBGModelRemoved;
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var model in bgModelManager.models)
            {
                var menuItem = new BoneMenuItem(model.name, model.displayName);
                allMenuItems.Add(menuItem);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            BGModelManager.onSetup -= OnBGModelSetup;
            BGModelManager.onModelAdded -= OnBGModelAdded;
            BGModelManager.onModelRemoved -= OnBGModelRemoved;
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

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated, MotionPlayData playData)
        {
            var model = bgModelManager.GetModel(motion.name);
            if (model == null || model.transform == null)
            {
                return;
            }

            if (indexUpdated)
            {
                ApplyMotionInit(motion, t, model);
            }

            ApplyMotionUpdate(motion, t, model);
        }

        protected void ApplyMotionInit(MotionData motion, float t, BGModelStat model)
        {
            var transform = model.transform;
            var start = motion.start;

            transform.localPosition = start.position;
            transform.localRotation = start.rotation;
            transform.localScale = start.scale;
            model.visible = start.visible;
        }

        protected void ApplyMotionUpdate(MotionData motion, float t, BGModelStat model)
        {
            var transform = model.transform;
            var start = motion.start;
            var end = motion.end;

            float easingTime = CalcEasingValue(t, motion.easing);

            if (start.position != end.position)
            {
                transform.localPosition = Vector3.Lerp(start.position, end.position, easingTime);
            }

            if (start.rotation != end.rotation)
            {
                transform.localRotation = Quaternion.Lerp(start.rotation, end.rotation, easingTime);
            }

            if (start.scale != end.scale)
            {
                transform.localScale = Vector3.Lerp(start.scale, end.scale, easingTime);
            }
        }

        public void OnBGModelSetup()
        {
            InitMenuItems();
            ApplyCurrentFrame(true);
        }

        public void OnBGModelAdded(BGModelStat model)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { model.name });
            ApplyCurrentFrame(true);
        }

        public void OnBGModelRemoved(BGModelStat model)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { model.name });
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            foreach (var model in bgModelManager.models)
            {
                if (model == null || model.transform == null)
                {
                    continue;
                }

                var modelName = model.name;

                var trans = CreateTransformData<TransformDataBGModel>(modelName);
                trans.position = model.transform.localPosition;
                trans.rotation = model.transform.localRotation;
                trans.scale = model.transform.localScale;
                trans.visible = model.visible;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        private GUIComboBox<TransformEditType> _transComboBox = new GUIComboBox<TransformEditType>
        {
            items = Enum.GetValues(typeof(TransformEditType)).Cast<TransformEditType>().ToList(),
            getName = (type, index) => type.ToString(),
        };

        private GUIComboBox<BGModelStat> _modelComboBox = new GUIComboBox<BGModelStat>
        {
            getName = (model, index) => model.displayName,
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
                    DrawModelEdit(view);
                    break;
                case TabType.管理:
                    DrawModelManage(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawModelEdit(GUIView view)
        {
            var models = bgModelManager.models;
            if (models.Count == 0)
            {
                view.DrawLabel("モデルが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            _transComboBox.DrawButton("操作種類", view);

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            foreach (var model in models)
            {
                DrawModel(view, model);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        private void DrawModel(GUIView view, BGModelStat model)
        {
            if (model == null || model.transform == null)
            {
                return;
            }

            view.DrawToggle(model.displayName, model.visible, 200, 20, newValue =>
            {
                model.visible = newValue;
            });

            var info = model.info;
            var editType = _transComboBox.currentItem;

            DrawTransform(
                view,
                model.transform,
                editType,
                DrawMaskAll,
                model.name,
                info.initialPosition,
                info.initialRotation.eulerAngles,
                info.initialScale);

            view.DrawHorizontalLine(Color.gray);
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.BGModel;
        }
    }
}