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
                material.SetValue(ValuePropertyType._Shininess, start.Shininess);
                material.SetValue(ValuePropertyType._OutlineWidth, start.OutlineWidth);
                material.SetValue(ValuePropertyType._RimPower, start.RimPower);
                material.SetValue(ValuePropertyType._RimShift, start.RimShift);

                // NPR用プロパティ
                material.SetColor(ColorPropertyType._EmissionColor, start.EmissionColor);
                material.SetColor(ColorPropertyType._MatcapColor, start.MatcapColor);
                material.SetColor(ColorPropertyType._ReflectionColor, start.ReflectionColor);
                material.SetValue(ValuePropertyType._NormalValue, start.NormalValue);
                material.SetValue(ValuePropertyType._ParallaxValue, start.ParallaxValue);
                material.SetValue(ValuePropertyType._MatcapValue, start.MatcapValue);
                material.SetValue(ValuePropertyType._MatcapMaskValue, start.MatcapMaskValue);
                material.SetValue(ValuePropertyType._EmissionValue, start.EmissionValue);
                material.SetValue(ValuePropertyType._EmissionHDRExposure, start.EmissionHDRExposure);
                material.SetValue(ValuePropertyType._EmissionPower, start.EmissionPower);
                material.SetValue(ValuePropertyType._RimLightValue, start.RimLightValue);
                material.SetValue(ValuePropertyType._RimLightPower, start.RimLightPower);
                material.SetValue(ValuePropertyType._MetallicValue, start.MetallicValue);
                material.SetValue(ValuePropertyType._SmoothnessValue, start.SmoothnessValue);
                material.SetValue(ValuePropertyType._OcclusionValue, start.OcclusionValue);
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

            if (start.Shininess != end.Shininess)
            {
                material.SetValue(ValuePropertyType._Shininess, Mathf.Lerp(start.Shininess, end.Shininess, easingTime));
            }

            if (start.OutlineWidth != end.OutlineWidth)
            {
                material.SetValue(ValuePropertyType._OutlineWidth, Mathf.Lerp(start.OutlineWidth, end.OutlineWidth, easingTime));
            }

            if (start.RimPower != end.RimPower)
            {
                material.SetValue(ValuePropertyType._RimPower, Mathf.Lerp(start.RimPower, end.RimPower, easingTime));
            }

            if (start.RimShift != end.RimShift)
            {
                material.SetValue(ValuePropertyType._RimShift, Mathf.Lerp(start.RimShift, end.RimShift, easingTime));
            }

            // NPR用プロパティの補間処理
            if (start.EmissionColor != end.EmissionColor)
            {
                material.SetColor(ColorPropertyType._EmissionColor, Color.Lerp(start.EmissionColor, end.EmissionColor, easingTime));
            }

            if (start.MatcapColor != end.MatcapColor)
            {
                material.SetColor(ColorPropertyType._MatcapColor, Color.Lerp(start.MatcapColor, end.MatcapColor, easingTime));
            }

            if (start.ReflectionColor != end.ReflectionColor)
            {
                material.SetColor(ColorPropertyType._ReflectionColor, Color.Lerp(start.ReflectionColor, end.ReflectionColor, easingTime));
            }

            if (start.NormalValue != end.NormalValue)
            {
                material.SetValue(ValuePropertyType._NormalValue, Mathf.Lerp(start.NormalValue, end.NormalValue, easingTime));
            }

            if (start.ParallaxValue != end.ParallaxValue)
            {
                material.SetValue(ValuePropertyType._ParallaxValue, Mathf.Lerp(start.ParallaxValue, end.ParallaxValue, easingTime));
            }

            if (start.MatcapValue != end.MatcapValue)
            {
                material.SetValue(ValuePropertyType._MatcapValue, Mathf.Lerp(start.MatcapValue, end.MatcapValue, easingTime));
            }

            if (start.MatcapMaskValue != end.MatcapMaskValue)
            {
                material.SetValue(ValuePropertyType._MatcapMaskValue, Mathf.Lerp(start.MatcapMaskValue, end.MatcapMaskValue, easingTime));
            }

            if (start.EmissionValue != end.EmissionValue)
            {
                material.SetValue(ValuePropertyType._EmissionValue, Mathf.Lerp(start.EmissionValue, end.EmissionValue, easingTime));
            }

            if (start.EmissionHDRExposure != end.EmissionHDRExposure)
            {
                material.SetValue(ValuePropertyType._EmissionHDRExposure, Mathf.Lerp(start.EmissionHDRExposure, end.EmissionHDRExposure, easingTime));
            }

            if (start.EmissionPower != end.EmissionPower)
            {
                material.SetValue(ValuePropertyType._EmissionPower, Mathf.Lerp(start.EmissionPower, end.EmissionPower, easingTime));
            }

            if (start.RimLightValue != end.RimLightValue)
            {
                material.SetValue(ValuePropertyType._RimLightValue, Mathf.Lerp(start.RimLightValue, end.RimLightValue, easingTime));
            }

            if (start.RimLightPower != end.RimLightPower)
            {
                material.SetValue(ValuePropertyType._RimLightPower, Mathf.Lerp(start.RimLightPower, end.RimLightPower, easingTime));
            }

            if (start.MetallicValue != end.MetallicValue)
            {
                material.SetValue(ValuePropertyType._MetallicValue, Mathf.Lerp(start.MetallicValue, end.MetallicValue, easingTime));
            }

            if (start.SmoothnessValue != end.SmoothnessValue)
            {
                material.SetValue(ValuePropertyType._SmoothnessValue, Mathf.Lerp(start.SmoothnessValue, end.SmoothnessValue, easingTime));
            }

            if (start.OcclusionValue != end.OcclusionValue)
            {
                material.SetValue(ValuePropertyType._OcclusionValue, Mathf.Lerp(start.OcclusionValue, end.OcclusionValue, easingTime));
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
                
                trans.Shininess = sourceMaterial.GetValue(ValuePropertyType._Shininess);
                trans.OutlineWidth = sourceMaterial.GetValue(ValuePropertyType._OutlineWidth);
                trans.RimPower = sourceMaterial.GetValue(ValuePropertyType._RimPower);
                trans.RimShift = sourceMaterial.GetValue(ValuePropertyType._RimShift);

                // NPR用プロパティの追加
                trans.EmissionColor = sourceMaterial.GetColor(ColorPropertyType._EmissionColor);
                trans.MatcapColor = sourceMaterial.GetColor(ColorPropertyType._MatcapColor);
                trans.ReflectionColor = sourceMaterial.GetColor(ColorPropertyType._ReflectionColor);
                
                trans.NormalValue = sourceMaterial.GetValue(ValuePropertyType._NormalValue);
                trans.ParallaxValue = sourceMaterial.GetValue(ValuePropertyType._ParallaxValue);
                trans.MatcapValue = sourceMaterial.GetValue(ValuePropertyType._MatcapValue);
                trans.MatcapMaskValue = sourceMaterial.GetValue(ValuePropertyType._MatcapMaskValue);
                trans.EmissionValue = sourceMaterial.GetValue(ValuePropertyType._EmissionValue);
                trans.EmissionHDRExposure = sourceMaterial.GetValue(ValuePropertyType._EmissionHDRExposure);
                trans.EmissionPower = sourceMaterial.GetValue(ValuePropertyType._EmissionPower);
                trans.RimLightValue = sourceMaterial.GetValue(ValuePropertyType._RimLightValue);
                trans.RimLightPower = sourceMaterial.GetValue(ValuePropertyType._RimLightPower);
                trans.MetallicValue = sourceMaterial.GetValue(ValuePropertyType._MetallicValue);
                trans.SmoothnessValue = sourceMaterial.GetValue(ValuePropertyType._SmoothnessValue);
                trans.OcclusionValue = sourceMaterial.GetValue(ValuePropertyType._OcclusionValue);
                
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

            var defaultTrans = TransformDataModelMaterial.defaultTrans;

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

                foreach (var propertyType in ModelMaterial.ValuePropertyTypes)
                {
                    if (!material.HasValue(propertyType)) continue;

                    var value = material.GetValue(propertyType);
                    var initialValue = material.GetInitialValue(propertyType);
                    var info = defaultTrans.GetCustomValueInfo(propertyType);
                    var fieldType = propertyType == ValuePropertyType._OutlineWidth ?
                        FloatFieldType.F4 : FloatFieldType.Float;

                    view.DrawLabel(info.name, 200, 20);

                    view.DrawSliderValue(new GUIView.SliderOption
                    {
                        fieldType = fieldType,
                        min = info.min,
                        max = info.max,
                        step = info.step,
                        defaultValue = initialValue,
                        value = value,
                        onChanged = newValue =>
                        {
                            material.SetValue(propertyType, newValue);
                        },
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