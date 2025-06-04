using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("サイリウム", 44)]
    public class PsylliumTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(PsylliumTimelineLayer);
        public override string layerName => nameof(PsylliumTimelineLayer);

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
                        psylliumManager.areaNames.Count +
                        psylliumManager.patternConfigNames.Count +
                        psylliumManager.transformConfigNames.Count);

                    _allBoneNames.AddRange(psylliumManager.controllerNames);
                    _allBoneNames.AddRange(psylliumManager.barConfigNames);
                    _allBoneNames.AddRange(psylliumManager.handConfigNames);
                    _allBoneNames.AddRange(psylliumManager.areaNames);
                    _allBoneNames.AddRange(psylliumManager.patternConfigNames);
                    _allBoneNames.AddRange(psylliumManager.transformConfigNames);
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

            PsylliumManager.onControllerAdded += OnControllerAdded;
            PsylliumManager.onControllerRemoved += OnControllerRemoved;
            PsylliumManager.onAreaAdded += OnAreaAdded;
            PsylliumManager.onAreaRemoved += OnAreaRemoved;
            PsylliumManager.onPatternAdded += OnPatternAdded;
            PsylliumManager.onPatternRemoved += OnPatternRemoved;
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
            }

            var areaSetMenuItem = new BoneSetMenuItem("PsylliumArea", "エリア");
            allMenuItems.Add(areaSetMenuItem);

            foreach (var area in psylliumManager.areas)
            {
                var menuItem = new BoneMenuItem(area.name, area.displayName);
                areaSetMenuItem.AddChild(menuItem);
            }

            var patternSetMenuItem = new BoneSetMenuItem("PsylliumPattern", "パターン");
            allMenuItems.Add(patternSetMenuItem);

            var transformSetMenuItem = new BoneSetMenuItem("PsylliumTransform", "移動回転");
            allMenuItems.Add(transformSetMenuItem);

            foreach (var pattern in psylliumManager.patterns)
            {
                var menuItem = new BoneMenuItem(pattern.patternConfig.name, pattern.patternConfig.displayName);
                patternSetMenuItem.AddChild(menuItem);

                var menuItem2 = new BoneMenuItem(pattern.transformConfig.name, pattern.transformConfig.displayName);
                transformSetMenuItem.AddChild(menuItem2);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            PsylliumManager.onControllerAdded -= OnControllerAdded;
            PsylliumManager.onControllerRemoved -= OnControllerRemoved;
            PsylliumManager.onAreaAdded -= OnAreaAdded;
            PsylliumManager.onAreaRemoved -= OnAreaRemoved;
            PsylliumManager.onPatternAdded -= OnPatternAdded;
            PsylliumManager.onPatternRemoved -= OnPatternRemoved;
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

            ApplyPlayDataByType(TransformType.PsylliumController);
            ApplyPlayDataByType(TransformType.PsylliumArea);
            ApplyPlayDataByType(TransformType.PsylliumBar);
            ApplyPlayDataByType(TransformType.PsylliumHand);
            ApplyPlayDataByType(TransformType.PsylliumPattern);
            ApplyPlayDataByType(TransformType.PsylliumTransform);

            ApplyTempPlayData();

            var playingTime = this.playingTime;
            foreach (var controller in psylliumManager.controllers)
            {
                controller.ManualUpdate(playingTime);
            }
        }

        protected Dictionary<string, MotionPlayData> _tempPlayDataMap = new Dictionary<string, MotionPlayData>(32);

        protected override void BuildPlayData()
        {
            base.BuildPlayData();

            foreach (var playData in _tempPlayDataMap.Values)
            {
                playData.ResetIndex();
                playData.motions = null;
            }

            foreach (var pair in _playDataMap)
            {
                var name = pair.Key;
                var playData = pair.Value;

                if (playData.motions.Count == 0)
                {
                    continue;
                }

                MotionPlayData tempPlayData;
                if (!_tempPlayDataMap.TryGetValue(name, out tempPlayData))
                {
                    tempPlayData = new MotionPlayData(playData.motions.Count);
                    _tempPlayDataMap[name] = tempPlayData;
                }

                tempPlayData.motions = playData.motions;
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated, MotionPlayData playData)
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
                case TransformType.PsylliumArea:
                    if (indexUpdated)
                    {
                        ApplyAreaMotionInit(motion, t);
                    }
                    break;
                case TransformType.PsylliumPattern:
                    if (indexUpdated)
                    {
                        ApplyPatternMotionInit(motion, t);
                    }
                    ApplyPatternMotionUpdate(motion, t);
                    break;
                case TransformType.PsylliumTransform:
                    if (indexUpdated)
                    {
                        ApplyTransformMotionInit(motion, t);
                    }
                    ApplyTransformMotionUpdate(motion, t);
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

        private void ApplyPatternMotionInit(MotionData motion, float t)
        {
            var patternConfig = psylliumManager.GetPatternConfig(motion.name);
            if (patternConfig == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumPattern;
            var targetConfig = start.ToConfig();

            if (targetConfig.Equals(patternConfig))
            {
                return;
            }

            patternConfig.CopyFrom(targetConfig);
        }

        private void ApplyPatternMotionUpdate(MotionData motion, float t)
        {
            var patternConfig = psylliumManager.GetPatternConfig(motion.name);
            if (patternConfig == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumPattern;
            var end = motion.end as TransformDataPsylliumPattern;

            var startConfig = start.ToConfig();
            var endConfig = end.ToConfig();

            if (startConfig.randomPositionRange != endConfig.randomPositionRange)
            {
                patternConfig.randomPositionRange = Vector3.Lerp(startConfig.randomPositionRange, endConfig.randomPositionRange, t);
            }

            if (startConfig.randomEulerAnglesRange != endConfig.randomEulerAnglesRange)
            {
                patternConfig.randomEulerAnglesRange = Vector3.Lerp(startConfig.randomEulerAnglesRange, endConfig.randomEulerAnglesRange, t);
            }
        }

        private void ApplyTransformMotionInit(MotionData motion, float t)
        {
            var transformConfig = psylliumManager.GetTransformConfig(motion.name);
            if (transformConfig == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumTransform;
            var targetConfig = start.ToConfig();

            if (targetConfig.Equals(transformConfig))
            {
                return;
            }

            transformConfig.CopyFrom(targetConfig);
        }

        private void ApplyTransformMotionUpdate(MotionData motion, float t)
        {
            var transformConfig = psylliumManager.GetTransformConfig(motion.name);
            if (transformConfig == null)
            {
                return;
            }

            var pattern = psylliumManager.GetPattern(transformConfig.groupIndex, transformConfig.patternIndex);
            if (pattern == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumTransform;
            var end = motion.end as TransformDataPsylliumTransform;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            if (start.position != end.position)
            {
                transformConfig.positionLeft = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    t);
            }

            if (start.subPosition != end.subPosition)
            {
                transformConfig.positionRight = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.subPositionValues,
                    end.subPositionValues,
                    t);
            }

            if (start.eulerAngles != end.eulerAngles)
            {
                transformConfig.eulerAnglesLeft = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.eulerAnglesValues,
                    end.eulerAnglesValues,
                    t);
            }

            if (start.subEulerAngles != end.subEulerAngles)
            {
                transformConfig.eulerAnglesRight = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.subEulerAnglesValues,
                    end.subEulerAnglesValues,
                    t);
            }
        }

        private void ApplyTempPlayData()
        {
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return;
            }

            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var pattern in psylliumManager.patterns)
            {
                var patternConfig = pattern.patternConfig;
                var transformConfig = pattern.transformConfig;
                var name = transformConfig.name;

                var timeRange = patternConfig.timeRange;
                var timeCount = patternConfig.timeCount;
                var deltaFrameNo = timeRange / timeCount * timeline.frameRate;
                var tempFrameNoFloat = playingFrameNoFloat - deltaFrameNo * timeCount * 0.5f;

                pattern.ClearTransformData();

                for (var i = 0; i < timeCount; i++)
                {
                    tempFrameNoFloat += deltaFrameNo;
                    ApplyTempPlayDataByName(name, tempFrameNoFloat);
                }
            }
        }

        private void ApplyTempPlayDataByName(
            string name,
            float playingFrameNoFloat)
        {
            var playData = _tempPlayDataMap.GetOrDefault(name);
            if (playData == null || playData.motions.Count == 0)
            {
                return;
            }

            playingFrameNoFloat = Mathf.Max(0, playingFrameNoFloat);

            var indexUpdated = playData.Update(playingFrameNoFloat);

            var current = playData.current;
            if (current != null)
            {
                ApplyTempMotion(current, playData.lerpFrame, indexUpdated);
            }
        }

        private void ApplyTempMotion(MotionData motion, float t, bool indexUpdated)
        {
            switch (motion.start.type)
            {
                case TransformType.PsylliumTransform:
                    if (indexUpdated)
                    {
                        ApplyTransformTempMotionInit(motion, t);
                    }
                    ApplyTransformTempMotionUpdate(motion, t);
                    break;
            }
        }

        private Dictionary<string, PsylliumTransformConfig> _tempTransformConfigMap = new Dictionary<string, PsylliumTransformConfig>(32);

        private void ApplyTransformTempMotionInit(MotionData motion, float t)
        {
            var transformConfig = _tempTransformConfigMap.GetOrCreate(motion.name);
            if (transformConfig == null)
            {
                return;
            }

            var start = motion.start as TransformDataPsylliumTransform;
            var targetConfig = start.ToConfig();
            transformConfig.CopyFrom(targetConfig);

            var sourceConfig = psylliumManager.GetTransformConfig(motion.name);
            transformConfig.groupIndex = sourceConfig.groupIndex;
            transformConfig.patternIndex = sourceConfig.patternIndex;
        }

        private void ApplyTransformTempMotionUpdate(MotionData motion, float t)
        {
            var transformConfig = _tempTransformConfigMap.GetOrCreate(motion.name);
            if (transformConfig == null)
            {
                return;
            }

            var pattern = psylliumManager.GetPattern(transformConfig.groupIndex, transformConfig.patternIndex);
            if (pattern == null)
            {
                MTEUtils.LogDebug("ApplyTransformTempMotionUpdate: pattern is null");
                return;
            }

            var start = motion.start as TransformDataPsylliumTransform;
            var end = motion.end as TransformDataPsylliumTransform;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            if (start.position != end.position)
            {
                transformConfig.positionLeft = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    t);
            }

            if (start.subPosition != end.subPosition)
            {
                transformConfig.positionRight = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.subPositionValues,
                    end.subPositionValues,
                    t);
            }

            if (start.eulerAngles != end.eulerAngles)
            {
                transformConfig.eulerAnglesLeft = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.eulerAnglesValues,
                    end.eulerAnglesValues,
                    t);
            }

            if (start.subEulerAngles != end.subEulerAngles)
            {
                transformConfig.eulerAnglesRight = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.subEulerAnglesValues,
                    end.subEulerAnglesValues,
                    t);
            }

            pattern.ApplyTransformData(transformConfig);
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
            AddFirstBones(allBoneNames);
            ApplyCurrentFrame(true);
        }

        public void OnAreaRemoved(string areaName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { areaName });
            ApplyCurrentFrame(true);
        }

        public void OnPatternAdded(string patternName)
        {
            InitMenuItems();
            AddFirstBones(allBoneNames);
            ApplyCurrentFrame(true);
        }

        public void OnPatternRemoved(string patternName)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { patternName });
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
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

            foreach (var pattern in psylliumManager.patterns)
            {
                if (pattern == null || pattern.controller == null)
                {
                    continue;
                }

                {
                    var patternConfig = pattern.patternConfig;
                    var patternName = patternConfig.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumPattern>(patternName);
                    trans.FromConfig(patternConfig);
                }

                {
                    var transformConfig = pattern.transformConfig;
                    var transformName = transformConfig.name;

                    var trans = frame.GetOrCreateTransformData<TransformDataPsylliumTransform>(transformName);
                    trans.FromConfig(transformConfig);
                }
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

        private GUIComboBox<PsylliumPattern> _patternComboBox = new GUIComboBox<PsylliumPattern>
        {
            getName = (pattern, index) => pattern.patternConfig.displayName,
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

        private GUIComboBox<PsylliumPattern> _copyToPatternComboBox = new GUIComboBox<PsylliumPattern>
        {
            getName = (area, index) => area.patternConfig.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<PsylliumPattern> _copyToTransformComboBox = new GUIComboBox<PsylliumPattern>
        {
            getName = (area, index) => area.transformConfig.displayName,
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

        private static TabType _tabType = TabType.基本;
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
                    DrawPsylliumPatternConfigEdit(view);
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

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumController.defaultTrans;
            var transformCache = view.GetTransformCache(null);

            updateTransform |= view.DrawToggle(controller.displayName, controller.visible, 200, 20, newValue =>
            {
                controller.visible = newValue;
            });

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

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

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

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

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

        public void DrawPsylliumPatternConfigEdit(GUIView view)
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

                view.DrawLabel("パターン数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = controller.patterns.Count,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    psylliumManager.RemovePattern(controller.groupIndex, true);
                }
                if (view.DrawButton("+", 20, 20))
                {
                    psylliumManager.AddPattern(controller.groupIndex, true);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            var patterns = controller.patterns;
            if (patterns.Count == 0)
            {
                view.DrawLabel("パターンが存在しません", 200, 20);
                return;
            }

            _patternComboBox.items = patterns;
            _patternComboBox.DrawButton("操作対象", view);

            var pattern = _patternComboBox.currentItem;

            if (pattern == null)
            {
                view.DrawLabel("パターンを選択してください", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var patternConfig = pattern.patternConfig;
            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumPattern.defaultTrans;
            var defaultConfig = TransformDataPsylliumPattern.defaultConfig;

            _handTabType = view.DrawTabs(_handTabType, 50, 20);

            DrawPsylliumTransformConfigEdit(view);

            view.DrawLabel("ランダム位置", 200, 20);

            {
                var initialPosition = defaultConfig.randomPositionRange;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = patternConfig.randomPositionRange;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    patternConfig.randomPositionRange = transformCache.position;
                }
            }

            view.DrawLabel("ランダム角度", 200, 20);

            {
                var initialEulerAngles = defaultConfig.randomEulerAnglesRange;
                var transformCache = view.GetTransformCache(null);
                var prevEulerAngles = Vector3.zero;
                transformCache.eulerAngles = patternConfig.randomEulerAnglesRange;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    patternConfig.randomEulerAnglesRange = transformCache.eulerAngles;
                }
            }
            
            updateTransform |= view.DrawCustomValueInt(
                defaultTrans.timeCountInfo,
                patternConfig.timeCount,
                y => patternConfig.timeCount = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.timeRangeInfo,
                patternConfig.timeRange,
                y => patternConfig.timeRange = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.timeShiftMinInfo,
                patternConfig.timeShiftMin,
                y => patternConfig.timeShiftMin = y);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.timeShiftMaxInfo,
                patternConfig.timeShiftMax,
                y => patternConfig.timeShiftMax = y);

            updateTransform |= view.DrawCustomValueIntRandom(
                defaultTrans.randomSeedInfo,
                patternConfig.randomSeed,
                y => patternConfig.randomSeed = y);

            if (updateTransform)
            {
                controller.ManualUpdate(playingTime);
            }

            {
                _copyToPatternComboBox.items = controller.patterns;
                _copyToPatternComboBox.DrawButton("コピー先", view);

                var copyToPattern = _copyToPatternComboBox.currentItem;

                if (view.DrawButton("コピー", 60, 20))
                {
                    if (copyToPattern != null && copyToPattern != pattern)
                    {
                        copyToPattern.patternConfig.CopyFrom(patternConfig);
                        controller.ManualUpdate(playingTime);
                    }
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.EndScrollView();
        }

        public void DrawPsylliumTransformConfigEdit(GUIView view)
        {
            var controller = _controllerComboBox.currentItem;
            if (controller == null)
            {
                view.DrawLabel("コントローラーを選択してください", 200, 20);
                return;
            }

            var pattern = _patternComboBox.currentItem;
            if (pattern == null)
            {
                view.DrawLabel("パターンを選択してください", 200, 20);
                return;
            }

            var transformConfig = pattern.transformConfig;
            var updateTransform = false;
            var defaultTrans = TransformDataPsylliumTransform.defaultTrans;
            var defaultConfig = TransformDataPsylliumTransform.defaultConfig;

            view.DrawLabel("移動", 200, 20);

            if (_handTabType == HandTabType.右手)
            {
                var initialPosition = defaultConfig.positionRight;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = transformConfig.positionRight;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    transformConfig.positionRight = transformCache.position;
                }
            }
            else
            {
                var initialPosition = defaultConfig.positionLeft;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = transformConfig.positionLeft;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    transformConfig.positionLeft = transformCache.position;
                }
            }

            view.DrawLabel("回転", 200, 20);

            if (_handTabType == HandTabType.右手)
            {
                var initialEulerAngles = defaultConfig.eulerAnglesRight;
                var prevEulerAngles = Vector3.zero;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = transformConfig.eulerAnglesRight;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    transformConfig.eulerAnglesRight = transformCache.eulerAngles;
                }
            }
            else
            {
                var initialEulerAngles = defaultConfig.eulerAnglesLeft;
                var prevEulerAngles = Vector3.zero;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = transformConfig.eulerAnglesLeft;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    transformConfig.eulerAnglesLeft = transformCache.eulerAngles;
                }
            }

            if (updateTransform)
            {
                if (_handTabType == HandTabType.両手)
                {
                    transformConfig.positionRight = transformConfig.positionLeft;
                    transformConfig.eulerAnglesRight = transformConfig.eulerAnglesLeft;
                }

                pattern.ClearTransformData();
                pattern.ApplyTransformData(transformConfig);
                controller.ManualUpdate(playingTime);
            }

            {
                _copyToTransformComboBox.items = controller.patterns;
                _copyToTransformComboBox.DrawButton("コピー先", view);

                var copyToPattern = _copyToTransformComboBox.currentItem;

                if (view.DrawButton("コピー", 60, 20))
                {
                    if (copyToPattern != null && copyToPattern != pattern)
                    {
                        copyToPattern.transformConfig.CopyFrom(transformConfig);
                        copyToPattern.ClearTransformData();
                        copyToPattern.ApplyTransformData(transformConfig);
                        controller.ManualUpdate(playingTime);
                    }
                }
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

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

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

            updateTransform |= view.DrawToggle(area.displayName, areaConfig.visible, 200, 20, newValue =>
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

            view.DrawLabel("バー数の重み", 200, 20);

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

            view.DrawLabel("色の重み", 200, 20);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.colorWeight1Info,
                areaConfig.colorWeight1,
                y => areaConfig.colorWeight1 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.colorWeight2Info,
                areaConfig.colorWeight2,
                y => areaConfig.colorWeight2 = y);

            view.DrawLabel("パターンの重み", 200, 20);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight0Info,
                areaConfig.patternWeight0,
                y => areaConfig.patternWeight0 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight1Info,
                areaConfig.patternWeight1,
                y => areaConfig.patternWeight1 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight2Info,
                areaConfig.patternWeight2,
                y => areaConfig.patternWeight2 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight3Info,
                areaConfig.patternWeight3,
                y => areaConfig.patternWeight3 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight4Info,
                areaConfig.patternWeight4,
                y => areaConfig.patternWeight4 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight5Info,
                areaConfig.patternWeight5,
                y => areaConfig.patternWeight5 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight6Info,
                areaConfig.patternWeight6,
                y => areaConfig.patternWeight6 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight7Info,
                areaConfig.patternWeight7,
                y => areaConfig.patternWeight7 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight8Info,
                areaConfig.patternWeight8,
                y => areaConfig.patternWeight8 = y);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.patternWeight9Info,
                areaConfig.patternWeight9,
                y => areaConfig.patternWeight9 = y);

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

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
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
            else if (name.StartsWith("PsylliumPattern", StringComparison.Ordinal))
            {
                return TransformType.PsylliumPattern;
            }
            else if (name.StartsWith("PsylliumTransform", StringComparison.Ordinal))
            {
                return TransformType.PsylliumTransform;
            }
            else
            {
                return TransformType.PsylliumArea;
            }
        }
    }
}