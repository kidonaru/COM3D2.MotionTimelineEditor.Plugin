using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("背景モデル", 33)]
    public class BGModelTimelineLayer : BGModelTimelineLayerBase
    {
        public override string className => typeof(BGModelTimelineLayer).Name;

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

            BGModelManager.onModelSetup += OnBGModelSetup;
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

            BGModelManager.onModelSetup -= OnBGModelSetup;
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

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var model = bgModelManager.GetModel(motion.name);
            if (model == null)
            {
                return;
            }

            var transform = model.transform;
            if (transform == null)
            {
                return;
            }

            var start = motion.start;
            var end = motion.end;

            float easingTime = CalcEasingValue(t, start.easing);
            transform.localPosition = Vector3.Lerp(start.position, end.position, easingTime);
            transform.localRotation = Quaternion.Lerp(start.rotation, end.rotation, easingTime);
            transform.localScale = Vector3.Lerp(start.scale, end.scale, easingTime);
            model.visible = start.visible;
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

        public override void UpdateFrame(FrameData frame)
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

            _modelComboBox.items = models;
            _modelComboBox.DrawButton("操作対象", view);

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            var model = _modelComboBox.currentItem;
            if (model == null || model.transform == null)
            {
                view.DrawLabel("モデルが選択されていません", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawToggle(model.displayName, model.visible, 200, 20, newValue =>
            {
                model.visible = newValue;
            });

            var info = model.info;

            DrawTransform(
                view,
                model.transform,
                TransformEditType.全て,
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