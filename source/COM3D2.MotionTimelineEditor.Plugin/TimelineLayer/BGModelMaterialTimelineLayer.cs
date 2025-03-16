using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static COM3D2.MotionTimelineEditor.Plugin.ModelMaterial;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("背景モデルマテリアル", 34)]
    public class BGModelMaterialTimelineLayer : BGModelTimelineLayerBase
    {
        public override Type layerType => typeof(BGModelMaterialTimelineLayer);
        public override string layerName => nameof(BGModelMaterialTimelineLayer);

        public override List<string> allBoneNames => bgModelManager.materialNames;

        private BGModelMaterialTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static BGModelMaterialTimelineLayer Create(int slotNo)
        {
            return new BGModelMaterialTimelineLayer(0);
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
                if (model.materials.Count == 0)
                {
                    continue;
                }

                var setMenuItem = new BoneSetMenuItem(model.name, model.displayName);
                allMenuItems.Add(setMenuItem);

                foreach (var material in model.materials)
                {
                    var menuItem = new BoneMenuItem(material.name, material.displayName);
                    setMenuItem.AddChild(menuItem);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

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

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var material = bgModelManager.GetMaterial(motion.name);
            if (material == null)
            {
                return;
            }

            var start = motion.start as TransformDataModelMaterial;
            var end = motion.end as TransformDataModelMaterial;

            if (indexUpdated)
            {
                material.SetColor(ColorPropertyType._Color, start.color);
                material.SetColor(ColorPropertyType._ShadowColor, start.ShadowColor);
                material.SetColor(ColorPropertyType._RimColor, start.RimColor);
                material.SetColor(ColorPropertyType._OutlineColor, start.OutlineColor);
            }

            float easingTime = CalcEasingValue(t, motion.easing);

            if (start.color != end.color)
            {
                material.SetColor(ColorPropertyType._Color, Color.Lerp(start.color, end.color, easingTime));
            }

            if (start.ShadowColor != end.ShadowColor)
            {
                material.SetColor(ColorPropertyType._ShadowColor, Color.Lerp(start.ShadowColor, end.ShadowColor, easingTime));
            }

            if (start.RimColor != end.RimColor)
            {
                material.SetColor(ColorPropertyType._RimColor, Color.Lerp(start.RimColor, end.RimColor, easingTime));
            }

            if (start.OutlineColor != end.OutlineColor)
            {
                material.SetColor(ColorPropertyType._OutlineColor, Color.Lerp(start.OutlineColor, end.OutlineColor, easingTime));
            }
        }

        public void OnBGModelSetup()
        {
            InitMenuItems();

            var materialNames = bgModelManager.materialNames;
            AddFirstBones(materialNames);
            ApplyCurrentFrame(true);
        }

        public void OnBGModelAdded(BGModelStat model)
        {
            InitMenuItems();

            var materialNames = model.materials.Select(x => x.name).ToList();
            AddFirstBones(materialNames);
            ApplyCurrentFrame(true);
        }

        public void OnBGModelRemoved(BGModelStat model)
        {
            InitMenuItems();

            var materialNames = model.materials.Select(x => x.name).ToList();
            RemoveAllBones(materialNames);
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var sourceMaterial in bgModelManager.materialMap.Values)
            {
                var materialName = sourceMaterial.name;

                var trans = frame.GetOrCreateTransformData<TransformDataModelMaterial>(materialName);
                trans.color = sourceMaterial.GetColor(ColorPropertyType._Color);
                trans.ShadowColor = sourceMaterial.GetColor(ColorPropertyType._ShadowColor);
                trans.RimColor = sourceMaterial.GetColor(ColorPropertyType._RimColor);
                trans.OutlineColor = sourceMaterial.GetColor(ColorPropertyType._OutlineColor);
                trans.easing = GetEasing(frame.frameNo, materialName);
            }
        }

        private GUIComboBox<BGModelStat> _modelComboBox = new GUIComboBox<BGModelStat>
        {
            getName = (model, index) => model.displayName,
            buttonSize = new Vector2(200, 20),
            contentSize = new Vector2(200, 300),
        };

        private GUIComboBox<ModelMaterial> _materialComboBox = new GUIComboBox<ModelMaterial>
        {
            getName = (material, index) => material.displayName,
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
                    DrawMaterial(view);
                    break;
                case TabType.管理:
                    DrawModelManage(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawMaterial(GUIView view)
        {
            _modelComboBox.items = bgModelManager.models;

            if (_modelComboBox.items.Count == 0)
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

            _materialComboBox.items = model.materials;

            if (_materialComboBox.items.Count == 0)
            {
                view.DrawLabel("マテリアルが存在しません", 200, 20);
                return;
            }

            _materialComboBox.DrawButton(view);

            var material = _materialComboBox.currentItem;
            if (material == null)
            {
                view.DrawLabel("マテリアルが見つかりません", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();
            {
                view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

                view.DrawLabel(material.displayName, 200, 20);

                foreach (var propertyType in ModelMaterial.ColorPropertyTypes)
                {
                    if (!material.HasColor(propertyType)) continue;

                    var color = material.GetColor(propertyType);
                    var initialColor = material.GetInitialColor(propertyType);
                    var cache = view.GetColorFieldCache("", false);

                    view.DrawLabel(propertyType.ToString(), 200, 20);

                    view.DrawColor(cache, color, initialColor, newColor =>
                    {
                        material.SetColor(propertyType, newColor);
                    });
                }

                view.DrawHorizontalLine(Color.gray);
            }
            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.ModelMaterial;
        }
    }
}