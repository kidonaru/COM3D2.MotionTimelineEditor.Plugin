using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("サイリウム", 44)]
    public class PsylliumTimelineLayer : TimelineLayerBase
    {
        public override string className => typeof(PsylliumTimelineLayer).Name;

        private List<string> _allBoneNames = null;

        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(
                        psylliumManager.controllerNames.Count +
                        psylliumManager.barConfigNames.Count +
                        psylliumManager.handConfigNames.Count +
                        psylliumManager.animationConfigNames.Count + 
                        psylliumManager.areaNames.Count);

                    _allBoneNames.AddRange(psylliumManager.controllerNames);
                    _allBoneNames.AddRange(psylliumManager.barConfigNames);
                    _allBoneNames.AddRange(psylliumManager.handConfigNames);
                    _allBoneNames.AddRange(psylliumManager.animationConfigNames);
                    _allBoneNames.AddRange(psylliumManager.animationHandConfigNames);
                    _allBoneNames.AddRange(psylliumManager.areaNames);
                }
                return _allBoneNames;
            }
        }

        private PsylliumTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static PsylliumTimelineLayer Create(int slotNo)
        {
            return new PsylliumTimelineLayer(0);
        }

        public static bool ValidateLayer()
        {
            return bundleManager.IsValid();
        }

        public override void Init()
        {
            base.Init();

            PsylliumManager.onSetup += OnAreaSetup;
            PsylliumManager.onControllerAdded += OnControllerAdded;
            PsylliumManager.onControllerRemoved += OnControllerRemoved;
            PsylliumManager.onAreaAdded += OnAreaAdded;
            PsylliumManager.onAreaRemoved += OnAreaRemoved;

            InitMenuItems();
        }

        protected override void InitMenuItems()
        {
            _allBoneNames = null;
            allMenuItems.Clear();

            foreach (var controller in psylliumManager.controllers)
            {
                var name = "PsylliumGroup (" + controller.groupIndex + ")";
                var displayName = "グループ (" + controller.groupIndex + ")";
                var setMenuItem = new BoneSetMenuItem(name, displayName);
                allMenuItems.Add(setMenuItem);

                {
                    var menuItem = new BoneMenuItem(controller.name, controller.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                {
                    var barConfig = controller.barConfig;
                    var menuItem = new BoneMenuItem(barConfig.name, barConfig.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                {
                    var handConfig = controller.handConfig;
                    var menuItem = new BoneMenuItem(handConfig.name, handConfig.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                {
                    var animationConfig = controller.animationConfig;
                    var menuItem = new BoneMenuItem(animationConfig.name, animationConfig.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                {
                    var animationHandConfigLeft = controller.animationHandConfigLeft;
                    var menuItem = new BoneMenuItem(animationHandConfigLeft.name, animationHandConfigLeft.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                {
                    var animationHandConfigRight = controller.animationHandConfigRight;
                    var menuItem = new BoneMenuItem(animationHandConfigRight.name, animationHandConfigRight.displayName);
                    setMenuItem.AddChild(menuItem);
                }

                foreach (var area in controller.areas)
                {
                    var menuItem = new BoneMenuItem(area.name, area.displayName);
                    setMenuItem.AddChild(menuItem);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            PsylliumManager.onSetup -= OnAreaSetup;
            PsylliumManager.onControllerAdded -= OnControllerAdded;
            PsylliumManager.onControllerRemoved -= OnControllerRemoved;
            PsylliumManager.onAreaAdded -= OnAreaAdded;
            PsylliumManager.onAreaRemoved -= OnAreaRemoved;
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        public override void Update()
        {
            base.Update();

            if (!studioHack.isPoseEditing)
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
            base.ApplyPlayData();

            var playingTime = this.playingTime;
            foreach (var controller in psylliumManager.controllers)
            {
                controller.ManualUpdate(playingTime);
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            switch (motion.start.type)
            {
                case TransformType.PsylliumController:
                    if (indexUpdated)
                    {
                        ApplyControllerMotionInit(motion, t);
                    }
                    break;
                case TransformType.PsylliumBar:
                    if (indexUpdated)
                    {
                        ApplyBarConfigMotionInit(motion, t);
                    }
                    break;
                case TransformType.PsylliumHand:
                    if (indexUpdated)
                    {
                        ApplyHandConfigMotionInit(motion, t);
                    }
                    break;
                case TransformType.PsylliumAnimation:
                    if (indexUpdated)
                    {
                        ApplyAnimationConfigMotionInit(motion, t);
                    }
                    ApplyAnimationConfigMotionUpdate(motion, t);
                    break;
                case TransformType.PsylliumAnimationHand:
                    if (indexUpdated)
                    {
                        ApplyAnimationHandConfigMotionInit(motion, t);
                    }
                    ApplyAnimationHandConfigMotionUpdate(motion, t);
                    break;
                case TransformType.PsylliumArea:
                    if (indexUpdated)
                    {
                        ApplyAreaMotionInit(motion, t);
                    }
                    break;
            }
        }

        private void ApplyControllerMotionInit(MotionData motion, float t)
        {
            var controller = psylliumManager.GetController(motion.name);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumController;

            controller.visible = start.visible;
            controller.position = start.position;
            controller.eulerAngles = start.eulerAngles;

            controller.refreshRequired = true;
        }

        private void ApplyBarConfigMotionInit(MotionData motion, float t)
        {
            var barConfig = psylliumManager.GetBarConfig(motion.name);
            if (barConfig == null)
            {
                return;
            }

            var controller = psylliumManager.GetController(barConfig.groupIndex);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumBar;
            var targetConfig = start.ToConfig();

            if (targetConfig.Equals(barConfig))
            {
                return;
            }

            barConfig.CopyFrom(targetConfig);
            controller.refreshRequired = true;
        }

        private void ApplyHandConfigMotionInit(MotionData motion, float t)
        {
            var handConfig = psylliumManager.GetHandConfig(motion.name);
            if (handConfig == null)
            {
                return;
            }

            var controller = psylliumManager.GetController(handConfig.groupIndex);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumHand;
            var targetConfig = start.ToConfig();

            if (targetConfig.Equals(handConfig))
            {
                return;
            }

            handConfig.CopyFrom(targetConfig);
            controller.refreshRequired = true;
        }

        private void ApplyAnimationConfigMotionInit(MotionData motion, float t)
        {
            var animationConfig = psylliumManager.GetAnimationConfig(motion.name);
            if (animationConfig == null)
            {
                return;
            }

            var controller = psylliumManager.GetController(animationConfig.groupIndex);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumAnimation;
            var targetConfig = start.ToConfig();

            if (targetConfig.Equals(animationConfig))
            {
                return;
            }

            animationConfig.CopyFrom(targetConfig);
        }

        private void ApplyAnimationConfigMotionUpdate(MotionData motion, float t)
        {
            var animationConfig = psylliumManager.GetAnimationConfig(motion.name);
            if (animationConfig == null)
            {
                return;
            }

            var controller = psylliumManager.GetController(animationConfig.groupIndex);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumAnimation;
            var end = motion.end as TransformDataPsylliumAnimation;

            var startConfig = start.ToConfig();
            var endConfig = end.ToConfig();

            if (startConfig.randomPosition1Range != endConfig.randomPosition1Range)
            {
                animationConfig.randomPosition1Range = Vector3.Lerp(startConfig.randomPosition1Range, endConfig.randomPosition1Range, t);
            }

            if (startConfig.randomPosition2Range != endConfig.randomPosition2Range)
            {
                animationConfig.randomPosition2Range = Vector3.Lerp(startConfig.randomPosition2Range, endConfig.randomPosition2Range, t);
            }

            if (startConfig.randomEulerAnglesRange != endConfig.randomEulerAnglesRange)
            {
                animationConfig.randomEulerAnglesRange = Vector3.Lerp(startConfig.randomEulerAnglesRange, endConfig.randomEulerAnglesRange, t);
            }

            if (startConfig.positionSyncRate != endConfig.positionSyncRate)
            {
                animationConfig.positionSyncRate = Mathf.Lerp(startConfig.positionSyncRate, endConfig.positionSyncRate, t);
            }

            if (startConfig.bpm != endConfig.bpm)
            {
                animationConfig.bpm = Mathf.Lerp(startConfig.bpm, endConfig.bpm, t);
            }

            if (startConfig.timeRatio != endConfig.timeRatio)
            {
                animationConfig.timeRatio = Mathf.Lerp(startConfig.timeRatio, endConfig.timeRatio, t);
            }

            if (startConfig.timeOffset != endConfig.timeOffset)
            {
                animationConfig.timeOffset = Mathf.Lerp(startConfig.timeOffset, endConfig.timeOffset, t);
            }
        }

        private void ApplyAnimationHandConfigMotionInit(MotionData motion, float t)
        {
            var animationConfig = psylliumManager.GetAnimationHandConfig(motion.name);
            if (animationConfig == null)
            {
                return;
            }

            var controller = psylliumManager.GetController(animationConfig.groupIndex);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumAnimationHand;
            var targetConfig = start.ToConfig();

            if (targetConfig.Equals(animationConfig))
            {
                return;
            }

            animationConfig.CopyFrom(targetConfig);
        }

        private void ApplyAnimationHandConfigMotionUpdate(MotionData motion, float t)
        {
            var animationConfig = psylliumManager.GetAnimationHandConfig(motion.name);
            if (animationConfig == null)
            {
                return;
            }

            var controller = psylliumManager.GetController(animationConfig.groupIndex);
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumAnimationHand;
            var end = motion.end as TransformDataPsylliumAnimationHand;

            var startConfig = start.ToConfig();
            var endConfig = end.ToConfig();

            if (startConfig.position1 != endConfig.position1)
            {
                animationConfig.position1 = Vector3.Lerp(startConfig.position1, endConfig.position1, t);
            }

            if (startConfig.position2 != endConfig.position2)
            {
                animationConfig.position2 = Vector3.Lerp(startConfig.position2, endConfig.position2, t);
            }

            if (startConfig.eulerAngles1 != endConfig.eulerAngles1)
            {
                animationConfig.eulerAngles1 = Vector3.Lerp(startConfig.eulerAngles1, endConfig.eulerAngles1, t);
            }

            if (startConfig.eulerAngles2 != endConfig.eulerAngles2)
            {
                animationConfig.eulerAngles2 = Vector3.Lerp(startConfig.eulerAngles2, endConfig.eulerAngles2, t);
            }
        }

        private void ApplyAreaMotionInit(MotionData motion, float t)
        {
            var area = psylliumManager.GetArea(motion.name);
            if (area == null)
            {
                return;
            }

            var controller = area.controller;
            if (controller == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumArea;
            var config = start.ToConfig();

            if (config.Equals(area.areaConfig))
            {
                return;
            }

            area.areaConfig = config;
            area.refreshRequired = true;
        }

        public void OnAreaSetup()
        {
            InitMenuItems();
            AddFirstBones(allBoneNames);
            ApplyCurrentFrame(true);
        }

        public void OnControllerAdded(string controllerName)
        {
            InitMenuItems();
            AddFirstBones(allBoneNames);
            ApplyCurrentFrame(true);
        }

        public void OnControllerRemoved(string controllerName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { controllerName }); // TODO: 他のボーンも削除する
            ApplyCurrentFrame(true);
        }

        public void OnAreaAdded(string areaName)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { areaName });
            ApplyCurrentFrame(true);
        }

        public void OnAreaRemoved(string areaName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { areaName });
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var area in psylliumManager.areas)
            {
                if (area == null || area.transform == null)
                {
                    continue;
                }

                var areaName = area.name;

                var trans = frame.GetOrCreateTransformData<TransformDataPsylliumArea>(areaName);
                trans.FromConfig(area.areaConfig);
            }

            foreach (var controller in psylliumManager.controllers)
            {
                if (controller == null || controller.transform == null)
                {
                    continue;
                }

                {
                    var controllerName = controller.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumController>(controllerName);
                    trans.position = controller.position;
                    trans.eulerAngles = controller.eulerAngles;
                    trans.visible = controller.visible;
                }

                {
                    var config = controller.barConfig;
                    var barConfigName = config.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumBar>(barConfigName);
                    trans.FromConfig(config);
                }

                {
                    var config = controller.handConfig;
                    var handConfigName = config.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumHand>(handConfigName);
                    trans.FromConfig(config);
                }

                {
                    var config = controller.animationConfig;
                    var animationConfigName = config.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumAnimation>(animationConfigName);
                    trans.FromConfig(config);
                }

                {
                    var config = controller.animationHandConfigLeft;
                    var animationConfigName = config.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumAnimationHand>(animationConfigName);
                    trans.FromConfig(config);
                }

                {
                    var config = controller.animationHandConfigRight;
                    var animationConfigName = config.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumAnimationHand>(animationConfigName);
                    trans.FromConfig(config);
                }
            }
        }

        private GUIComboBox<PsylliumController> _controllerComboBox = new GUIComboBox<PsylliumController>
        {
            getName = (area, index) => area.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<PsylliumArea> _areaComboBox = new GUIComboBox<PsylliumArea>
        {
            getName = (area, index) => area.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<PsylliumController> _copyToControllerComboBox = new GUIComboBox<PsylliumController>
        {
            getName = (area, index) => area.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<PsylliumArea> _copyToAreaComboBox = new GUIComboBox<PsylliumArea>
        {
            getName = (area, index) => area.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<MoveEasingType> _easingType1ComboBox = new GUIComboBox<MoveEasingType>
        {
            items = Enum.GetValues(typeof(MoveEasingType)).Cast<MoveEasingType>().ToList(),
            getName = (type, index) => type.ToString(),
        };

        private GUIComboBox<MoveEasingType> _easingType2ComboBox = new GUIComboBox<MoveEasingType>
        {
            items = Enum.GetValues(typeof(MoveEasingType)).Cast<MoveEasingType>().ToList(),
            getName = (type, index) => type.ToString(),
        };

        private ColorFieldCache _color1aFieldValue = new ColorFieldCache("", true);
        private ColorFieldCache _color1bFieldValue = new ColorFieldCache("", true);
        private ColorFieldCache _color1cFieldValue = new ColorFieldCache("", true);
        private ColorFieldCache _color2aFieldValue = new ColorFieldCache("", true);
        private ColorFieldCache _color2bFieldValue = new ColorFieldCache("", true);
        private ColorFieldCache _color2cFieldValue = new ColorFieldCache("", true);

        private enum TabType
        {
            基本,
            バー,
            持ち手,
            アニメ,
            エリア,
        }

        private enum HandTabType
        {
            両手,
            左手,
            右手,
        }

        private TabType _tabType = TabType.基本;
        private HandTabType _handTabType = HandTabType.両手;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.基本:
                    DrawPsylliumControllEdit(view);
                    break;
                case TabType.バー:
                    DrawPsylliumBarConfigEdit(view);
                    break;
                case TabType.持ち手:
                    DrawPsylliumHandConfigEdit(view);
                    break;
                case TabType.アニメ:
                    DrawPsylliumAnimationConfigEdit(view);
                    break;
                case TabType.エリア:
                    DrawPsylliumAreaEdit(view);
                    break;
            }

            view.DrawComboBox();
        }

        public void DrawPsylliumControllEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("コントローラー数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = psylliumManager.controllers.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    psylliumManager.RemoveController(true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    psylliumManager.AddController(true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            var controllers = psylliumManager.controllers;
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

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumController.defaultTrans;
            var transformCache = view.GetTransformCache(null);

            {
                var initialPosition = defaultTrans.initialPosition;
                transformCache.position = controller.position;

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
                transformCache.eulerAngles = controller.eulerAngles;

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

            updateTransform |= view.DrawToggle("表示", controller.visible, 200, 20, newValue =>
            {
                controller.visible = newValue;
            });

            if (updateTransform)
            {
                controller.Refresh();
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
                        copyToController.Refresh();
                    }
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawPsylliumBarConfigEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            var controllers = psylliumManager.controllers;
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

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var barConfig = controller.barConfig;
            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumBar.defaultTrans;

            view.DrawLabel("中心色1", 200, 20);

            updateTransform |= view.DrawColor(
                _color1aFieldValue,
                barConfig.color1a,
                Color.white,
                c => barConfig.color1a = c);

            view.DrawLabel("縁色1", 200, 20);

            updateTransform |= view.DrawColor(
                _color1bFieldValue,
                barConfig.color1b,
                Color.white,
                c => barConfig.color1b = c);

            view.DrawLabel("散乱色1", 200, 20);

            updateTransform |= view.DrawColor(
                _color1cFieldValue,
                barConfig.color1c,
                Color.white,
                c => barConfig.color1c = c);
            
            view.DrawLabel("中心色2", 200, 20);

            updateTransform |= view.DrawColor(
                _color2aFieldValue,
                barConfig.color2a,
                Color.white,
                c => barConfig.color2a = c);

            view.DrawLabel("縁色2", 200, 20);
            
            updateTransform |= view.DrawColor(
                _color2bFieldValue,
                barConfig.color2b,
                Color.white,
                c => barConfig.color2b = c);

            view.DrawLabel("散乱色2", 200, 20);

            updateTransform |= view.DrawColor(
                _color2cFieldValue,
                barConfig.color2c,
                Color.white,
                c => barConfig.color2c = c);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.baseScaleInfo,
                barConfig.baseScale,
                y => barConfig.baseScale = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.widthInfo,
                barConfig.width,
                y => barConfig.width = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.heightInfo,
                barConfig.height,
                y => barConfig.height = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.positionYInfo,
                barConfig.positionY,
                y => barConfig.positionY = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.radiusInfo,
                barConfig.radius,
                y => barConfig.radius = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.topThresholdInfo,
                barConfig.topThreshold,
                y => barConfig.topThreshold = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.cutoffAlphaInfo,
                barConfig.cutoffAlpha,
                y => barConfig.cutoffAlpha = y);

            if (updateTransform)
            {
                controller.Refresh();
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawPsylliumHandConfigEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            var controllers = psylliumManager.controllers;
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

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var handConfig = controller.handConfig;
            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumHand.defaultTrans;
            var defaultConfig = TransformDataPsylliumHand.defaultConfig;

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.handSpacingInfo,
                handConfig.handSpacing,
                y => handConfig.handSpacing = y);
            
            var transformCache = view.GetTransformCache(null);

            view.DrawLabel("サイリウム間の位置", 200, 20);

            {
                var initialPosition = defaultConfig.barOffsetPosition;
                transformCache.position = handConfig.barOffsetPosition;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    handConfig.barOffsetPosition = transformCache.position;
                }
            }

            view.DrawLabel("サイリウム間の角度", 200, 20);

            {
                var initialEulerAngles = defaultConfig.barOffsetRotation;
                var prevEulerAngles = Vector3.zero;
                transformCache.eulerAngles = handConfig.barOffsetRotation;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    handConfig.barOffsetRotation = transformCache.eulerAngles;
                }
            }

            if (updateTransform)
            {
                controller.Refresh();
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public void DrawPsylliumAnimationConfigEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            var controllers = psylliumManager.controllers;
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

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var animationConfig = controller.animationConfig;
            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumAnimation.defaultTrans;
            var defaultConfig = TransformDataPsylliumAnimation.defaultConfig;

            _handTabType = view.DrawTabs(_handTabType, 50, 20);

            DrawPsylliumAnimationHandConfigEdit(view);

            view.DrawLabel("ランダム位置1", 200, 20);

            {
                var initialPosition = defaultConfig.randomPosition1Range;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = animationConfig.randomPosition1Range;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    animationConfig.randomPosition1Range = transformCache.position;
                }
            }

            view.DrawLabel("ランダム位置2", 200, 20);

            {
                var initialPosition = defaultConfig.randomPosition2Range;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = animationConfig.randomPosition2Range;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    animationConfig.randomPosition2Range = transformCache.position;
                }
            }

            view.DrawLabel("ランダム角度", 200, 20);

            {
                var initialEulerAngles = defaultConfig.randomEulerAnglesRange;
                var transformCache = view.GetTransformCache(null);
                var prevEulerAngles = Vector3.zero;
                transformCache.eulerAngles = animationConfig.randomEulerAnglesRange;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    animationConfig.randomEulerAnglesRange = transformCache.eulerAngles;
                }
            }

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.positionSyncRateInfo,
                animationConfig.positionSyncRate,
                y => animationConfig.positionSyncRate = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.bpmInfo,
                animationConfig.bpm,
                y => animationConfig.bpm = y);
            
            updateTransform |= view.DrawCustomValueInt(
                defaultTrans.patternCountInfo,
                animationConfig.patternCount,
                y => animationConfig.patternCount = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.randomTimeInfo,
                animationConfig.randomTime,
                y => animationConfig.randomTime = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.timeRatioInfo,
                animationConfig.timeRatio,
                y => animationConfig.timeRatio = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.timeOffsetInfo,
                animationConfig.timeOffset,
                y => animationConfig.timeOffset = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.timeShiftMinInfo,
                animationConfig.timeShiftMin,
                y => animationConfig.timeShiftMin = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.timeShiftMaxInfo,
                animationConfig.timeShiftMax,
                y => animationConfig.timeShiftMax = y);

            _easingType1ComboBox.currentIndex = (int)animationConfig.easingType1;
            _easingType1ComboBox.onSelected = (type, _) =>
            {
                animationConfig.easingType1 = type;
                updateTransform = true;
            };
            _easingType1ComboBox.DrawButton("イージング1", view);

            _easingType2ComboBox.currentIndex = (int)animationConfig.easingType2;
            _easingType2ComboBox.onSelected = (type, _) =>
            {
                animationConfig.easingType2 = type;
                updateTransform = true;
            };
            _easingType2ComboBox.DrawButton("イージング2", view);

            updateTransform |= view.DrawCustomValueIntRandom(
                defaultTrans.randomSeedInfo,
                animationConfig.randomSeed,
                y => animationConfig.randomSeed = y);

            if (updateTransform)
            {
                controller.ManualUpdate(this.playingTime);
            }

            view.DrawHorizontalLine(Color.gray);

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawToggle("編集中のアニメ再生", config.psylliumEditUpdate, 200, 20, newValue =>
            {
                config.psylliumEditUpdate = newValue;
            });

            view.EndScrollView();
        }

        public void DrawPsylliumAnimationHandConfigEdit(GUIView view)
        {
            var controller = _controllerComboBox.currentItem;
            if (controller == null)
            {
                view.DrawLabel("コントローラーを選択してください", 200, 20);
                return;
            }

            var animationConfig = _handTabType == HandTabType.右手
                ? controller.animationHandConfigRight
                : controller.animationHandConfigLeft;
            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumAnimationHand.defaultTrans;
            var defaultConfig = TransformDataPsylliumAnimationHand.defaultConfig;

            view.DrawLabel("移動1", 200, 20);

            {
                var initialPosition = defaultConfig.position1;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = animationConfig.position1;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    animationConfig.position1 = transformCache.position;
                }
            }

            view.DrawLabel("移動2", 200, 20);

            {
                var initialPosition = defaultConfig.position2;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = animationConfig.position2;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    animationConfig.position2 = transformCache.position;
                }
            }

            view.DrawLabel("回転1", 200, 20);

            {
                var initialEulerAngles = defaultConfig.eulerAngles1;
                var prevEulerAngles = Vector3.zero;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = animationConfig.eulerAngles1;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    animationConfig.eulerAngles1 = transformCache.eulerAngles;
                }
            }

            view.DrawLabel("回転2", 200, 20);

            {
                var initialEulerAngles = defaultConfig.eulerAngles2;
                var prevEulerAngles = Vector3.zero;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = animationConfig.eulerAngles2;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    animationConfig.eulerAngles2 = transformCache.eulerAngles;
                }
            }

            if (updateTransform)
            {
                if (_handTabType == HandTabType.両手)
                {
                    controller.animationHandConfigRight.CopyFrom(animationConfig);
                }

                controller.ManualUpdate(this.playingTime);
            }

            view.DrawHorizontalLine(Color.gray);
        }

        public void DrawPsylliumAreaEdit(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            var controllers = psylliumManager.controllers;
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

                view.DrawLabel("エリア数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = controller.areas.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    psylliumManager.RemoveArea(controller.groupIndex, true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    psylliumManager.AddArea(controller.groupIndex, true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            var areas = controller.areas;
            if (areas.Count == 0)
            {
                view.DrawLabel("エリアが存在しません", 200, 20);
                return;
            }

            _areaComboBox.items = areas;
            _areaComboBox.DrawButton("操作対象", view);

            var area = _areaComboBox.currentItem;

            if (area == null || area.transform == null)
            {
                view.DrawLabel("エリアを選択してください", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawLabel(area.displayName, 200, 20);

            var areaConfig = area.areaConfig;
            var transformCache = view.GetTransformCache();
            var defaultTrans = TransformDataPsylliumArea.defaultTrans;
            transformCache.position = areaConfig.position;
            transformCache.eulerAngles = areaConfig.rotation;
            var initialPosition = defaultTrans.initialPosition;
            var initialEulerAngles = defaultTrans.initialEulerAngles;
            var initialScale = Vector3.one;
            var updateTransform = false;
            var editType = TransformEditType.全て;

            updateTransform |= view.DrawToggle("表示", areaConfig.visible, 120, 20, newValue =>
            {
                areaConfig.visible = newValue;
            });

            updateTransform |= DrawPosition(view, transformCache, editType, initialPosition);
            if (updateTransform)
            {
                areaConfig.position = transformCache.position;
            }

            updateTransform |= DrawEulerAngles(view, transformCache, editType, area.name, initialEulerAngles);
            if (updateTransform)
            {
                areaConfig.rotation = transformCache.eulerAngles;
            }

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.sizeXInfo,
                areaConfig.size.x,
                x => areaConfig.size.x = x);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.sizeYInfo,
                areaConfig.size.y,
                y => areaConfig.size.y = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.seatDistanceXInfo,
                areaConfig.seatDistance.x,
                x => areaConfig.seatDistance.x = x);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.seatDistanceYInfo,
                areaConfig.seatDistance.y,
                y => areaConfig.seatDistance.y = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.randomPositionRangeXInfo,
                areaConfig.randomPositionRange.x,
                x => areaConfig.randomPositionRange.x = x);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.randomPositionRangeYInfo,
                areaConfig.randomPositionRange.y,
                y => areaConfig.randomPositionRange.y = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.randomPositionRangeZInfo,
                areaConfig.randomPositionRange.z,
                z => areaConfig.randomPositionRange.z = z);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.barCountWeight0Info,
                areaConfig.barCountWeight0,
                y => areaConfig.barCountWeight0 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.barCountWeight1Info,
                areaConfig.barCountWeight1,
                y => areaConfig.barCountWeight1 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.barCountWeight2Info,
                areaConfig.barCountWeight2,
                y => areaConfig.barCountWeight2 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.barCountWeight3Info,
                areaConfig.barCountWeight3,
                y => areaConfig.barCountWeight3 = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.colorWeight1Info,
                areaConfig.colorWeight1,
                y => areaConfig.colorWeight1 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.colorWeight2Info,
                areaConfig.colorWeight2,
                y => areaConfig.colorWeight2 = y);

            updateTransform |= view.DrawCustomValueIntRandom(
                defaultTrans.randomSeedInfo,
                areaConfig.randomSeed,
                y => areaConfig.randomSeed = y);

            if (updateTransform)
            {
                area.Refresh();
            }

            view.DrawHorizontalLine(Color.gray);

            {
                _copyToAreaComboBox.items = areas;
                _copyToAreaComboBox.DrawButton("コピー先", view);

                view.BeginHorizontal();
                {
                    var copyToArea = _copyToAreaComboBox.currentItem;

                    if (view.DrawButton("コピー", 60, 20))
                    {
                        if (copyToArea != null && copyToArea != area)
                        {
                            copyToArea.CopyFrom(area, config.psylliumAreaCopyIgnoreTransform);
                        }
                    }

                    if (view.DrawButton("全エリアにコピー", 120, 20))
                    {
                        foreach (var a in areas)
                        {
                            if (a != area)
                            {
                                a.CopyFrom(area, config.psylliumAreaCopyIgnoreTransform);
                            }
                        }
                    }
                }
                view.EndLayout();

                view.DrawToggle("位置/角度/サイズを反映しない", config.psylliumAreaCopyIgnoreTransform, 200, 20, newValue =>
                {
                    config.psylliumAreaCopyIgnoreTransform = newValue;
                });

                view.DrawHorizontalLine(Color.gray);
                view.AddSpace(5);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            if (name.StartsWith("PsylliumController", StringComparison.Ordinal))
            {
                return TransformType.PsylliumController;
            }
            else if (name.StartsWith("PsylliumBar", StringComparison.Ordinal))
            {
                return TransformType.PsylliumBar;
            }
            else if (name.StartsWith("PsylliumHand", StringComparison.Ordinal))
            {
                return TransformType.PsylliumHand;
            }
            else if (name.StartsWith("PsylliumAnimation", StringComparison.Ordinal))
            {
                return TransformType.PsylliumAnimation;
            }
            else
            {
                return TransformType.PsylliumArea;
            }
        }
    }
}