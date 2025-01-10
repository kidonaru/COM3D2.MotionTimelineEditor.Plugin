using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("ステージレーザー", 43)]
    public class StageLaserTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(StageLaserTimelineLayer);
        public override string layerName => nameof(StageLaserTimelineLayer);

        private List<string> _allBoneNames = null;

        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(stageLaserManager.laserNames.Count + stageLaserManager.controllerNames.Count);
                    _allBoneNames.AddRange(stageLaserManager.laserNames);
                    _allBoneNames.AddRange(stageLaserManager.controllerNames);
                }
                return _allBoneNames;
            }
        }

        private StageLaserTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static StageLaserTimelineLayer Create(int slotNo)
        {
            return new StageLaserTimelineLayer(0);
        }

        public static bool ValidateLayer()
        {
            return bundleManager.IsValid();
        }

        public override void Init()
        {
            base.Init();

            StageLaserManager.onControllerAdded += OnControllerAdded;
            StageLaserManager.onControllerRemoved += OnControllerRemoved;
            StageLaserManager.onLaserAdded += OnLaserAdded;
            StageLaserManager.onLaserRemoved += OnLaserRemoved;

            InitMenuItems();
        }

        protected override void InitMenuItems()
        {
            _allBoneNames = null;
            allMenuItems.Clear();

            foreach (var controller in stageLaserManager.controllers)
            {
                var name = "StageLaserGroup (" + controller.groupIndex + ")";
                var displayName = "グループ (" + controller.groupIndex + ")";
                var setMenuItem = new BoneSetMenuItem(name, displayName);
                allMenuItems.Add(setMenuItem);

                {
                    var menuItem = new BoneMenuItem(controller.name, controller.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                foreach (var laser in controller.lasers)
                {
                    var menuItem = new BoneMenuItem(laser.name, laser.displayName);
                    setMenuItem.AddChild(menuItem);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            StageLaserManager.onControllerAdded -= OnControllerAdded;
            StageLaserManager.onControllerRemoved -= OnControllerRemoved;
            StageLaserManager.onLaserAdded -= OnLaserAdded;
            StageLaserManager.onLaserRemoved -= OnLaserRemoved;
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

            ApplyPlayDataByType(TransformType.StageLaserController);
            ApplyPlayDataByType(TransformType.StageLaser);

            foreach (var controller in stageLaserManager.controllers)
            {
                controller.UpdateLasers();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            switch (motion.start.type)
            {
                case TransformType.StageLaser:
                    if (indexUpdated)
                    {
                        ApplyLaserMotionInit(motion, t);
                    }
                    ApplyLaserMotionUpdate(motion, t);
                    break;
                case TransformType.StageLaserController:
                    if (indexUpdated)
                    {
                        ApplyControllerMotionInit(motion, t);
                    }
                    ApplyControllerMotionUpdate(motion, t);
                    break;
            }
        }

        private void ApplyLaserMotionInit(MotionData motion, float t)
        {
            var laser = stageLaserManager.GetLaser(motion.name);
            if (laser == null)
            {
                return;
            }

            var controller = laser.controller;
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLaser;

            if (!controller.autoVisible)
            {
                laser.visible = start.visible;
            }

            if (!controller.autoRotation)
            {
                laser.eulerAngles = start.eulerAngles;
            }

            if (!controller.autoColor)
            {
                laser.color1 = start.color;
                laser.color2 = start.subColor;
            }

            if (!controller.autoLaserInfo)
            {
                laser.intensity = start.intensity;
                laser.laserRange = start.laserRange;
                laser.laserWidth = start.laserWidth;

                laser.falloffExp = start.falloffExp;
                laser.noiseStrength = start.noiseStrength;
                laser.noiseScale = start.noiseScale;
                laser.coreRadius = start.coreRadius;
                laser.offsetRange = start.offsetRange;
                laser.glowWidth = start.glowWidth;
                laser.segmentRange = start.segmentRange;
                laser.zTest = start.zTest;
            }
        }

        private void ApplyLaserMotionUpdate(MotionData motion, float t)
        {
            var laser = stageLaserManager.GetLaser(motion.name);
            if (laser == null)
            {
                return;
            }

            var controller = laser.controller;
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLaser;
            var end = motion.end as TransformDataStageLaser;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            if (!controller.autoRotation)
            {
                laser.eulerAngles = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.eulerAnglesValues,
                    end.eulerAnglesValues,
                    t
                ).ToVector3();
            }

            if (!controller.autoColor)
            {
                laser.color1 = Color.Lerp(start.color, end.color, t);
                laser.color2 = Color.Lerp(start.subColor, end.subColor, t);
            }

            if (!controller.autoLaserInfo)
            {
                laser.intensity = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.intensityValue,
                    end.intensityValue,
                    t);

                laser.laserRange = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.laserRangeValue,
                    end.laserRangeValue,
                    t);

                laser.laserWidth = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.laserWidthValue,
                    end.laserWidthValue,
                    t);
            }
        }

        private void ApplyControllerMotionInit(MotionData motion, float t)
        {
            var controller = stageLaserManager.GetController(motion.name);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLaserController;

            controller.visible = start.visible;

            controller.position = start.position;
            controller.eulerAngles = start.eulerAngles;
            controller.rotationMin = start.rotationMin;
            controller.rotationMax = start.rotationMax;
            controller.color1 = start.color;
            controller.color2 = start.subColor;

            var laserInfo = controller.laserInfo;

            laserInfo.intensity = start.intensity;
            laserInfo.laserRange = start.laserRange;
            laserInfo.laserWidth = start.laserWidth;

            laserInfo.falloffExp = start.falloffExp;
            laserInfo.noiseStrength = start.noiseStrength;
            laserInfo.noiseScale = start.noiseScale;
            laserInfo.coreRadius = start.coreRadius;
            laserInfo.offsetRange = start.offsetRange;
            laserInfo.glowWidth = start.glowWidth;
            laserInfo.segmentRange = start.segmentRange;
            laserInfo.zTest = start.zTest;

            controller.autoRotation = start.autoRotation;
            controller.autoColor = start.autoColor;
            controller.autoLaserInfo = start.autoLaserInfo;
            controller.autoVisible = start.autoVisible;
        }

        private void ApplyControllerMotionUpdate(MotionData motion, float t)
        {
            var controller = stageLaserManager.GetController(motion.name);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataStageLaserController;
            var end = motion.end as TransformDataStageLaserController;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            controller.position = PluginUtils.HermiteValues(
                t0,
                t1,
                start.positionValues,
                end.positionValues,
                t
            ).ToVector3();

            controller.eulerAngles = PluginUtils.HermiteValues(
                t0,
                t1,
                start.eulerAnglesValues,
                end.eulerAnglesValues,
                t
            ).ToVector3();

            if (controller.autoRotation)
            {
                controller.rotationMin = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.rotationMinValues,
                    end.rotationMinValues,
                    t
                ).ToVector3();

                controller.rotationMax = PluginUtils.HermiteValues(
                    t0,
                    t1,
                    start.rotationMaxValues,
                    end.rotationMaxValues,
                    t
                ).ToVector3();
            }

            if (controller.autoColor)
            {
                controller.color1 = Color.Lerp(start.color, end.color, t);
                controller.color2 = Color.Lerp(start.subColor, end.subColor, t);
            }

            var laserInfo = controller.laserInfo;

            if (controller.autoLaserInfo)
            {
                laserInfo.intensity = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.intensityValue,
                    end.intensityValue,
                    t);

                laserInfo.laserRange = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.laserRangeValue,
                    end.laserRangeValue,
                    t);

                laserInfo.laserWidth = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.laserWidthValue,
                    end.laserWidthValue,
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

        public void OnLaserAdded(string laserName)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { laserName });
            ApplyCurrentFrame(true);
        }

        public void OnLaserRemoved(string laserName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { laserName });
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var laser in stageLaserManager.lasers)
            {
                if (laser == null || laser.transform == null)
                {
                    continue;
                }

                var laserName = laser.name;

                var trans = CreateTransformData<TransformDataStageLaser>(laserName);
                trans.FromStageLaser(laser);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }

            foreach (var controller in stageLaserManager.controllers)
            {
                if (controller == null || controller.transform == null)
                {
                    continue;
                }

                var controllerName = controller.name;

                var trans = CreateTransformData<TransformDataStageLaserController>(controllerName);
                trans.FromStageLaserController(controller);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        private GUIComboBox<StageLaserController> _controllerComboBox = new GUIComboBox<StageLaserController>
        {
            getName = (laser, index) => laser.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<StageLaser> _laserComboBox = new GUIComboBox<StageLaser>
        {
            getName = (laser, index) => laser.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<StageLaserController> _copyToControllerComboBox = new GUIComboBox<StageLaserController>
        {
            getName = (laser, index) => laser.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<StageLaser> _copyToLaserComboBox = new GUIComboBox<StageLaser>
        {
            getName = (laser, index) => laser.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private ColorFieldCache _color1FieldValue = new ColorFieldCache("", true);
        private ColorFieldCache _color2FieldValue = new ColorFieldCache("", true);

        private enum TabType
        {
            一括,
            個別,
        }

        private TabType _tabType = TabType.一括;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.一括:
                    DrawStageLaserControllEdit(view);
                    break;
                case TabType.個別:
                    DrawStageLaserEdit(view);
                    break;
            }

            view.DrawComboBox();
        }
        
        public void DrawStageLaserControllEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("コントローラー数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = stageLaserManager.controllers.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    stageLaserManager.RemoveController(true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    stageLaserManager.AddController(true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            var controllers = stageLaserManager.controllers;
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

                view.DrawLabel("レーザー数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = controller.lasers.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    stageLaserManager.RemoveLaser(controller.groupIndex, true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    stageLaserManager.AddLaser(controller.groupIndex, true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var updateTransform = false;
            var defaultTrans = TransformDataStageLaserController.defaultTrans;

            {
                var initialPosition = defaultTrans.initialPosition;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = controller.position;

                view.DrawLabel("位置", 200, 20);

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    controller.position = transformCache.position;
                }
            }

            {
                var initialEulerAngles = defaultTrans.initialEulerAngles;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = controller.eulerAngles;

                view.DrawLabel("角度", 200, 20);

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    controller.name,
                    initialEulerAngles);

                if (updateTransform)
                {
                    controller.eulerAngles = transformCache.eulerAngles;
                }
            }

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

            updateTransform |= view.DrawToggle("一括角度設定", controller.autoRotation, 200, 20, newValue =>
            {
                controller.autoRotation = newValue;
            });

            if (controller.autoRotation)
            {
                var initialEulerAngles = defaultTrans.initialRotationMin;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = controller.rotationMin;

                var prevBone = GetPrevBone(timelineManager.currentFrameNo, controller.name);
                var prevTransform = prevBone != null ? prevBone.transform as TransformDataStageLaserController : null;
                var prevAngles = prevTransform != null ? prevTransform.rotationMin : initialEulerAngles;

                view.DrawLabel("最小角度", 200, 20);

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    controller.rotationMin = transformCache.eulerAngles;
                }

                initialEulerAngles = defaultTrans.initialRotationMax;
                transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = controller.rotationMax;

                prevAngles = prevTransform != null ? prevTransform.rotationMax : initialEulerAngles;

                view.DrawLabel("最大角度", 200, 20);

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevAngles,
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
                view.DrawLabel("中心色", 200, 20);

                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    controller.color1,
                    defaultTrans.initialColor,
                    c => controller.color1 = c);

                view.DrawLabel("錯乱色", 200, 20);

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    controller.color2,
                    defaultTrans.initialSubColor,
                    c => controller.color2 = c);
            }

            updateTransform |= view.DrawToggle("一括レーザー情報設定", controller.autoLaserInfo, 200, 20, newValue =>
            {
                controller.autoLaserInfo = newValue;
            });

            if (controller.autoLaserInfo)
            {
                var laserInfo = controller.laserInfo;

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.intensityInfo,
                    laserInfo.intensity,
                    x => laserInfo.intensity = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.laserRangeInfo,
                    laserInfo.laserRange,
                    x => laserInfo.laserRange = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.laserWidthInfo,
                    laserInfo.laserWidth,
                    x => laserInfo.laserWidth = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.falloffExpInfo,
                    laserInfo.falloffExp,
                    x => laserInfo.falloffExp = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseStrengthInfo,
                    laserInfo.noiseStrength,
                    x => laserInfo.noiseStrength = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseScaleInfo,
                    laserInfo.noiseScale,
                    x => laserInfo.noiseScale = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.coreRadiusInfo,
                    laserInfo.coreRadius,
                    x => laserInfo.coreRadius = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.offsetRangeInfo,
                    laserInfo.offsetRange,
                    x => laserInfo.offsetRange = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.glowWidthInfo,
                    laserInfo.glowWidth,
                    x => laserInfo.glowWidth = x);
                
                updateTransform |= view.DrawCustomValueInt(
                    defaultTrans.segmentRangeInfo,
                    laserInfo.segmentRange,
                    x => laserInfo.segmentRange = x);

                updateTransform |= view.DrawCustomValueBool(
                    defaultTrans.zTestInfo,
                    laserInfo.zTest,
                    x => laserInfo.zTest = x);
            }

            if (updateTransform)
            {
                controller.UpdateLasers();
            }

            view.DrawHorizontalLine(Color.gray);

            {
                _copyToControllerComboBox.items = controllers;
                _copyToControllerComboBox.DrawButton("コピー先", view);

                var copyToController = _copyToControllerComboBox.currentItem;

                if (view.DrawButton("コピー", 60, 20))
                {
                    if (copyToController != null && copyToController != controller)
                    {
                        copyToController.CopyFrom(controller);
                        copyToController.UpdateLasers();
                    }
                }

                view.DrawHorizontalLine(Color.gray);
                view.AddSpace(5);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawStageLaserEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            var controllers = stageLaserManager.controllers;
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

            var lasers = controller.lasers;
            if (lasers.Count == 0)
            {
                view.DrawLabel("レーザーが存在しません", 200, 20);
                return;
            }

            _laserComboBox.items = lasers;
            _laserComboBox.DrawButton("操作対象", view);

            var laser = _laserComboBox.currentItem;

            if (laser == null || laser.transform == null)
            {
                view.DrawLabel("レーザーを選択してください", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            view.DrawLabel(laser.displayName, 200, 20);

            if (!controller.autoVisible)
            {
                view.DrawToggle("表示", laser.visible, 120, 20, newValue =>
                {
                    laser.visible = newValue;
                });
            }

            var transformCache = view.GetTransformCache();
            var defaultTrans = TransformDataStageLaser.defaultTrans;
            transformCache.eulerAngles = laser.eulerAngles;
            var initialPosition = defaultTrans.initialPosition;
            var initialEulerAngles = defaultTrans.initialEulerAngles;
            var initialScale = Vector3.one;
            var updateTransform = false;
            var editType = TransformEditType.全て;

            if (!controller.autoRotation)
            {
                updateTransform |= DrawEulerAngles(view, transformCache, editType, laser.name, initialEulerAngles);
            }

            if (updateTransform)
            {
                laser.eulerAngles = transformCache.eulerAngles;
            }

            if (!controller.autoColor)
            {
                view.DrawLabel("中心色", 200, 20);

                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    laser.color1,
                    Color.white,
                    c => laser.color1 = c);

                view.DrawLabel("錯乱色", 200, 20);

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    laser.color2,
                    Color.white,
                    c => laser.color2 = c);
            }

            if (!controller.autoLaserInfo)
            {
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.intensityInfo,
                    laser.intensity,
                    x => laser.intensity = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.laserRangeInfo,
                    laser.laserRange,
                    x => laser.laserRange = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.laserWidthInfo,
                    laser.laserWidth,
                    x => laser.laserWidth = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.falloffExpInfo,
                    laser.falloffExp,
                    x => laser.falloffExp = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseStrengthInfo,
                    laser.noiseStrength,
                    x => laser.noiseStrength = x);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.noiseScaleInfo,
                    laser.noiseScale,
                    x => laser.noiseScale = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.coreRadiusInfo,
                    laser.coreRadius,
                    x => laser.coreRadius = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.offsetRangeInfo,
                    laser.offsetRange,
                    x => laser.offsetRange = x);
                
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.glowWidthInfo,
                    laser.glowWidth,
                    x => laser.glowWidth = x);
                
                updateTransform |= view.DrawCustomValueInt(
                    defaultTrans.segmentRangeInfo,
                    laser.segmentRange,
                    x => laser.segmentRange = x);

                updateTransform |= view.DrawCustomValueBool(
                    defaultTrans.zTestInfo,
                    laser.zTest,
                    x => laser.zTest = x);
            }

            view.DrawHorizontalLine(Color.gray);

            {
                _copyToLaserComboBox.items = lasers;
                _copyToLaserComboBox.DrawButton("コピー先", view);

                var copyToLaser = _copyToLaserComboBox.currentItem;

                if (view.DrawButton("コピー", 60, 20))
                {
                    if (copyToLaser != null && copyToLaser != laser)
                    {
                        copyToLaser.CopyFrom(laser);
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
            if (name.StartsWith("StageLaserController", StringComparison.Ordinal))
            {
                return TransformType.StageLaserController;
            }
            else
            {
                return TransformType.StageLaser;
            }
        }
    }
}