using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("ステージライト", 42)]
    public class StageLightTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(StageLightTimelineLayer);
        public override string layerName => nameof(StageLightTimelineLayer);

        private List<string> _allBoneNames = null;

        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(stageLightManager.lightNames.Count + stageLightManager.controllerNames.Count);
                    _allBoneNames.AddRange(stageLightManager.lightNames);
                    _allBoneNames.AddRange(stageLightManager.controllerNames);
                }
                return _allBoneNames;
            }
        }

        private StageLightTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static StageLightTimelineLayer Create(int slotNo)
        {
            return new StageLightTimelineLayer(0);
        }

        public static bool ValidateLayer()
        {
            return bundleManager.IsValid();
        }

        public override void Init()
        {
            base.Init();

            StageLightManager.onControllerAdded += OnControllerAdded;
            StageLightManager.onControllerRemoved += OnControllerRemoved;
            StageLightManager.onLightAdded += OnLightAdded;
            StageLightManager.onLightRemoved += OnLightRemoved;

            InitMenuItems();
        }

        protected override void InitMenuItems()
        {
            _allBoneNames = null;
            allMenuItems.Clear();

            foreach (var controller in stageLightManager.controllers)
            {
                var name = "StageLightGroup (" + controller.groupIndex + ")";
                var displayName = "グループ (" + controller.groupIndex + ")";
                var setMenuItem = new BoneSetMenuItem(name, displayName);
                allMenuItems.Add(setMenuItem);

                {
                    var menuItem = new BoneMenuItem(controller.name, controller.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                foreach (var light in controller.lights)
                {
                    var menuItem = new BoneMenuItem(light.name, light.displayName);
                    setMenuItem.AddChild(menuItem);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            StageLightManager.onControllerAdded -= OnControllerAdded;
            StageLightManager.onControllerRemoved -= OnControllerRemoved;
            StageLightManager.onLightAdded -= OnLightAdded;
            StageLightManager.onLightRemoved -= OnLightRemoved;
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        public override void Update()
        {
            base.Update();

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        protected override void ApplyPlayData()
        {
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return;
            }

            ApplyPlayDataByType(TransformType.StageLightController);
            ApplyPlayDataByType(TransformType.StageLight);

            foreach (var controller in stageLightManager.controllers)
            {
                controller.UpdateLights();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated, MotionPlayData playData)
        {
            switch (motion.start.type)
            {
                case TransformType.StageLight:
                    if (indexUpdated)
                    {
                        ApplyLightMotionInit(motion, t);
                    }
                    ApplyLightMotionUpdate(motion, t);
                    break;
                case TransformType.StageLightController:
                    if (indexUpdated)
                    {
                        ApplyControllerMotionInit(motion, t);
                    }
                    ApplyControllerMotionUpdate(motion, t);
                    break;
            }
        }

        private void ApplyLightMotionInit(MotionData motion, float t)
        {
            var light = stageLightManager.GetLight(motion.name);
            if (light == null)
            {
                return;
            }

            var controller = light.controller;
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLight;

            if (!controller.autoVisible)
            {
                light.visible = start.visible;
            }

            if (!controller.autoPosition)
            {
                light.position = start.position;
            }

            if (!controller.autoRotation)
            {
                light.eulerAngles = start.eulerAngles;
            }

            if (!controller.autoColor)
            {
                light.color = start.color;
            }

            if (!controller.autoLightInfo)
            {
                light.spotAngle = start.spotAngle;
                light.spotRange = start.spotRange;

                light.rangeMultiplier = start.rangeMultiplier;
                light.falloffExp = start.falloffExp;
                light.noiseStrength = start.noiseStrength;
                light.noiseScale = start.noiseScale;
                light.coreRadius = start.coreRadius;
                light.offsetRange = start.offsetRange;
                light.segmentAngle = start.segmentAngle;
                light.segmentRange = start.segmentRange;
                light.zTest = start.zTest;
            }
        }

        private void ApplyLightMotionUpdate(MotionData motion, float t)
        {
            var light = stageLightManager.GetLight(motion.name);
            if (light == null)
            {
                return;
            }

            var controller = light.controller;
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLight;
            var end = motion.end as TransformDataStageLight;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            if (!controller.autoVisible)
            {
                light.visible = start.visible;
            }

            if (!controller.autoPosition)
            {
                light.position = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    t
                ).ToVector3();
            }

            if (!controller.autoRotation)
            {
                light.eulerAngles = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.eulerAnglesValues,
                    end.eulerAnglesValues,
                    t
                ).ToVector3();
            }

            if (!controller.autoColor)
            {
                light.color = Color.Lerp(start.color, end.color, t);
            }

            if (!controller.autoLightInfo)
            {
                light.spotAngle = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.spotAngleValue,
                    end.spotAngleValue,
                    t);

                light.spotRange = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.spotRangeValue,
                    end.spotRangeValue,
                    t);
            }
        }

        private void ApplyControllerMotionInit(MotionData motion, float t)
        {
            var controller = stageLightManager.GetController(motion.name);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLightController;

            controller.visible = start.visible;

            controller.positionMin = start.position;
            controller.positionMax = start.subPosition;
            controller.rotationMin = start.eulerAngles;
            controller.rotationMax = start.subEulerAngles;
            controller.colorMin = start.color;
            controller.colorMax = start.subColor;

            var lightInfo = controller.lightInfo;

            lightInfo.spotAngle = start.spotAngle;
            lightInfo.spotRange = start.spotRange;

            lightInfo.rangeMultiplier = start.rangeMultiplier;
            lightInfo.falloffExp = start.falloffExp;
            lightInfo.noiseStrength = start.noiseStrength;
            lightInfo.noiseScale = start.noiseScale;
            lightInfo.coreRadius = start.coreRadius;
            lightInfo.offsetRange = start.offsetRange;
            lightInfo.segmentAngle = start.segmentAngle;
            lightInfo.segmentRange = start.segmentRange;
            lightInfo.zTest = start.zTest;

            controller.autoPosition = start.autoPosition;
            controller.autoRotation = start.autoRotation;
            controller.autoColor = start.autoColor;
            controller.autoLightInfo = start.autoLightInfo;
            controller.autoVisible = start.autoVisible;
        }

        private void ApplyControllerMotionUpdate(MotionData motion, float t)
        {
            var controller = stageLightManager.GetController(motion.name);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLightController;
            var end = motion.end as TransformDataStageLightController;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            if (controller.autoPosition)
            {
                controller.positionMin = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    t
                ).ToVector3();

                controller.positionMax = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.subPositionValues,
                    end.subPositionValues,
                    t
                ).ToVector3();
            }

            if (controller.autoRotation)
            {
                controller.rotationMin = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.eulerAnglesValues,
                    end.eulerAnglesValues,
                    t
                ).ToVector3();

                controller.rotationMax = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.subEulerAnglesValues,
                    end.subEulerAnglesValues,
                    t
                ).ToVector3();
            }

            if (controller.autoColor)
            {
                controller.colorMin = Color.Lerp(start.color, end.color, t);
                controller.colorMax = Color.Lerp(start.subColor, end.subColor, t);
            }

            var lightInfo = controller.lightInfo;

            if (controller.autoLightInfo)
            {
                lightInfo.spotAngle = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.spotAngleValue,
                    end.spotAngleValue,
                    t);

                lightInfo.spotRange = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.spotRangeValue,
                    end.spotRangeValue,
                    t);
            }
        }

        public void OnControllerAdded(string controllerName)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { controllerName });
            ApplyCurrentFrame(true);
        }

        public void OnControllerRemoved(string controllerName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { controllerName });
            ApplyCurrentFrame(true);
        }

        public void OnLightAdded(string lightName)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { lightName });
            ApplyCurrentFrame(true);
        }

        public void OnLightRemoved(string lightName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { lightName });
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            foreach (var light in stageLightManager.lights)
            {
                if (light == null || light.transform == null)
                {
                    continue;
                }

                var lightName = light.name;

                var trans = CreateTransformData<TransformDataStageLight>(lightName);
                trans.FromStageLight(light);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }

            foreach (var controller in stageLightManager.controllers)
            {
                if (controller == null || controller.transform == null)
                {
                    continue;
                }

                var controllerName = controller.name;

                var trans = CreateTransformData<TransformDataStageLightController>(controllerName);
                trans.FromStageLightController(controller);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        private GUIComboBox<StageLightController> _controllerComboBox = new GUIComboBox<StageLightController>
        {
            getName = (light, index) => light.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<StageLight> _lightComboBox = new GUIComboBox<StageLight>
        {
            getName = (light, index) => light.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<StageLight> _copyToLightComboBox = new GUIComboBox<StageLight>
        {
            getName = (light, index) => light.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private ColorFieldCache _color1FieldValue = new ColorFieldCache("Color1", true);
        private ColorFieldCache _color2FieldValue = new ColorFieldCache("Color2", true);

        private enum TabType
        {
            一括,
            個別,
        }

        private static TabType _tabType = TabType.一括;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.一括:
                    DrawStageLightControllEdit(view);
                    break;
                case TabType.個別:
                    DrawStageLightEdit(view);
                    break;
            }

            view.DrawComboBox();
        }
        
        public void DrawStageLightControllEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("コントローラー数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = stageLightManager.controllers.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    stageLightManager.RemoveController(true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    stageLightManager.AddController(true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            var controllers = stageLightManager.controllers;
            if (controllers.Count == 0)
            {
                view.DrawLabel("コントローラーが存在しません", 200, 20);
                return;
            }

            _controllerComboBox.items = controllers;
            _controllerComboBox.DrawButton("操作対象", view);

            var controller = _controllerComboBox.currentItem;
            if (controller == null)
            {
                view.DrawLabel("コントローラーを選択してください", 200, 20);
                return;
            }
            
            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("ライト数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = controller.lights.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    stageLightManager.RemoveLight(controller.groupIndex, true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    stageLightManager.AddLight(controller.groupIndex, true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var updateTransform = false;
            var defaultTrans = TransformDataStageLightController.defaultTrans;

            updateTransform |= view.DrawToggle("一括表示設定", controller.autoVisible, 200, 20, newValue =>
            {
                controller.autoVisible = newValue;
            });

            if (controller.autoVisible)
            {
                updateTransform |= view.DrawToggle("表示", controller.visible, 120, 20, newValue =>
                {
                    controller.visible = newValue;
                });
            }

            updateTransform |= view.DrawToggle("一括位置設定", controller.autoPosition, 200, 20, newValue =>
            {
                controller.autoPosition = newValue;
            });

            if (controller.autoPosition)
            {
                var initialPosition = defaultTrans.initialPosition;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = controller.positionMin;

                view.DrawLabel("最小位置", 200, 20);

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    controller.positionMin = transformCache.position;
                }

                initialPosition = defaultTrans.initialSubPosition;
                transformCache = view.GetTransformCache(null);
                transformCache.position = controller.positionMax;

                view.DrawLabel("最大位置", 200, 20);

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    controller.positionMax = transformCache.position;
                }
            }

            updateTransform |= view.DrawToggle("一括角度設定", controller.autoRotation, 200, 20, newValue =>
            {
                controller.autoRotation = newValue;
            });

            if (controller.autoRotation)
            {
                var initialEulerAngles = defaultTrans.initialEulerAngles;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = controller.rotationMin;

                view.DrawLabel("最小角度", 200, 20);

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    controller.name,
                    initialEulerAngles);

                if (updateTransform)
                {
                    controller.rotationMin = transformCache.eulerAngles;
                }

                transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = controller.rotationMax;

                view.DrawLabel("最大角度", 200, 20);

                updateTransform |= DrawSubEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    controller.name,
                    initialEulerAngles);

                if (updateTransform)
                {
                    controller.rotationMax = transformCache.eulerAngles;
                }
            }

            updateTransform |= view.DrawToggle("一括色設定", controller.autoColor, 200, 20, newValue =>
            {
                controller.autoColor = newValue;
            });

            if (controller.autoColor)
            {
                view.DrawLabel("最小色", 200, 20);

                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    controller.colorMin,
                    defaultTrans.initialColor,
                    c => controller.colorMin = c);

                view.DrawLabel("最大色", 200, 20);

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    controller.colorMax,
                    defaultTrans.initialSubColor,
                    c => controller.colorMax = c);
            }

            updateTransform |= view.DrawToggle("一括ライト情報設定", controller.autoLightInfo, 200, 20, newValue =>
            {
                controller.autoLightInfo = newValue;
            });

            if (controller.autoLightInfo)
            {
                var lightInfo = controller.lightInfo;

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.spotAngleInfo,
                    lightInfo.spotAngle,
                    x => lightInfo.spotAngle = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.spotRangeInfo,
                    lightInfo.spotRange,
                    x => lightInfo.spotRange = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.rangeMultiplierInfo,
                    lightInfo.rangeMultiplier,
                    x => lightInfo.rangeMultiplier = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.falloffExpInfo,
                    lightInfo.falloffExp,
                    x => lightInfo.falloffExp = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseStrengthInfo,
                    lightInfo.noiseStrength,
                    x => lightInfo.noiseStrength = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseScaleInfo,
                    lightInfo.noiseScale,
                    x => lightInfo.noiseScale = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.coreRadiusInfo,
                    lightInfo.coreRadius,
                    x => lightInfo.coreRadius = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.offsetRangeInfo,
                    lightInfo.offsetRange,
                    x => lightInfo.offsetRange = x);
                
                updateTransform |= view.DrawCustomValueInt(
                    defaultTrans.segmentAngleInfo,
                    lightInfo.segmentAngle,
                    x => lightInfo.segmentAngle = x);
                
                updateTransform |= view.DrawCustomValueInt(
                    defaultTrans.segmentRangeInfo,
                    lightInfo.segmentRange,
                    x => lightInfo.segmentRange = x);

                updateTransform |= view.DrawCustomValueBool(
                    defaultTrans.zTestInfo,
                    lightInfo.zTest,
                    x => lightInfo.zTest = x);
            }

            if (updateTransform)
            {
                controller.UpdateLights();
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawStageLightEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            var controllers = stageLightManager.controllers;
            if (controllers.Count == 0)
            {
                view.DrawLabel("コントローラーが存在しません", 200, 20);
                return;
            }

            _controllerComboBox.items = controllers;
            _controllerComboBox.DrawButton("操作対象", view);

            var controller = _controllerComboBox.currentItem;
            if (controller == null)
            {
                view.DrawLabel("コントローラーを選択してください", 200, 20);
                return;
            }

            var lights = controller.lights;
            if (lights.Count == 0)
            {
                view.DrawLabel("ライトが存在しません", 200, 20);
                return;
            }

            _lightComboBox.items = lights;
            _lightComboBox.DrawButton("操作対象", view);

            var light = _lightComboBox.currentItem;

            if (light == null || light.transform == null)
            {
                view.DrawLabel("ライトを選択してください", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            view.DrawLabel(light.displayName, 200, 20);

            if (!controller.autoVisible)
            {
                view.DrawToggle("表示", light.visible, 120, 20, newValue =>
                {
                    light.visible = newValue;
                });
            }

            var transformCache = view.GetTransformCache();
            var defaultTrans = TransformDataStageLight.defaultTrans;
            transformCache.position = light.position;
            transformCache.eulerAngles = light.eulerAngles;
            var initialPosition = defaultTrans.initialPosition;
            var initialEulerAngles = defaultTrans.initialEulerAngles;
            var initialScale = Vector3.one;
            var updateTransform = false;
            var editType = TransformEditType.全て;

            if (!controller.autoPosition)
            {
                updateTransform |= DrawPosition(view, transformCache, editType, initialPosition);
            }

            if (!controller.autoRotation)
            {
                updateTransform |= DrawEulerAngles(view, transformCache, editType, light.name, initialEulerAngles);
            }

            if (updateTransform)
            {
                light.position = transformCache.position;
                light.eulerAngles = transformCache.eulerAngles;
            }

            if (!controller.autoColor)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    light.color,
                    Color.white,
                    c => light.color = c);
            }

            if (!controller.autoLightInfo)
            {
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.spotAngleInfo,
                    light.spotAngle,
                    x => light.spotAngle = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.spotRangeInfo,
                    light.spotRange,
                    x => light.spotRange = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.rangeMultiplierInfo,
                    light.rangeMultiplier,
                    x => light.rangeMultiplier = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.falloffExpInfo,
                    light.falloffExp,
                    x => light.falloffExp = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseStrengthInfo,
                    light.noiseStrength,
                    x => light.noiseStrength = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseScaleInfo,
                    light.noiseScale,
                    x => light.noiseScale = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.coreRadiusInfo,
                    light.coreRadius,
                    x => light.coreRadius = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.offsetRangeInfo,
                    light.offsetRange,
                    x => light.offsetRange = x);
                
                updateTransform |= view.DrawCustomValueInt(
                    defaultTrans.segmentAngleInfo,
                    light.segmentAngle,
                    x => light.segmentAngle = x);
                
                updateTransform |= view.DrawCustomValueInt(
                    defaultTrans.segmentRangeInfo,
                    light.segmentRange,
                    x => light.segmentRange = x);

                updateTransform |= view.DrawCustomValueBool(
                    defaultTrans.zTestInfo,
                    light.zTest,
                    x => light.zTest = x);
            }

            view.DrawHorizontalLine(Color.gray);

            {
                _copyToLightComboBox.items = lights;
                _copyToLightComboBox.DrawButton("コピー先", view);

                var copyToLight = _copyToLightComboBox.currentItem;

                if (view.DrawButton("コピー", 60, 20))
                {
                    if (copyToLight != null && copyToLight != light)
                    {
                        copyToLight.CopyFrom(light);
                    }
                }

                view.DrawHorizontalLine(Color.gray);
                view.AddSpace(5);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            if (name.StartsWith("StageLightController", StringComparison.Ordinal))
            {
                return TransformType.StageLightController;
            }
            else
            {
                return TransformType.StageLight;
            }
        }
    }
}